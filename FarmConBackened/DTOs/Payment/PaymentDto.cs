using FarmConBackened.Models.Enum;

namespace FarmConBackened.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? PaymentGateway { get; set; }
        public string? GatewayReference { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
    }
}
