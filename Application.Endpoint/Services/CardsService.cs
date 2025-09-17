using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Domain.BoardForge.Interfaces.Services;
using DevStack.Domain.BoardForge.Specifications;

namespace DevStack.Application.BoardForge.Services;

public class CardsService(IUnitOfWork unitOfWork, IEtagService etagService, TimeProvider timeProvider) : ICardsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEtagService _etagService = etagService;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<IEnumerable<CardResponse>> ListAsync(int teamId)
    {
        if (teamId <= 0) throw new ArgumentException("Invalid team ID.", nameof(teamId));

        IReadOnlyList<Card> cards = await _unitOfWork.Cards.ListAsync(new CardsByTeamIdSpecification(teamId));
        return cards.Select(MapEntityToCardResponse);
    }

    public async Task<CardResponse> CreateAsync(BaseRequest<CreateCardRequest> request)
    {
        bool teamExists = await _unitOfWork.Teams.ExistsAsync(request.ObjectId);
        if (!teamExists) throw new KeyNotFoundException($"Team with ID {request.ObjectId} not found.");

        var card = new Card
        {
            Title = request.Data.Title,
            Description = request.Data.Description,
            Order = request.Data.Order ?? 0,
            BoardColumnId = request.Data.BoardColumnId,
            BoardId = request.Data.BoardId,
            OwnerId = request.Data.OwnerId,
            TeamId = request.ObjectId,
            CreatedBy = request.UserId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
        };

        await _unitOfWork.Cards.AddAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return MapEntityToCardResponse(card);
    }

    public async Task<CardResponse> GetAsync(int teamId, int cardId)
    {
        bool teamExists = await _unitOfWork.Teams.ExistsAsync(teamId);
        if (!teamExists) throw new KeyNotFoundException($"Team with ID {teamId} not found.");

        Card? card = await _unitOfWork.Cards.GetFirstAsync(new CardByIdAndTeamIdSpecification(cardId, teamId));
        if (card == null) throw new KeyNotFoundException($"Card with ID {cardId} not found in team {teamId}.");

        return MapEntityToCardResponse(card);
    }

    public async Task<CardResponse> GetAsync(int id)
    {
        Card? card = await _unitOfWork.Cards.GetByIdAsync(id);
        if (card == null) throw new KeyNotFoundException($"Card with ID {id} not found.");

        return MapEntityToCardResponse(card);
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