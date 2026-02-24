using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Messaging
{
    public class Notification
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required] public string Body { get; set; } = string.Empty;
        public string? ReferenceId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public User User { get; set; } = null!;
    }

}
