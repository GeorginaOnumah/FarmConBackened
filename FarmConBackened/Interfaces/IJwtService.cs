using FarmConBackened.Models.Users;

namespace FarmConBackened.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        (string token, DateTime expiry) GenerateRefreshToken();
        Guid? ValidateToken(string token);
    }
}
