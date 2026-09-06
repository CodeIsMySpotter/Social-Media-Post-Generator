using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorService.Core.User.Repositories.Models;

public class UserProfileModel {
    [Key]
    [ForeignKey(nameof(User))]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }
    
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    
    public int Credits { get; set; } = 10; // Default credits for new users

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual UserAuthModel? User { get; set; }
}

public enum SubscriptionTier {
    Free,
    Pro,
    Enterprise
}
