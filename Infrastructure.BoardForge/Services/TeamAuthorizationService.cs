using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DevStack.Infrastructure.BoardForge.Services;

public class TeamAuthorizationService(BoardForgeDbContext dbContext, IMemoryCache memoryCache) : ITeamAuthorizationService
{
    private readonly BoardForgeDbContext _dbContext = dbContext;
    private readonly IMemoryCache _cache = memoryCache;

    public async Task<TeamMembershipRole.Role?> GetUserRoleAsync(int userId, int teamId)
    {
        var key = $"TeamRole.{userId}.{teamId}";
        if (_cache.TryGetValue(key, out TeamMembershipRole.Role role)) return role;

        TeamMembership? membership = await _dbContext.TeamMemberships.Where(tm => tm.UserId == userId && tm.TeamId == teamId)
            .FirstOrDefaultAsync();

        if (membership is null) return null;

        role = TeamMembershipRole.ToEnum(membership.Role);
        _cache.Set(key, role, TimeSpan.FromMinutes(10));
        return role;
    }
}
