using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Interfaces.Repositories;

public interface IAsyncRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<TEntity>> ListAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task UpdateAsync(ConcurrencyToken token, Action<TEntity> applyChanges);
    Task DeleteAsync(TEntity entity);
    Task<bool> ExistsAsync(int id);
    Task<List<TEntity>> AddAsync(IEnumerable<TEntity> entity);
    Task<List<TEntity>> DeleteAsync(IEnumerable<TEntity> entity);
    Task<List<TEntity>> UpdateAsync(IEnumerable<TEntity> entity);
    public Task<int> SaveChangesAsync();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    // specification pattern
    Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec);
    Task<int> CountAsync(ISpecification<TEntity> spec);
    Task<bool> ExistsAsync(ISpecification<TEntity> spec);
    IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec);
    Task<TEntity?> GetFirstAsync(ISpecification<TEntity> spec);
}