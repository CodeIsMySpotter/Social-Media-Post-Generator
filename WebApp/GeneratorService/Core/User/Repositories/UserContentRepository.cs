using GeneratorService.Core.Global.Database;
using GeneratorService.Core.User.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Core.User.Repositories;


public interface IUserContentRepository {
    Task<UserContentModel?> GetByContentNameAndUserIdAsync(string contentName, Guid userId);
    Task<UserContentModel?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(UserContentModel userContentModel);
    Task<bool> DeleteAsync(Guid userId, string contentName);
}


public class UserContentRepository(AppDbContext _context) : IUserContentRepository {
    public async Task<UserContentModel?> GetByContentNameAndUserIdAsync(string contentName, Guid userId) {
        var result = await _context.UserContent
            .Where(c => c.ContentName == contentName && c.UserId == userId)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<UserContentModel?> GetByIdAsync(Guid id) {
        var result = await _context.UserContent.FindAsync(id);
        return result;
    }

    public async Task<bool> CreateAsync(UserContentModel userContent) {
        await _context.UserContent.AddAsync(userContent);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid userId, string contentName) {

        var userContent = await _context.UserContent
            .Where(c => c.UserId == userId && c.ContentName == contentName)
            .FirstOrDefaultAsync();

        if (userContent != null) {
            _context.UserContent.Remove(userContent);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        return false;
    }

}