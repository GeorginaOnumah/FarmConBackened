using FarmConBackened.DTOs.Delivery;
using FarmConBackened.DTOs.Payment;
using FarmConBackened.Models.Enum;

namespace FarmConBackened.DTOs.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public PaymentDto? Payment { get; set; }
        public DeliveryDto? Delivery { get; set; }
    }
}
