using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Payment
{
    public class InitiatePaymentDto
    {
        [Required] public Guid OrderId { get; set; }
        [Required] public string PaymentGateway { get; set; } = "Paystack";
    }
}
