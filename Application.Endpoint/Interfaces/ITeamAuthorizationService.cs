using DevStack.Application.BoardForge.DTOs;
using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Application.BoardForge.Interfaces;

public interface ITeamAuthorizationService
{
    Task<TeamResource?> GetTeamResourceAsync<TEntity>(int resourceId) where TEntity : BaseEntity, ITeamResource;
    Task<TeamMembershipRole.Role?> GetUserRoleAsync(int userId, int teamId);
}