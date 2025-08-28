using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public interface ITeamResource
{
    public int TeamId { get; set; }
    public Team? Team { get; set; }
}

public class Team : SoftDeletableEntity
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Board> Boards { get; set; } = [];
    public ICollection<TeamMembership> TeamMemberships { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
    public ICollection<Card> Cards { get; set; } = [];
}
