using FarmConBackened.Models.Products;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Orders
{
    public class OrderItem
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalPrice { get; set; }
        public string Unit { get; set; } = "kg";
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}