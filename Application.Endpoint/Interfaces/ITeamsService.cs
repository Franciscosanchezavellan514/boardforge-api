using DevStack.Application.Endpoint.DTOs.Response;

namespace DevStack.Application.Endpoint.Interfaces;

public interface ITeamsService
{
    Task<IEnumerable<TeamResponse>> ListMyTeamsAsync(int userId);
}