namespace DevStack.Application.BoardForge.DTOs.Request;

public record CreateRequest<TData>(int UserId, TData Data);
public record BaseRequest<TData>(int ObjectId, int UserId, TData Data);
public record BaseRequest(int ObjectId, int UserId);