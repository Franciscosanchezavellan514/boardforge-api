using DevStack.BoardForgeAPI.Authorization;
using DevStack.Domain.BoardForge.Entities;

namespace DevStack.BoardForgeAPI.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(TeamMembershipRole.Viewer, policy => policy.Requirements.Add(new TeamRoleRequirement(TeamMembershipRole.Role.Viewer)));
            options.AddPolicy(TeamMembershipRole.Member, policy => policy.Requirements.Add(new TeamRoleRequirement(TeamMembershipRole.Role.Member)));
            options.AddPolicy(TeamMembershipRole.Owner, policy => policy.Requirements.Add(new TeamRoleRequirement(TeamMembershipRole.Role.Owner)));
        });

        return services;
    }
}