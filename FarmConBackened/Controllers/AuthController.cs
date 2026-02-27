using FarmConBackened.DTOs.Auth;
using FarmConBackened.Helpers.Responses;
using FarmConBackened.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>Register a new user (Farmer, Buyer, Transporter)</summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.RegisterAsync(dto, GetIpAddress());
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful."));
        }

        /// <summary>Login and receive JWT tokens</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.LoginAsync(dto, GetIpAddress(), GetUserAgent());
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken, GetIpAddress());
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            await _authService.LogoutAsync(dto.RefreshToken);
            return Ok(ApiResponse.Ok("Logged out successfully."));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            return result ? Ok(ApiResponse.Ok("Email verified.")) : BadRequest(ApiResponse.Fail("Invalid token."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(ApiResponse.Ok("If the email exists, a reset link has been sent."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return result ? Ok(ApiResponse.Ok("Password reset successful.")) : BadRequest(ApiResponse.Fail("Invalid or expired token."));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            await _authService.ChangePasswordAsync(CurrentUserId, dto);
            return Ok(ApiResponse.Ok("Password changed. Please log in again."));
        }
    }
}
