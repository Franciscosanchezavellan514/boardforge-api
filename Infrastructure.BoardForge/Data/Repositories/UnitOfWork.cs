using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Exceptions;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BoardForgeDbContext _dbContext;
    public IUserRepository Users { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IAsyncRepository<Team> Teams { get; }
    public IAsyncRepository<TeamMembership> TeamMemberships { get; }
    public IAsyncRepository<Label> Labels { get; }
    public IAsyncRepository<Card> Cards { get; }

    public IAsyncRepository<CardLabel> CardLabels { get; }

    public UnitOfWork(BoardForgeDbContext dbContext)
    {
        _dbContext = dbContext;
        Users = new UserRepository(_dbContext);
        RefreshTokens = new RefreshTokenRepository(_dbContext);
        Teams = new EfRepository<Team>(_dbContext);
        TeamMemberships = new EfRepository<TeamMembership>(_dbContext);
        Labels = new EfRepository<Label>(_dbContext);
        Cards = new EfRepository<Card>(_dbContext);
        CardLabels = new EfRepository<CardLabel>(_dbContext);
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new EntityConcurrencyConflictException();
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new EntityConcurrencyConflictException();
        }
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
