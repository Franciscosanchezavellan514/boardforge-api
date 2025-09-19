using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Application.BoardForge.DTOs;

public record TeamResource(int TeamId) : ITeamResource
{
    public int TeamId { get; set; } = TeamId;
    public Team? Team { get; set; }
}
