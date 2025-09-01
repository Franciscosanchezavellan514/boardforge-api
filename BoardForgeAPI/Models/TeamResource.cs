using DevStack.Domain.BoardForge.Entities;

namespace DevStack.BoardForgeAPI.Models;

public record TeamResource(int teamId) : ITeamResource
{
    public int TeamId { get; set; } = teamId;
    public Team? Team { get; set; }
}
