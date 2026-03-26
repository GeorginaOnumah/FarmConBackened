using FarmConBackened.Models.Users;
using FarmConBackened.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FarmConBackened.Models.Orders;
using FarmConBackened.Models.Reviews;

namespace FarmConBackened.Models.Products
{
    public class Product
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FarmerProfileId { get; set; }
        public int CategoryId { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal PricePerUnit { get; set; }
        [MaxLength(50)] public string Unit { get; set; } = "kg";
        [Column(TypeName = "decimal(18,2)")] public decimal QuantityAvailable { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public FarmerProfile FarmerProfile { get; set; } = null!;
        public ProductCategory Category { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
