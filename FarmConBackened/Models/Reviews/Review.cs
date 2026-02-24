using FarmConBackened.Models.Products;
using FarmConBackened.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Reviews
{
    public class Review
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReviewerId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ReviewedUserId { get; set; }
        public Guid OrderId { get; set; }
        [Range(1, 5)] public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Reviewer { get; set; } = null!;
        public Product? Product { get; set; }
    }

}
