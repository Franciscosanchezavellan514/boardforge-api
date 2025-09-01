using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Interfaces.Repositories;

public interface IAsyncRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<TEntity>> ListAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<List<TEntity>> AddAsync(List<TEntity> entity);
    Task<List<TEntity>> DeleteAsync(List<TEntity> entity);
    Task<List<TEntity>> UpdateAsync(List<TEntity> entity);
    public Task<int> SaveChangesAsync();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    // specification pattern
    Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec);
    Task<int> CountAsync(ISpecification<TEntity> spec);
    IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec);
}