using GeneratorService.Core.User.Repositories;
using GeneratorService.Core.User.Repositories.Models;
using GeneratorService.Core.User.Services.Subservices;

namespace GeneratorService.Core.User.Services;

public interface IUserProfileService {
    Task CreateProfileAsync(Guid userId);
    Task<UserProfileModel?> GetProfileByIdAsync(Guid userId);
}

public class UserProfileService(IUserProfileRepository _userProfileRepository, IUserProfileNameGeneratorService _nameGeneratorService) : IUserProfileService {
    public async Task CreateProfileAsync(Guid userId) {
        var profile = new UserProfileModel {
            Id = userId,
            Name = _nameGeneratorService.GenerateRandomName()
        };

        await _userProfileRepository.CreateAsync(profile);
    }

    public async Task<UserProfileModel?> GetProfileByIdAsync(Guid userId) {
        return await _userProfileRepository.GetByIdAsync(userId);
    }
}
