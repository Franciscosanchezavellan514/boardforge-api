using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ITeamAuthorizationService
{
    Task<TeamMembershipRole.Role?> GetUserRoleAsync(int userId, int teamId);
}