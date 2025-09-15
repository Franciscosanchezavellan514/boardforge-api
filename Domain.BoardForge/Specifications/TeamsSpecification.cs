using System.Linq.Expressions;
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
        Expression<Func<TeamMembership, bool>> criteria;

        if (includeActiveOnly)
        {
            criteria = tm => tm.UserId == userId && tm.TeamId == teamId && tm.IsActive == true;
        }
        else
        {
            criteria = tm => tm.UserId == userId && tm.TeamId == teamId;
        }

        ApplyCriteria(criteria);
    }
}

public sealed class GetTeamMembershipsByTeamSpecification : BaseSpecification<TeamMembership>
{
    public GetTeamMembershipsByTeamSpecification(int teamId, bool includeActiveOnly = true)
    {
        Expression<Func<TeamMembership, bool>> criteria;

        if (includeActiveOnly)
        {
            criteria = tm => tm.TeamId == teamId && tm.IsActive == true;
        }
        else
        {
            criteria = tm => tm.TeamId == teamId;
        }

        ApplyCriteria(criteria);
        AddInclude(tm => tm.User!);
    }
}

public sealed class GetLabelsByTeamSpecification : BaseSpecification<Label>
{
    public GetLabelsByTeamSpecification(int teamId)
    {
        ApplyCriteria(c => c.TeamId.Equals(teamId));
        ApplyOrderBy(order => order.Name);
    }
}

public sealed class GetLabelByIdAndTeamSpecification : BaseSpecification<Label>
{
    public GetLabelByIdAndTeamSpecification(int id, int teamId)
    {
        ApplyCriteria(c => c.Id.Equals(id) && c.TeamId.Equals(teamId));
    }
}

public sealed class GetLabelsByTeamAndNormalizedNameSpecification : BaseSpecification<Label>
{
    public GetLabelsByTeamAndNormalizedNameSpecification(int teamId, IEnumerable<string> normalizedNames)
    {
        ApplyCriteria(c => c.TeamId.Equals(teamId) && normalizedNames.Contains(c.NormalizedName));
    }

    public GetLabelsByTeamAndNormalizedNameSpecification(int teamId, string normalizedName)
    {
        ApplyCriteria(c => c.TeamId.Equals(teamId) && c.NormalizedName.Equals(normalizedName));
    }
}