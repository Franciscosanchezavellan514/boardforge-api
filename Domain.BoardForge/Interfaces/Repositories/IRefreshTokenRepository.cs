using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RemoveAsync(string token);
}
