using DevStack.Application.BoardForge.DTOs.Request;

namespace DevStack.Application.BoardForge.Interfaces;

public interface IUsersService
{
    Task<UserResponse> GetUserByIdAsync(int userId);
}