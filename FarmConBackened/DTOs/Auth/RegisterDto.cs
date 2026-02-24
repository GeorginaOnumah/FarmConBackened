using FarmConnect.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Auth
{
    public class RegisterDto
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        [Required] public UserRole Role { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? LGA { get; set; }
    }
}
