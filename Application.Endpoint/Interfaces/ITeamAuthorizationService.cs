using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Application.Endpoint.Interfaces;

public interface ITeamAuthorizationService
{
    Task<TeamMembershipRole.Role?> GetUserRoleAsync(int userId, int teamId);
}