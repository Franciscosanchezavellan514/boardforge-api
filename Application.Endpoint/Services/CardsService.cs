using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Domain.BoardForge.Interfaces.Services;
using DevStack.Domain.BoardForge.Specifications;

namespace DevStack.Application.BoardForge.Services;

public class CardsService(IUnitOfWork unitOfWork, IEtagService etagService) : ICardsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEtagService _etagService = etagService;

    public async Task<IEnumerable<CardResponse>> ListAsync(int teamId)
    {
        if (teamId <= 0) throw new ArgumentException("Invalid team ID.", nameof(teamId));

        IReadOnlyList<Card> cards = await _unitOfWork.Cards.ListAsync(new CardsByTeamIdSpecification(teamId));
        return cards.Select(MapEntityToCardResponse);
    }

    private CardResponse MapEntityToCardResponse(Card card)
    {
        string etag = _etagService.FromRowVersion(card.RowVersion);

        return new CardResponse(
            card.Id,
            card.Title,
            card.Description,
            card.Order,
            card.BoardColumnId,
            card.TeamId,
            card.BoardId,
            card.OwnerId,
            etag,
            card.CreatedAt,
            card.UpdatedAt,
            card.CreatedBy,
            card.UpdatedBy
        );
    }
}