using FarmConBackened.Models.Orders;
using FarmConBackened.Models.Products;
using FarmConBackened.Models;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Users
{
    public class FarmerProfile
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        [MaxLength(200)] public string? FarmName { get; set; }
        public string? FarmDescription { get; set; }
        public string? FarmAddress { get; set; }
        public double? FarmLatitude { get; set; }
        public double? FarmLongitude { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? NINNumber { get; set; }
        public double Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

}
