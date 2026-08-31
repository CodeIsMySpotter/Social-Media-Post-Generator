using GeneratorService.Core.Global.Database;
using GeneratorService.Core.User.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Core.User.Repositories;

public interface IUserAuthRepository
{
    Task<UserAuthModel?> GetByEmailAsync(string email);
    Task<UserAuthModel?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(UserAuthModel user);
    Task<bool> UpdateAsync(UserAuthModel user);
    Task<bool> DeleteAsync(Guid id);
}

public class UserAuthRepository(AppDbContext _context) : IUserAuthRepository
{
    public Task<UserAuthModel?> GetByEmailAsync(string email)
    {
        return _context.UserAuth.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserAuthModel?> GetByIdAsync(Guid id)
    {
        var result = await _context.UserAuth.FindAsync(id);
        return result;
    }

    public async Task<bool> CreateAsync(UserAuthModel user)
    {
        await _context.UserAuth.AddAsync(user);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(UserAuthModel user)
    {
        _context.UserAuth.Update(user);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.UserAuth.FindAsync(id);
        if (user != null)
        {
            _context.UserAuth.Remove(user);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        return false;
    }
}


