using FarmConBackened.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.Models.Messaging
{
    public class Message
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? OrderId { get; set; }
        [Required] public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }
}
