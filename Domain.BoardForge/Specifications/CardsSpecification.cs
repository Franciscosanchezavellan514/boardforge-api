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

public class CardByIdAndTeamIdSpecification : BaseSpecification<Card>
{
    public CardByIdAndTeamIdSpecification(int id, int teamId)
    {
        ApplyCriteria(c => c.Id == id && c.TeamId == teamId);
        AddInclude(i => i.Labels);
    }
}

public class CardLabelsByCardIdSpecification : BaseSpecification<CardLabel>
{
    public CardLabelsByCardIdSpecification(int cardId, bool includeLabel = false)
    {
        ApplyCriteria(cl => cl.CardId == cardId);
        if (includeLabel)
        {
            AddInclude(i => i.Label!);
        }
    }
}

public class CardLabelByCardAndLabelIdSpecification : BaseSpecification<CardLabel>
{
    public CardLabelByCardAndLabelIdSpecification(int cardId, int labelId)
    {
        ApplyCriteria(cl => cl.CardId == cardId && cl.LabelId == labelId);
    }
}