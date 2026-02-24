using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Product
{
    public class UpdateProductDto
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Range(0.01, double.MaxValue)] public decimal? PricePerUnit { get; set; }
        public string? Unit { get; set; }
        [Range(0, double.MaxValue)] public decimal? QuantityAvailable { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
