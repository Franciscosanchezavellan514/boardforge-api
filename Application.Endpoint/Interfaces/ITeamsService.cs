using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ITeamsService
{
    Task<TeamResponse> GetByIdAsync(int id);
    Task<IEnumerable<TeamResponse>> ListMyTeamsAsync(int userId);
    Task<TeamResponse> CreateAsync(CreateRequest<CreateTeamRequest> request);
    Task<TeamResponse> UpdateAsync(BaseRequest<UpdateTeamRequest> request);
    Task<TeamMembershipResponse> AddMemberAsync(BaseRequest<AddTeamMemberRequest> request);
    Task<TeamMembershipResponse> RemoveMemberAsync(BaseRequest<RemoveTeamMemberRequest> request);
    Task<IEnumerable<TeamMembersResponse>> ListMembersAsync(int teamId);
    Task<TeamResponse> SoftDeleteAsync(BaseRequest request);
    /// <summary>
    /// Update a team member's role within a team.
    /// The request containing the team member's ID and the new role.<br/>
    /// Key of the KeyValuePair is the UserId of the member whose role is to be updated.<br/>
    /// Value of the KeyValuePair is an UpdateTeamMembershipRequest containing the new role.
    /// </summary>
    /// <param name="request">The request containing the team member's ID and the new role.</param>
    /// <returns></returns>
    Task<TeamMembershipResponse> UpdateMembershipAsync(BaseRequest<KeyValuePair<int, UpdateTeamMembershipRequest>> request);
}