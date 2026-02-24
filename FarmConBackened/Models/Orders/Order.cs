using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FarmConBackened.Models.Payments;
using FarmConBackened.Models.Deliveries;

namespace FarmConBackened.Models.Orders
{
    public class Order
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BuyerProfileId { get; set; }
        public Guid FarmerProfileId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal DeliveryFee { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public double? DeliveryLatitude { get; set; }
        public double? DeliveryLongitude { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public BuyerProfile BuyerProfile { get; set; } = null!;
        public FarmerProfile FarmerProfile { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
        public Delivery? Delivery { get; set; }
    }
}
