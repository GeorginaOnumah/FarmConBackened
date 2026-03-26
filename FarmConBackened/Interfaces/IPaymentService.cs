using FarmConBackened.DTOs.Payment;

namespace FarmConBackened.Interfaces
{
    public interface IPaymentService
    {
        Task<object> InitiatePaymentAsync(Guid buyerUserId, InitiatePaymentDto dto);
        Task<PaymentDto> VerifyPaymentAsync(PaymentVerificationDto dto);
        Task<bool> ReleaseEscrowAsync(Guid orderId);
        Task<bool> RefundPaymentAsync(Guid orderId);
        Task<List<PaymentDto>> GetOrderPaymentsAsync(Guid userId, Guid orderId);
        Task<bool> HandleWebhookAsync(string rawBody, string paystackSignature);
    }
}
