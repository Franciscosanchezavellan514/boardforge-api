namespace DevStack.Application.BoardForge.DTOs.Request;

public record CreateCardRequest(
    string Title,
    string Description,
    int? Order,
    int? BoardColumnId,
    int? BoardId,
    int? OwnerId
);

public record UpdateCardRequest(
    string? Title,
    string? Description,
    int? Order,
    int? BoardColumnId,
    int? BoardId,
    int? OwnerId
);

public record AddCardLabelsRequest(IEnumerable<int> LabelIds);
