using GeneratorService.Core.User.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Core.Global.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    public DbSet<UserAuthModel> UserAuth { get; set; }
    public DbSet<UserProfileModel> UserProfiles { get; set; }
    public DbSet<UserContentModel> UserContent { get; set; }
}
