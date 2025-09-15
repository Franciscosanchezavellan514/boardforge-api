namespace DevStack.Application.BoardForge.DTOs.Request;

/// <summary>
/// Generic request wrapper for create operations
/// </summary>
/// <typeparam name="TData">The type of the data being wrapped</typeparam>
/// <param name="UserId">The ID of the user making the request</param>
/// <param name="Data">The data being wrapped</param>
public record CreateRequest<TData>(int UserId, TData Data);
/// <summary>
/// Generic request wrapper for operations that require the root object ID and user ID
/// </summary>
/// <typeparam name="TData">The type of the data being wrapped</typeparam>
/// <param name="ObjectId">The ID of the root object (e.g., team ID)</param>
/// <param name="UserId">The ID of the user making the request</param>
/// <param name="Data">The data being wrapped</param>
public record BaseRequest<TData>(int ObjectId, int UserId, TData Data);
/// <summary>
/// Generic request wrapper for operations that only require the root object ID and user ID
/// </summary>
/// <param name="ObjectId">The ID of the root object (e.g., team ID)</param>
/// <param name="UserId">The ID of the user making the request</param>
public record BaseRequest(int ObjectId, int UserId);