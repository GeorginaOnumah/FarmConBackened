using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Admin
{
    public class CreateMarketPriceDto
    {
        [Required] public string CropName { get; set; } = string.Empty;
        [Required, Range(0.01, double.MaxValue)] public decimal PricePerKg { get; set; }
        [Required] public string Market { get; set; } = string.Empty;
        public string? State { get; set; }
    }
}
