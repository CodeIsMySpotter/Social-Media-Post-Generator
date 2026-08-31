using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Core.User.Repositories.Models;

[Index(nameof(Email), IsUnique = true)]
public class UserAuthModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual UserProfileModel? Profile { get; set; }
}
