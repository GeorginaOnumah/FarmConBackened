using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Order
{
    public class CreateOrderItemDto
    {
        [Required] public Guid ProductId { get; set; }
        [Required, Range(0.01, double.MaxValue)] public decimal Quantity { get; set; }
    }

}
