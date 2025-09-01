namespace DevStack.Application.BoardForge.DTOs.Request;

public record BaseRequest<TData>(int? ObjectId, int UserId, TData Data);