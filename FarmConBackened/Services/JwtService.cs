using FarmConBackened.Interfaces;
using FarmConBackened.Models.Users;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace FarmConBackened.Services
{
    public class JwtService : IJwtService
    {
        private readonly SymmetricSecurityKey _signingKey;
        private readonly IConfiguration _config;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration config)
        {
            var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            _issuer = config["Jwt:Issuer"] ?? "FarmConnect";
            _audience = config["Jwt:Audience"] ?? "FarmConnect";
            _expiryMinutes = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");
        }

        public string GenerateAccessToken(User user)
        {
            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()), // ClaimTypes.Role is standard
            new Claim("firstName", user.FirstName ?? ""),
            new Claim("lastName", user.LastName ?? ""),
            new Claim("accountStatus", user.Status.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string token, DateTime expiry) GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var token = Convert.ToBase64String(bytes);
            var expiry = DateTime.UtcNow.AddDays(30);
            return (token, expiry);
        }

        public Guid? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                return sub != null ? Guid.Parse(sub) : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
