namespace FarmConBackened.DTOs.Product
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerUnit { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal QuantityAvailable { get; set; }
        public bool IsAvailable { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public string? FarmName { get; set; }
        public double FarmerRating { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? PrimaryImageUrl { get; set; }
    }
}
