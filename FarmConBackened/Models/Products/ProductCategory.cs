using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Products
{
    public class ProductCategory
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
