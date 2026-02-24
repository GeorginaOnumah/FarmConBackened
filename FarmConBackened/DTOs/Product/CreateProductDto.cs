using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Product
{
    public class CreateProductDto
    {
        [Required] public int CategoryId { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required, Range(0.01, double.MaxValue)] public decimal PricePerUnit { get; set; }
        [Required] public string Unit { get; set; } = "kg";
        [Required, Range(0, double.MaxValue)] public decimal QuantityAvailable { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Required] public DateTime HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
