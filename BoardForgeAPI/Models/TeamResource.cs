using DevStack.Domain.BoardForge.Entities;

namespace DevStack.BoardForgeAPI.Models;

public record TeamResource(int TeamId) : ITeamResource
{
    public int TeamId { get; set; } = TeamId;
    public Team? Team { get; set; }
}
