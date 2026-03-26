using FarmConBackened.DTOs.Payment;
using FarmConBackened.DTOs.Paystack;
using FarmConBackened.Helpers;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Payments;
using FarmConBackened.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FarmConBackened.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;
        private readonly IAuditService _audit;
        private readonly PaystackHelper _paystack;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            AppDbContext db,
            INotificationService notifications,
            IAuditService audit,
            PaystackHelper paystack,
            ILogger<PaymentService> logger)
        {
            _db = db;
            _notifications = notifications;
            _audit = audit;
            _paystack = paystack;
            _logger = logger;
        }

        public async Task<object> InitiatePaymentAsync(Guid buyerUserId, InitiatePaymentDto dto)
        {
            var order = await _db.Orders
                .Include(o => o.BuyerProfile).ThenInclude(b => b.User)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId)
                ?? throw new KeyNotFoundException("Order not found");

            if (order.BuyerProfile.UserId != buyerUserId)
                throw new UnauthorizedAccessException("Access denied");

            if (order.Status != OrderStatus.Accepted)
                throw new InvalidOperationException("Order must be accepted before payment");

            var reference = $"FC-{order.OrderNumber}-{Guid.NewGuid():N[..6]}";

            var paystackResult = await _paystack.InitializeTransactionAsync(
                order.BuyerProfile.User.Email,
                (long)(order.TotalAmount * 100),
                reference,
                new { orderId = order.Id }
            );

            var payment = order.Payment ?? new Payment { OrderId = order.Id };

            payment.Amount = order.TotalAmount;
            payment.PaymentGateway = "Paystack";
            payment.GatewayReference = reference;
            payment.Status = PaymentStatus.Pending;

            if (order.Payment == null) _db.Payments.Add(payment);

            await _db.SaveChangesAsync();

            return new
            {
                reference,
                paystackResult?.AuthorizationUrl
            };
        }

        public async Task<PaymentDto> VerifyPaymentAsync(PaymentVerificationDto dto)
        {
            var payment = await _db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.GatewayReference == dto.GatewayReference)
                ?? throw new KeyNotFoundException("Payment not found");

            var verify = await _paystack.VerifyTransactionAsync(dto.GatewayReference);

            if (verify == null || verify.Status != "success")
            {
                payment.Status = PaymentStatus.Failed;
                await _db.SaveChangesAsync();
                throw new Exception("Payment verification failed");
            }

            payment.Status = PaymentStatus.Held;
            payment.Order.Status = OrderStatus.Processing;

            await _db.SaveChangesAsync();

            return new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Status = payment.Status
            };
        }

        public async Task<bool> HandleWebhookAsync(string body, string signature)
        {
            if (!_paystack.VerifySignature(body, signature)) return false;

            var payload = JsonSerializer.Deserialize<PaystackWebhookPayload>(body);

            if (payload?.Event == "charge.success")
            {
                var payment = await _db.Payments
                    .FirstOrDefaultAsync(p => p.GatewayReference == payload.Data!.Reference);

                if (payment != null)
                {
                    payment.Status = PaymentStatus.Held;
                    await _db.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> ReleaseEscrowAsync(Guid orderId)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
                throw new Exception("Payment not found");

            payment.Status = PaymentStatus.Released;
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RefundPaymentAsync(Guid orderId)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
                throw new Exception("Payment not found");

            await _paystack.RefundAsync(
                payment.GatewayTransactionId!,
                (long)(payment.Amount * 100)
            );

            payment.Status = PaymentStatus.Refunded;
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<PaymentDto>> GetOrderPaymentsAsync(Guid userId, Guid orderId)
        {
            return await _db.Payments
                .Where(p => p.OrderId == orderId)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Status = p.Status
                })
                .ToListAsync();
        }
    }
}