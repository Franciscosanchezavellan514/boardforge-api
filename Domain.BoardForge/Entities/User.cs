using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class User : SoftDeletableEntity
{
    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
}