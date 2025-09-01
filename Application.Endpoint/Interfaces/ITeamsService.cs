using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ITeamsService
{
    Task<IEnumerable<TeamResponse>> ListMyTeamsAsync(int userId);
}