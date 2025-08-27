using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Data.Repositories;

public class EfRepository<TEntity> : IAsyncRepository<TEntity> where TEntity : BaseEntity
{
    private readonly BoardForgeDbContext _dbContext;

    public EfRepository(BoardForgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity);
        return entity;
    }

    public async Task<List<TEntity>> AddAsync(List<TEntity> entities)
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities);
        return entities;
    }

    public Task DeleteAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task<List<TEntity>> DeleteAsync(List<TEntity> entities)
    {
        _dbContext.Set<TEntity>().RemoveRange(entities);
        return Task.FromResult(entities);
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.Set<TEntity>().FindAsync(id);
    }

    public async Task<IReadOnlyList<TEntity>> ListAllAsync()
    {
        return await _dbContext.Set<TEntity>().ToListAsync();
    }

    public Task UpdateAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public Task<List<TEntity>> UpdateAsync(List<TEntity> entities)
    {
        _dbContext.Set<TEntity>().UpdateRange(entities);
        return Task.FromResult(entities);
    }
}