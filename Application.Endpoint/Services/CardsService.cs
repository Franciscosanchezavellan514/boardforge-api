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
            IsActive = true
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
            card.Owner?.DisplayName ?? string.Empty,
            etag,
            card.CreatedAt,
            card.UpdatedAt,
            card.CreatedBy,
            card.UpdatedBy,
            card.Labels?.Select(cl => new CardLabelResponse(cl.Label!.Id, cl.Label.Name, cl.Label.ColorHex)) ?? []
        );
    }

    public async Task<CardResponse> UpdateAsync(UpdateTeamResourceRequest<UpdateCardRequest> request, string etag)
    {
        if (!HasUpdates(request.Data)) throw new ArgumentException("At least one field must be provided for update.", nameof(request));

        bool teamExists = await _unitOfWork.Teams.ExistsAsync(request.TeamId);
        if (!teamExists) throw new KeyNotFoundException($"Team with ID {request.TeamId} not found.");

        bool cardExists = await _unitOfWork.Cards.ExistsAsync(new CardByIdAndTeamIdSpecification(request.ResourceId, request.TeamId));
        if (!cardExists) throw new KeyNotFoundException($"Card with ID {request.ResourceId} not found in team {request.TeamId}.");

        if (!_etagService.TryParseIfMatch(etag, out var rowVersion, out _)) throw new ArgumentException("Invalid etag format.", nameof(etag));

        var token = new ConcurrencyToken(request.ResourceId, rowVersion);
        await _unitOfWork.Cards.UpdateAsync(token, card => ApplyUpdates(card, request));
        await _unitOfWork.SaveChangesAsync();

        Card updatedCard = await _unitOfWork.Cards.GetByIdAsync(request.ResourceId)
            ?? throw new KeyNotFoundException($"Card with ID {request.ResourceId} not found after update.");
        return MapEntityToCardResponse(updatedCard);
    }

    private static bool HasUpdates(UpdateCardRequest updates) =>
        !string.IsNullOrWhiteSpace(updates.Title) ||
        updates.Description != null ||
        updates.Order.HasValue ||
        updates.BoardColumnId.HasValue ||
        updates.BoardId.HasValue ||
        updates.OwnerId.HasValue;
    
    private void ApplyUpdates(Card card, UpdateTeamResourceRequest<UpdateCardRequest> request)
    {
        var updates = request.Data;

        if (!string.IsNullOrWhiteSpace(updates.Title))
            card.Title = updates.Title;

        if (updates.Description != null)
            card.Description = updates.Description;

        if (updates.Order.HasValue)
            card.Order = updates.Order.Value;

        if (updates.BoardColumnId.HasValue)
            card.BoardColumnId = updates.BoardColumnId.Value;

        if (updates.BoardId.HasValue)
            card.BoardId = updates.BoardId.Value;

        if (updates.OwnerId.HasValue)
            card.OwnerId = updates.OwnerId.Value;

        card.UpdatedBy = request.UserId;
        card.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
    }

    public async Task SoftDeleteAsync(DeleteTeamResourceRequest request)
    {
        bool teamExists = await _unitOfWork.Teams.ExistsAsync(request.TeamId);
        if (!teamExists) throw new KeyNotFoundException($"Team with ID {request.TeamId} not found.");

        Card? card = await _unitOfWork.Cards.GetFirstAsync(new CardByIdAndTeamIdSpecification(request.ResourceId, request.TeamId));
        if (card == null) throw new KeyNotFoundException($"Card with ID {request.ResourceId} not found in team {request.TeamId}.");

        card.IsActive = false;
        card.DeletedAt = _timeProvider.GetUtcNow().UtcDateTime;
        card.DeletedBy = request.UserId;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CardLabelResponse>> GetLabelsAsync(int id)
    {
        bool cardExists = await _unitOfWork.Cards.ExistsAsync(id);
        if (!cardExists) throw new KeyNotFoundException($"Card with ID {id} not found.");

        var labels = await _unitOfWork.CardLabels.ListAsync(new CardLabelsByCardIdSpecification(id, true));
        return labels.Select(cl => new CardLabelResponse(cl.Label!.Id, cl.Label.Name, cl.Label.ColorHex));
    }

    public async Task AddLabelsAsync(int id, int teamId, AddCardLabelsRequest request)
    {
        bool cardExists = await _unitOfWork.Cards.ExistsAsync(id);
        if (!cardExists) throw new KeyNotFoundException($"Card with ID {id} not found.");

        var uniqueLabelIds = request.LabelIds.Distinct().ToList();
        var labels = await _unitOfWork.Labels.ListAsync(new GetLabelsByIdsAndTeamSpecification(teamId, uniqueLabelIds));
        if (labels.Count() != uniqueLabelIds.Count()) throw new KeyNotFoundException("One or more labels not found in the specified team.");
        if (!labels.Any()) return;

        var cardLabels = labels.Select(label => new CardLabel { CardId = id, LabelId = label.Id });
        await _unitOfWork.CardLabels.AddAsync(cardLabels);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveLabelAsync(int id, int labelId)
    {
        bool cardExists = await _unitOfWork.Cards.ExistsAsync(id);
        if (!cardExists) throw new KeyNotFoundException($"Card with ID {id} not found.");

        var label = await _unitOfWork.CardLabels.GetFirstAsync(new CardLabelByCardAndLabelIdSpecification(id, labelId));
        if (label == null) throw new KeyNotFoundException($"Label with ID {labelId} not found.");

        await _unitOfWork.CardLabels.DeleteAsync(label);
        await _unitOfWork.SaveChangesAsync();
    }
}