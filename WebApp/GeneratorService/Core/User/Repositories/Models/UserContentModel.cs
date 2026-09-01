using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Core.User.Repositories.Models;

public class UserContentModel {
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(UserAuthModel))]
    public Guid UserId { get; set; }

    [Required]
    public string ContentName { get; set; } = string.Empty;

    [Required]
    public string ContentCode { get; set; } = string.Empty;
}