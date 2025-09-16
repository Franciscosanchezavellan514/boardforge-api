using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Specifications;

public class CardsByTeamIdSpecification : BaseSpecification<Card>
{
    public CardsByTeamIdSpecification(int teamId)
    {
        ApplyCriteria(c => c.TeamId == teamId && c.IsActive);
        ApplyOrderBy(o => o.Title);
        ApplyOrderBy(o => o.BoardId);
        AddInclude(i => i.Labels);
    }
}