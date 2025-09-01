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