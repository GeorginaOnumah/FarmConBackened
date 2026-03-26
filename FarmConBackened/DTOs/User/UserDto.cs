using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Enums;

namespace FarmConBackened.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public AccountStatus Status { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
