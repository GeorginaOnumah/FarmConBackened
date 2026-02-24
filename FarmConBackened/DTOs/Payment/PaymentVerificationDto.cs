using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Payment
{
    public class PaymentVerificationDto
    {
        [Required] public string GatewayReference { get; set; } = string.Empty;
        [Required] public Guid OrderId { get; set; }
    }
}
