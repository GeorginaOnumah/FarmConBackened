using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required] public string Token { get; set; } = string.Empty;
        [Required, MinLength(8)] public string NewPassword { get; set; } = string.Empty;
    }
}
