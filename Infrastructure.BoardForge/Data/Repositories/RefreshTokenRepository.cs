using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly BoardForgeDbContext _dbContext;

    public RefreshTokenRepository(BoardForgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _dbContext.Set<RefreshToken>().AddAsync(refreshToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.TokenHash == token);
    }

    public async Task RemoveAsync(string token)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken != null)
        {
            _dbContext.Set<RefreshToken>().Remove(refreshToken);
        }
    }

    public Task UpdateAsync(RefreshToken existingToken)
    {
        _dbContext.Set<RefreshToken>().Update(existingToken);
        return Task.CompletedTask;
    }
}
