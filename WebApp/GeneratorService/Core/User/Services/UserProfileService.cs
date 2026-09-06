using GeneratorService.Core.User.Repositories;
using GeneratorService.Core.User.Repositories.Models;
using GeneratorService.Core.User.Services.Subservices;

namespace GeneratorService.Core.User.Services;

public interface IUserProfileService {
    Task CreateProfileAsync(Guid userId);
    Task<UserProfileModel?> GetProfileByIdAsync(Guid userId);
    Task<bool> HasEnoughCreditsAsync(Guid userId, int requiredCredits);
    Task DeductCreditsAsync(Guid userId, int creditsToDeduct);
    Task ChangeSubscriptionTierAsync(Guid userId, SubscriptionTier newTier);
    Task UpdateProfileAsync(Guid userId, string? name, string? avatarUrl);
}

public class UserProfileService(IUserProfileRepository _userProfileRepository, IUserProfileNameGeneratorService _nameGeneratorService) : IUserProfileService {
    public async Task CreateProfileAsync(Guid userId) {
        var profile = new UserProfileModel {
            Id = userId,
            Name = _nameGeneratorService.GenerateRandomName(),
            Credits = 10,
            SubscriptionTier = SubscriptionTier.Free,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userProfileRepository.CreateAsync(profile);
    }

    public async Task<UserProfileModel?> GetProfileByIdAsync(Guid userId) {
        return await _userProfileRepository.GetByIdAsync(userId);
    }

    public async Task<bool> HasEnoughCreditsAsync(Guid userId, int requiredCredits) {
        var profile = await GetProfileByIdAsync(userId);
        if (profile == null) return false;
        
        return profile.SubscriptionTier == SubscriptionTier.Enterprise || profile.Credits >= requiredCredits;
    }

    public async Task DeductCreditsAsync(Guid userId, int creditsToDeduct) {
        var profile = await GetProfileByIdAsync(userId);
        if (profile == null) throw new Exception("User profile not found");
        
        if (profile.SubscriptionTier != SubscriptionTier.Enterprise) {
            if (profile.Credits < creditsToDeduct) throw new Exception("Not enough credits");
            profile.Credits -= creditsToDeduct;
            await _userProfileRepository.UpdateAsync(profile);
        }
    }

    public async Task ChangeSubscriptionTierAsync(Guid userId, SubscriptionTier newTier) {
        var profile = await GetProfileByIdAsync(userId);
        if (profile == null) throw new Exception("User profile not found");
        
        profile.SubscriptionTier = newTier;
        if (newTier == SubscriptionTier.Pro) profile.Credits += 100;
        
        await _userProfileRepository.UpdateAsync(profile);
    }

    public async Task UpdateProfileAsync(Guid userId, string? name, string? avatarUrl) {
        var profile = await GetProfileByIdAsync(userId);
        if (profile == null) throw new Exception("User profile not found");
        
        if (!string.IsNullOrWhiteSpace(name)) profile.Name = name;
        if (avatarUrl != null) profile.AvatarUrl = avatarUrl;
        
        await _userProfileRepository.UpdateAsync(profile);
    }
}
