using DevStack.Domain.BoardForge.Entities;
using Microsoft.AspNetCore.Authorization;

namespace DevStack.BoardForgeAPI.Authorization;

public class TeamRoleRequirement : IAuthorizationRequirement
{
    public TeamRoleRequirement(TeamMembershipRole.Role role)
    {
        MinimumRole = role;
    }

    public TeamMembershipRole.Role MinimumRole { get; }
}
