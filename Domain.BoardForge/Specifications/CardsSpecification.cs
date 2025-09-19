using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Specifications;

public class CardByIdAndTeamIdSpecification : BaseSpecification<Card>
{
    public CardByIdAndTeamIdSpecification(int id, int teamId)
    {
        ApplyCriteria(c => c.Id == id && c.TeamId == teamId);
        AddInclude($"{nameof(Card.Labels)}.{nameof(CardLabel.Label)}");
    }
}

public class CardByIdSpecification : BaseSpecification<Card>
{
    public CardByIdSpecification(int id)
    {
        ApplyCriteria(c => c.Id == id);
        AddInclude($"{nameof(Card.Labels)}.{nameof(CardLabel.Label)}");
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