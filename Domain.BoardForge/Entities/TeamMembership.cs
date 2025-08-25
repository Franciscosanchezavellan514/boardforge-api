using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class MembershipRole
{
    public const string Owner = "Owner";
    public const string Member = "Member";
    public const string Viewer = "Viewer";
}

public class TeamMembership : SoftDeletableEntity
{
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }
    [Required]
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    [Required, MaxLength(20)]
    public string Role { get; set; } = MembershipRole.Member;
}