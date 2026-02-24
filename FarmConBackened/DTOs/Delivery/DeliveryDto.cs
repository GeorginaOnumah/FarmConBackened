using FarmConBackened.Models.Enum;

namespace FarmConBackened.DTOs.Delivery
{
    public class DeliveryDto
    {
        public Guid Id { get; set; }
        public DeliveryStatus Status { get; set; }
        public string? PickupAddress { get; set; }
        public string? DropoffAddress { get; set; }
        public string? TrackingCode { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public string? TransporterName { get; set; }
        public string? TransporterPhone { get; set; }
        public string? VehicleType { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

}
