using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Application.Endpoint.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAtUtc) GenerateToken(User user);
    RefreshTokenGeneratedDTO GenerateRefreshToken();
    string ComputeHash(string raw);
}
