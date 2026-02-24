using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }
}
