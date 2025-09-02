namespace DevStack.Application.BoardForge.DTOs.Request;

public record BaseRequest<TData>(int? ObjectId, int UserId, TData Data);
public record BaseRequest(int ObjectId, int UserId);