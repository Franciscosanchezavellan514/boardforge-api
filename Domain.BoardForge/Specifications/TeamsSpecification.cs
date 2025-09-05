using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Specifications;

public sealed class TeamsByUserIdSpecification : BaseSpecification<Team>
{
    public TeamsByUserIdSpecification(int userId)
    {
        ApplyCriteria(t => t.IsActive && t.TeamMemberships.Any(tm => tm.UserId == userId && tm.IsActive));
        AddInclude(t => t.TeamMemberships.Where(tm => tm.IsActive));
    }
}

public sealed class GetTeamMembershipByUserAndTeamSpecification : BaseSpecification<TeamMembership>
{
    public GetTeamMembershipByUserAndTeamSpecification(int userId, int teamId, bool includeActiveOnly = true)
    {
        ApplyCriteria(tm => tm.UserId == userId && tm.TeamId == teamId);
        if (includeActiveOnly)
        {
            ApplyCriteria(tm => tm.IsActive);
        }
    }
}