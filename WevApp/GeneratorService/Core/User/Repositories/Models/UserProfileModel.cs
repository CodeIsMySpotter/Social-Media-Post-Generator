using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorService.Core.User.Repositories.Models;

public class UserProfileModel {
    [Key]
    [ForeignKey(nameof(User))]
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public virtual UserAuthModel? User { get; set; }
}
