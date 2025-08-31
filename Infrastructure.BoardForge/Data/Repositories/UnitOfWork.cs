using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;

namespace DevStack.Infrastructure.BoardForge.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BoardForgeDbContext _dbContext;
    public IUserRepository Users { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public IAsyncRepository<Team> Teams { get; }

    public IAsyncRepository<TeamMembership> TeamMemberships { get; }

    public UnitOfWork(BoardForgeDbContext dbContext)
    {
        _dbContext = dbContext;
        Users = new UserRepository(_dbContext);
        RefreshTokens = new RefreshTokenRepository(_dbContext);
        Teams = new EfRepository<Team>(_dbContext);
        TeamMemberships = new EfRepository<TeamMembership>(_dbContext);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
