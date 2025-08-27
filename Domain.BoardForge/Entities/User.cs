using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class User : BaseEntity
{
    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    [Required]
    public string Salt { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<TeamMembership> TeamMemberships { get; set; } = [];
    public ICollection<Card> AssignedCards { get; set; } = [];
    public ICollection<CardComment> Comments { get; set; } = [];
    public ICollection<CardAttachment> Attachments { get; set; } = [];
    public ICollection<Board> OwnedBoards { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}