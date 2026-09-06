using GeneratorService.Core.User.Repositories;
using GeneratorService.Core.User.Repositories.Models;

namespace GeneratorService.Core.User.Services;

public interface IUserContentService {
    Task<UserContentModel?> GetContentAsync(Guid userId, string contentName);
    Task<List<UserContentModel>> GetAllUserContentAsync(Guid userId);
    Task<bool> CreateContentAsync(Guid userId, string contentName, string contentCode);
    Task<bool> DeleteContentAsync(Guid userId, string contentName);
    Task<bool> UpdateContentAsync(Guid userId, string contentName, string newContentCode);
}

public class UserContentService(IUserContentRepository _userContentRepository) : IUserContentService {
    
    public async Task<UserContentModel?> GetContentAsync(Guid userId, string contentName) {
        return await _userContentRepository.GetByContentNameAndUserIdAsync(contentName, userId);
    }

    public async Task<List<UserContentModel>> GetAllUserContentAsync(Guid userId) {
        return await _userContentRepository.GetAllByUserIdAsync(userId);
    }

    public async Task<bool> CreateContentAsync(Guid userId, string contentName, string contentCode) {
        var existing = await _userContentRepository.GetByContentNameAndUserIdAsync(contentName, userId);
        if (existing != null) return false;

        var newContent = new UserContentModel {
            Id = Guid.NewGuid(),
            UserId = userId,
            ContentName = contentName,
            ContentCode = contentCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _userContentRepository.CreateAsync(newContent);
    }

    public async Task<bool> DeleteContentAsync(Guid userId, string contentName) {
        return await _userContentRepository.DeleteAsync(userId, contentName);
    }

    public async Task<bool> UpdateContentAsync(Guid userId, string contentName, string newContentCode) {
        var existing = await _userContentRepository.GetByContentNameAndUserIdAsync(contentName, userId);
        if (existing == null) return false;

        existing.ContentCode = newContentCode;
        return await _userContentRepository.UpdateAsync(existing);
    }
}
