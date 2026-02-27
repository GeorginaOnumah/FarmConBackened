using FarmConBackened.DTOs.User;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Users;
using FarmConnect.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace FarmConBackened.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;

        public UserService(AppDbContext db, IImageService imageService) { _db = db; _imageService = imageService; }

        public async Task<UserDto> GetProfileAsync(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found.");
            return MapUserDto(user);
        }

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found.");
            if (dto.FirstName != null) user.FirstName = dto.FirstName.Trim();
            if (dto.LastName != null) user.LastName = dto.LastName.Trim();
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.Address != null) user.Address = dto.Address;
            if (dto.State != null) user.State = dto.State;
            if (dto.LGA != null) user.LGA = dto.LGA;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapUserDto(user);
        }

        public async Task<string> UploadProfileImageAsync(Guid userId, IFormFile file)
        {
            var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException();
            if (user.ProfileImageUrl != null) await _imageService.DeleteImageAsync(user.ProfileImageUrl);
            var (url, _, _) = await _imageService.SaveImageAsync(file, "profiles");
            user.ProfileImageUrl = url;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return url;
        }

        public async Task<FarmerProfile?> GetFarmerProfileAsync(Guid userId) =>
            await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == userId);

        public async Task<FarmerProfile> UpdateFarmerProfileAsync(Guid userId, UpdateFarmerProfileDto dto)
        {
            var profile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == userId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");
            if (dto.FarmName != null) profile.FarmName = dto.FarmName;
            if (dto.FarmDescription != null) profile.FarmDescription = dto.FarmDescription;
            if (dto.FarmAddress != null) profile.FarmAddress = dto.FarmAddress;
            if (dto.FarmLatitude.HasValue) profile.FarmLatitude = dto.FarmLatitude;
            if (dto.FarmLongitude.HasValue) profile.FarmLongitude = dto.FarmLongitude;
            if (dto.NINNumber != null) profile.NINNumber = dto.NINNumber;
            await _db.SaveChangesAsync();
            return profile;
        }

        public async Task<TransporterProfile?> GetTransporterProfileAsync(Guid userId) =>
            await _db.TransporterProfiles.FirstOrDefaultAsync(t => t.UserId == userId);

        public async Task<TransporterProfile> UpdateTransporterProfileAsync(Guid userId, UpdateTransporterProfileDto dto)
        {
            var profile = await _db.TransporterProfiles.FirstOrDefaultAsync(t => t.UserId == userId)
                ?? throw new KeyNotFoundException("Transporter profile not found.");
            if (dto.VehicleType != null) profile.VehicleType = dto.VehicleType;
            if (dto.VehiclePlateNumber != null) profile.VehiclePlateNumber = dto.VehiclePlateNumber;
            if (dto.VehicleCapacity != null) profile.VehicleCapacity = dto.VehicleCapacity;
            await _db.SaveChangesAsync();
            return profile;
        }

        public async Task UpdateTransporterLocationAsync(Guid userId, double latitude, double longitude)
        {
            var profile = await _db.TransporterProfiles.FirstOrDefaultAsync(t => t.UserId == userId);
            if (profile != null)
            {
                profile.CurrentLatitude = latitude;
                profile.CurrentLongitude = longitude;
                await _db.SaveChangesAsync();
            }
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
