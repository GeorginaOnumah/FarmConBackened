using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Enums;
using FarmConBackened.Models;
using System.ComponentModel.DataAnnotations;
using FarmConBackened.Models.Messaging;

namespace FarmConBackened.Models.Users
{
    public class User
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;
        [Phone, MaxLength(20)] public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Pending;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? LGA { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public FarmerProfile? FarmerProfile { get; set; }
        public BuyerProfile? BuyerProfile { get; set; }
        public TransporterProfile? TransporterProfile { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }

}
