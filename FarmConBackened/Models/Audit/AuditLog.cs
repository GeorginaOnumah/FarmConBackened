using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Audit
{
    public class AuditLog
    {
        [Key] public long Id { get; set; }
        public Guid? UserId { get; set; }
        [Required, MaxLength(100)] public string Action { get; set; } = string.Empty;
        [MaxLength(100)] public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
