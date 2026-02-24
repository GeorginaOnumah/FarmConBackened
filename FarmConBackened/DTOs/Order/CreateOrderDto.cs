using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required] public List<CreateOrderItemDto> Items { get; set; } = new();
        [Required] public string DeliveryAddress { get; set; } = string.Empty;
        public double? DeliveryLatitude { get; set; }
        public double? DeliveryLongitude { get; set; }
        public string? Notes { get; set; }
    }
}
