using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Orders;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Payments
{
    public class Payment
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? PaymentGateway { get; set; }
        public string? GatewayReference { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? EscrowReference { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Order Order { get; set; } = null!;
    }

}
