namespace DevStack.Application.BoardForge.DTOs.Response;

public record CardResponse(
    int Id,
    string Title,
    string Description,
    int Order,
    int? BoardColumnId,
    int TeamId,
    int BoardId,
    int OwnerId,
    string? ETag,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int CreatedBy,
    int? UpdatedBy
);