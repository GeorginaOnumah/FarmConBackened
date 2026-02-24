using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Products
{
    public class ProductImage
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public bool IsPrimary { get; set; } = false;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Product Product { get; set; } = null!;
    }
}
