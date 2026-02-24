using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Orders;
using FarmConBackened.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Deliveries
{
    public class Delivery
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid? TransporterProfileId { get; set; }
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Unassigned;
        public string? PickupAddress { get; set; }
        public string? DropoffAddress { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public string? TrackingCode { get; set; }
        public string? Notes { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? TransporterEarning { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Order Order { get; set; } = null!;
        public TransporterProfile? TransporterProfile { get; set; }
    }
}
