using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;

namespace DevStack.Application.BoardForge.Services;

public class UsersService(IUnitOfWork unitOfWork) : IUsersService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<UserResponse> GetUserByIdAsync(int userId)
    {
        User? user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found.");

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt,
            EmailConfirmed = user.EmailConfirmed,
        };
    }
}