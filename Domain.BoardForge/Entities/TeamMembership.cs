using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class TeamMembershipRole
{
    public const string Owner = "Owner";
    public const string Member = "Member";
    public const string Viewer = "Viewer";

    public static Role ToEnum(string role) => role.ToLower() switch
    {
        "owner" => Role.Owner,
        "member" => Role.Member,
        "viewer" => Role.Viewer,
        _ => throw new ArgumentOutOfRangeException(nameof(role), $"Not expected role value: {role}")
    };

    public static string FromEnum(Role role) => role switch
    {
        Role.Owner => "Owner",
        Role.Member => "Member",
        Role.Viewer => "Viewer",
        _ => throw new ArgumentOutOfRangeException(nameof(role), $"Not expected role value: {role}")
    };

    public enum Role
    {
        Owner = 30, // Full access to all resources
        Member = 20, // Limited access to certain resources
        Viewer = 10  // Read-only access
    }
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
    public string Role { get; set; } = TeamMembershipRole.Member;
}