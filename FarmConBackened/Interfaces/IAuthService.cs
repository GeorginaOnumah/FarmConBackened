namespace FarmConBackened.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string ipAddress);
        Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string userAgent);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task LogoutAsync(string refreshToken);
        Task<bool> VerifyEmailAsync(string token);
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}
