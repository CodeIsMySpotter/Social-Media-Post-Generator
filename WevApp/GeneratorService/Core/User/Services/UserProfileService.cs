namespace GeneratorService.Core.User.Services;


public interface IUserProfileService {
    Task<void> CreateProfile(Guid id) {
        var profile = UserProfileModel{
            Id = id,
            Name = ""
        }
    }
}

