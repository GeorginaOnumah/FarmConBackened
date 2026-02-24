using FarmConBackened.Interfaces;
using FarmConBackened.Models.Deliveries;
using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Payments;
using System;

namespace FarmConBackened.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;
        private readonly IAuditService _audit;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext db, INotificationService notifications,
            IAuditService audit, ILogger<PaymentService> logger)
        {
            _db = db;
            _notifications = notifications;
            _audit = audit;
            _logger = logger;
        }

        public async Task<object> InitiatePaymentAsync(Guid buyerUserId, InitiatePaymentDto dto)
        {
            var order = await _db.Orders
                .Include(o => o.BuyerProfile)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId)
                ?? throw new KeyNotFoundException("Order not found.");

            if (order.BuyerProfile.UserId != buyerUserId)
                throw new UnauthorizedAccessException("Access denied.");

            if (order.Status != OrderStatus.Accepted)
                throw new InvalidOperationException("Order must be accepted before payment.");

            if (order.Payment != null && order.Payment.Status == PaymentStatus.Held)
                throw new InvalidOperationException("Payment already completed for this order.");

            var gatewayRef = $"FC-{order.OrderNumber}-{DateTime.UtcNow.Ticks}";
            var payment = order.Payment ?? new Payment { OrderId = order.Id };
            payment.Amount = order.TotalAmount;
            payment.PaymentGateway = dto.PaymentGateway;
            payment.GatewayReference = gatewayRef;
            payment.Status = PaymentStatus.Pending;
            payment.UpdatedAt = DateTime.UtcNow;

            if (order.Payment == null) _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // In production, call Paystack/Flutterwave SDK here and return their payment URL
            _logger.LogInformation("Payment initiated: OrderId={OrderId}, Ref={Ref}", order.Id, gatewayRef);

            return new
            {
                PaymentReference = gatewayRef,
                Amount = order.TotalAmount,
                OrderNumber = order.OrderNumber,
                Gateway = dto.PaymentGateway,
                Message = "Redirect user to payment gateway. Call /verify after payment.",
                // In production: AuthorizationUrl = paystackResponse.data.authorization_url
            };
        }

        public async Task<PaymentDto> VerifyPaymentAsync(PaymentVerificationDto dto)
        {
            var payment = await _db.Payments
                .Include(p => p.Order).ThenInclude(o => o.FarmerProfile).ThenInclude(f => f.User)
                .Include(p => p.Order).ThenInclude(o => o.BuyerProfile).ThenInclude(b => b.User)
                .FirstOrDefaultAsync(p => p.GatewayReference == dto.GatewayReference && p.OrderId == dto.OrderId)
                ?? throw new KeyNotFoundException("Payment record not found.");

            // In production: verify with gateway using GatewayReference
            // For now, simulate success
            payment.Status = PaymentStatus.Held; // Escrow hold
            payment.PaidAt = DateTime.UtcNow;
            payment.EscrowReference = $"ESC-{Guid.NewGuid():N}";
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Order.Status = OrderStatus.Processing;
            payment.Order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _audit.LogAsync(null, "PAYMENT_RECEIVED", "Payment", payment.Id.ToString());

            await _notifications.SendNotificationAsync(
                payment.Order.FarmerProfile.UserId,
                NotificationType.PaymentUpdate,
                "Payment Received",
                $"Payment of ₦{payment.Amount:N0} for order #{payment.Order.OrderNumber} is held in escrow.",
                payment.OrderId.ToString());

            return MapPaymentDto(payment);
        }

        public async Task<bool> ReleaseEscrowAsync(Guid orderId)
        {
            var payment = await _db.Payments
                .Include(p => p.Order).ThenInclude(o => o.FarmerProfile).ThenInclude(f => f.User)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Held)
                ?? throw new KeyNotFoundException("No held payment found for this order.");

            payment.Status = PaymentStatus.Released;
            payment.ReleasedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.SendNotificationAsync(
                payment.Order.FarmerProfile.UserId,
                NotificationType.PaymentUpdate,
                "Payment Released",
                $"₦{payment.Amount:N0} has been released to your account.",
                orderId.ToString());

            return true;
        }

        public async Task<bool> RefundPaymentAsync(Guid orderId)
        {
            var payment = await _db.Payments
                .Include(p => p.Order).ThenInclude(o => o.BuyerProfile).ThenInclude(b => b.User)
                .FirstOrDefaultAsync(p => p.OrderId == orderId)
                ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.Status == PaymentStatus.Released)
                throw new InvalidOperationException("Cannot refund a payment that has already been released.");

            payment.Status = PaymentStatus.Refunded;
            payment.RefundedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.SendNotificationAsync(
                payment.Order.BuyerProfile.UserId,
                NotificationType.PaymentUpdate,
                "Payment Refunded",
                $"₦{payment.Amount:N0} has been refunded to your account.",
                orderId.ToString());

            return true;
        }

        public async Task<List<PaymentDto>> GetOrderPaymentsAsync(Guid userId, Guid orderId)
        {
            var payments = await _db.Payments
                .Include(p => p.Order).ThenInclude(o => o.BuyerProfile)
                .Include(p => p.Order).ThenInclude(o => o.FarmerProfile)
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            return payments.Select(MapPaymentDto).ToList();
        }

        private static PaymentDto MapPaymentDto(Payment p) => new()
        {
            Id = p.Id,
            Amount = p.Amount,
            Status = p.Status,
            PaymentGateway = p.PaymentGateway,
            GatewayReference = p.GatewayReference,
            PaidAt = p.PaidAt,
            ReleasedAt = p.ReleasedAt
        };
    }

    
}
