using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ITeamsService
{
    Task<TeamResponse> GetByIdAsync(int id);
    Task<IEnumerable<TeamResponse>> ListMyTeamsAsync(int userId);
    Task<TeamResponse> CreateAsync(BaseRequest<CreateTeamRequest> request);
    Task<TeamResponse> UpdateAsync(BaseRequest<UpdateTeamRequest> request);
    Task AddMemberAsync(BaseRequest<AddTeamMemberRequest> request);
}