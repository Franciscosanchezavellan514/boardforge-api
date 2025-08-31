using System.Security.Claims;
using DevStack.Application.Endpoint.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using Microsoft.AspNetCore.Authorization;

namespace DevStack.BoardForgeAPI.Authorization;

public sealed class TeamRoleAuthorizationHandler(ITeamAuthorizationService teamAuthService) : AuthorizationHandler<TeamRoleRequirement, ITeamResource>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TeamRoleRequirement requirement, ITeamResource resource)
    {
        string userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        if (!int.TryParse(userIdStr, out int userId)) return;

        TeamMembershipRole.Role? role = await teamAuthService.GetUserRoleAsync(userId, resource.TeamId);
        if (role is null) return;

        if (role >= requirement.MinimumRole)
        {
            context.Succeed(requirement);
        }
    }
}