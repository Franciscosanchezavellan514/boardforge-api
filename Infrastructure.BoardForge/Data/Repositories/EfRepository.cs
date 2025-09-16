using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Exceptions;
using DevStack.Domain.BoardForge.Interfaces;
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

    public async Task<List<TEntity>> AddAsync(IEnumerable<TEntity> entities)
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities);
        return entities.ToList();
    }

    public Task DeleteAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task<List<TEntity>> DeleteAsync(IEnumerable<TEntity> entities)
    {
        _dbContext.Set<TEntity>().RemoveRange(entities);
        return Task.FromResult(entities.ToList());
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.Set<TEntity>().FindAsync(id);
    }

    public async Task<IReadOnlyList<TEntity>> ListAllAsync()
    {
        return await _dbContext.Set<TEntity>().Where(e => e.IsActive).ToListAsync();
    }

    public Task UpdateAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public Task<List<TEntity>> UpdateAsync(IEnumerable<TEntity> entities)
    {
        _dbContext.Set<TEntity>().UpdateRange(entities);
        return Task.FromResult(entities.ToList());
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            // Attempt to save changes
            return await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle concurrency conflicts
            throw new EntityConcurrencyConflictException<TEntity>();
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Attempt to save changes
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle concurrency conflicts
            throw new EntityConcurrencyConflictException<TEntity>();
        }
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    public async Task<int> CountAsync(ISpecification<TEntity> spec)
    {
        return await ApplySpecification(spec).CountAsync();
    }

    public IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
    {
        IQueryable<TEntity> query = SpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>().AsQueryable(), spec);
        return query;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Set<TEntity>().FindAsync(id) is not null;
    }

    public async Task<bool> ExistsAsync(ISpecification<TEntity> spec)
    {
        return await ApplySpecification(spec).AnyAsync();
    }

    public Task<TEntity?> GetFirstAsync(ISpecification<TEntity> spec)
    {
        return ApplySpecification(spec).FirstOrDefaultAsync();
    }

    public Task UpdateAsync(ConcurrencyToken token, Action<TEntity> applyChanges)
    {
        var entity = _dbContext.Set<TEntity>().FirstOrDefault(e => e.Id == token.Id);
        if (entity is null) throw new EntityNotFoundException($"Entity of type {typeof(TEntity).Name} with ID {token.Id} not found.");

        // Apply changes
        applyChanges(entity);
        if (entity is not VersionedEntity versionedEntity)
            throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} does not support concurrency control.");

        // Set the original RowVersion to handle concurrency
        SetOriginalRowVersion(versionedEntity, token.RowVersion);

        // Don't call update, EF Core will mark all properties as modified, which we don't want.
        //_dbContext.Set<TEntity>().Update(entity);

        return Task.CompletedTask;
    }

    private void SetOriginalRowVersion<TVersionedEntity>(TVersionedEntity entity, byte[] originalRowVersion) where TVersionedEntity : VersionedEntity
    {
        var entry = _dbContext.Entry(entity);
        entry.Property(e => e.RowVersion).OriginalValue = originalRowVersion;
        // Do NOT set CurrentValue; EF will set the new RowVersion after a successful save.
    }
}