using FarmConBackened.DTOs.User;
using FarmConBackened.Helpers.Responses;
using FarmConBackened.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [Route("api/users")]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) => _userService = userService;

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userService.GetProfileAsync(CurrentUserId);
            return Ok(ApiResponse<UserDto>.Ok(user));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var user = await _userService.UpdateProfileAsync(CurrentUserId, dto);
            return Ok(ApiResponse<UserDto>.Ok(user, "Profile updated."));
        }

        [HttpPost("me/profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(ApiResponse.Fail("No file provided."));
            var url = await _userService.UploadProfileImageAsync(CurrentUserId, file);
            return Ok(ApiResponse<string>.Ok(url, "Profile image updated."));
        }

        [HttpGet("me/farmer-profile")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> GetFarmerProfile()
        {
            var profile = await _userService.GetFarmerProfileAsync(CurrentUserId);
            return profile != null ? Ok(ApiResponse<object>.Ok(profile)) : NotFound(ApiResponse.Fail("Profile not found."));
        }

        [HttpPut("me/farmer-profile")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> UpdateFarmerProfile([FromBody] UpdateFarmerProfileDto dto)
        {
            var profile = await _userService.UpdateFarmerProfileAsync(CurrentUserId, dto);
            return Ok(ApiResponse<object>.Ok(profile, "Farmer profile updated."));
        }

        [HttpGet("me/transporter-profile")]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> GetTransporterProfile()
        {
            var profile = await _userService.GetTransporterProfileAsync(CurrentUserId);
            return profile != null ? Ok(ApiResponse<object>.Ok(profile)) : NotFound(ApiResponse.Fail("Profile not found."));
        }

        [HttpPut("me/transporter-profile")]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> UpdateTransporterProfile([FromBody] UpdateTransporterProfileDto dto)
        {
            var profile = await _userService.UpdateTransporterProfileAsync(CurrentUserId, dto);
            return Ok(ApiResponse<object>.Ok(profile, "Transporter profile updated."));
        }

        [HttpPut("me/location")]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationDto dto)
        {
            await _userService.UpdateTransporterLocationAsync(CurrentUserId, dto.Latitude, dto.Longitude);
            return Ok(ApiResponse.Ok("Location updated."));
        }
    }
    public record LocationDto(double Latitude, double Longitude);//location dto

}
