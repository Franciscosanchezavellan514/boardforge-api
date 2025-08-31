using DevStack.Application.Endpoint.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Data;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Services;

public class TeamAuthorizationService(BoardForgeDbContext dbContext) : ITeamAuthorizationService
{
    private readonly BoardForgeDbContext _dbContext = dbContext;

    public async Task<TeamMembershipRole.Role?> GetUserRoleAsync(int userId, int teamId)
    {
        // TODO: implement caching...
        // var key = $"TeamRole-{userId}-{teamId}";
        // if (_cache.TryGetValue(key, out TeamMembershipRole.Role role))
        TeamMembership? membership = await _dbContext.TeamMemberships.Where(tm => tm.UserId == userId && tm.TeamId == teamId)
            .FirstOrDefaultAsync();

        if (membership is null) return null;

        var role = TeamMembershipRole.ToEnum(membership.Role);
        // _cache.Set(key, role, TimeSpan.FromMinutes(10));
        return role;
    }
}
