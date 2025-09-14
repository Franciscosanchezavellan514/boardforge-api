using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ITeamsService
{
    Task<TeamResponse> GetByIdAsync(int id);
    Task<IEnumerable<TeamResponse>> ListMyTeamsAsync(int userId);
    Task<TeamResponse> CreateAsync(BaseRequest<CreateTeamRequest> request);
    Task<TeamResponse> UpdateAsync(BaseRequest<UpdateTeamRequest> request);
    Task<TeamMembershipResponse> AddMemberAsync(BaseRequest<AddTeamMemberRequest> request);
    Task<TeamMembershipResponse> RemoveMemberAsync(BaseRequest<RemoveTeamMemberRequest> request);
    Task<IEnumerable<TeamMembersResponse>> ListMembersAsync(int teamId);
    Task<TeamResponse> SoftDeleteAsync(BaseRequest request);
    Task<TeamLabelOperationResponse> AddLabels(BaseRequest<IEnumerable<AddTeamLabelRequest>> request);
    Task<IEnumerable<TeamLabelResponse>> GetLabelsAsync(int teamId);
}