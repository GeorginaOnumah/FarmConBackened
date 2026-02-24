using FarmConBackened.Models.Deliveries;
using FarmConnect.Models;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Users
{
    public class TransporterProfile
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string? VehicleType { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? VehicleCapacity { get; set; }
        public bool IsVerified { get; set; } = false;
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public bool IsAvailable { get; set; } = true;
        public double Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public decimal TotalEarnings { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}
