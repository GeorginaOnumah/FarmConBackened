using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Products
{
    public class MarketPrice
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(100)] public string CropName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")] public decimal PricePerKg { get; set; }
        [MaxLength(100)] public string Market { get; set; } = string.Empty;
        public string? State { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public Guid UpdatedByAdminId { get; set; }
    }

}
