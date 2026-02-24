using FarmConBackened.Models.Orders;
using FarmConnect.Models;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Users
{
    public class BuyerProfile
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? BusinessType { get; set; }
        public double Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

}
