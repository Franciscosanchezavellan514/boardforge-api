namespace DevStack.Application.BoardForge.DTOs.Request;

public record CreateCardRequest(
    int TeamId,
    string Title,
    string Description,
    int? Order,
    int? BoardColumnId,
    int? BoardId,
    int? OwnerId
);