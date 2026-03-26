using FarmConBackened.Models.Enum;

namespace FarmConBackened.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
