using FarmConBackened.DTOs.User;
using FarmConBackened.Models.Users;

namespace FarmConBackened.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetProfileAsync(Guid userId);
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<string> UploadProfileImageAsync(Guid userId, IFormFile file);
        Task<FarmerProfile?> GetFarmerProfileAsync(Guid userId);
        Task<FarmerProfile> UpdateFarmerProfileAsync(Guid userId, UpdateFarmerProfileDto dto);
        Task<TransporterProfile?> GetTransporterProfileAsync(Guid userId);
        Task<TransporterProfile> UpdateTransporterProfileAsync(Guid userId, UpdateTransporterProfileDto dto);
        Task UpdateTransporterLocationAsync(Guid userId, double latitude, double longitude);
    }
}
