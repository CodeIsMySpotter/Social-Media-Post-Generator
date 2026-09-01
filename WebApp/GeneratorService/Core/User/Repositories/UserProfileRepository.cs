using GeneratorService.Core.Global.Database;
using GeneratorService.Core.User.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Core.User.Repositories;

public interface IUserProfileRepository {
    Task<UserProfileModel?> GetByIdAsync(Guid id);
    Task<UserProfileModel?> GetByEmailAsync(string email);
    Task CreateAsync(UserProfileModel profile);
}

public class UserProfileRepository(AppDbContext _context) : IUserProfileRepository {
    
    public async Task<UserProfileModel?> GetByIdAsync(Guid id){
        var result = await _context.UserProfiles.FindAsync(id);
        return result;
    }

    public async Task<UserProfileModel?> GetByEmailAsync(string email) {
        var result = await _context.UserProfiles
            .Where(p => p.User != null && p.User.Email == email)
            .FirstOrDefaultAsync();
        
        return result;
    }

    public async Task CreateAsync(UserProfileModel profile) {
        await _context.UserProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();
    }
}