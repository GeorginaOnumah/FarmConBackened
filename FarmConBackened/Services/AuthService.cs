using FarmConBackened.DTOs.Auth;
using FarmConBackened.DTOs.User;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Users;
using FarmConnect.Data;
using FarmConnect.Models;
using FarmConnect.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FarmConBackened.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwt;
        private readonly IAuditService _audit;
        private readonly INotificationService _notifications;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext db, IJwtService jwt, IAuditService audit,
            INotificationService notifications, ILogger<AuthService> logger)
        {
            _db = db;
            _jwt = jwt;
            _audit = audit;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string ipAddress)
        {
            // Check duplicate email
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower()))
                throw new InvalidOperationException("Email already registered.");

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                Address = dto.Address,
                State = dto.State,
                LGA = dto.LGA,
                EmailVerificationToken = Guid.NewGuid().ToString("N"),
                Status = AccountStatus.Pending
            };

            _db.Users.Add(user);

            // Create role-specific profile
            switch (dto.Role)
            {
                case UserRole.Farmer:
                    _db.FarmerProfiles.Add(new FarmerProfile { UserId = user.Id });
                    break;
                case UserRole.Buyer:
                    _db.BuyerProfiles.Add(new BuyerProfile { UserId = user.Id });
                    break;
                case UserRole.Transporter:
                    _db.TransporterProfiles.Add(new TransporterProfile { UserId = user.Id });
                    break;
            }

            await _db.SaveChangesAsync();

            // NOTE: In production, send verification email here
            _logger.LogInformation("User registered: {Email}, VerificationToken: {Token}",
                user.Email, user.EmailVerificationToken);

            await _audit.LogAsync(user.Id, "USER_REGISTERED", "User", user.Id.ToString(), ipAddress: ipAddress);

            // Auto-activate for demo (remove in production)
            user.Status = AccountStatus.Active;
            user.IsEmailVerified = true;
            await _db.SaveChangesAsync();

            return await IssueTokensAsync(user, ipAddress, "System");
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string userAgent)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                await _audit.LogAsync(null, "LOGIN_FAILED", ipAddress: ipAddress);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (user.Status == AccountStatus.Suspended)
                throw new UnauthorizedAccessException("Your account has been suspended.");

            if (!user.IsEmailVerified)
                throw new UnauthorizedAccessException("Please verify your email before logging in.");

            await _audit.LogAsync(user.Id, "LOGIN_SUCCESS", ipAddress: ipAddress, userAgent: userAgent);
            return await IssueTokensAsync(user, ipAddress, userAgent);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var session = await _db.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && !s.IsRevoked);

            if (session == null || session.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            // Rotate refresh token
            session.IsRevoked = true;
            var response = await IssueTokensAsync(session.User, ipAddress, session.UserAgent ?? "");
            await _db.SaveChangesAsync();
            return response;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
            if (session != null)
            {
                session.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
            if (user == null) return false;

            user.IsEmailVerified = true;
            user.Status = AccountStatus.Active;
            user.EmailVerificationToken = null;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
            if (user == null) return; // Silent for security

            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetExpiry = DateTime.UtcNow.AddHours(2);
            await _db.SaveChangesAsync();

            // NOTE: Send email with reset token in production
            _logger.LogInformation("Password reset token for {Email}: {Token}", email, user.PasswordResetToken);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token && u.PasswordResetExpiry > DateTime.UtcNow);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
            user.UpdatedAt = DateTime.UtcNow;

            // Revoke all sessions to force re-login
            var sessions = await _db.UserSessions.Where(s => s.UserId == userId && !s.IsRevoked).ToListAsync();
            sessions.ForEach(s => s.IsRevoked = true);
            await _db.SaveChangesAsync();
        }

        // ── Private ───────────────────────────────────────────────────────

        private async Task<AuthResponseDto> IssueTokensAsync(User user, string ipAddress, string userAgent)
        {
            var accessToken = _jwt.GenerateAccessToken(user);
            var (refreshToken, expiry) = _jwt.GenerateRefreshToken();

            _db.UserSessions.Add(new UserSession
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                ExpiresAt = expiry,
                IpAddress = ipAddress,
                UserAgent = userAgent
            });
            await _db.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = MapUserDto(user)
            };
        }

        private static UserDto MapUserDto(User u) => new()
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role,
            Status = u.Status,
            IsEmailVerified = u.IsEmailVerified,
            ProfileImageUrl = u.ProfileImageUrl,
            Address = u.Address,
            State = u.State,
            CreatedAt = u.CreatedAt
        };
    }
}