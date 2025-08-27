using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Interfaces.Repositories;

public interface IAsyncRepository<TEntity> where TEntity : BaseEntity
{
    // TODO: Implement Specification Pattern
    // IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec);
    Task<TEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<TEntity>> ListAllAsync();
    // Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec);
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    // Task<int> CountAsync(ISpecification<TEntity> spec);
    Task<List<TEntity>> AddAsync(List<TEntity> entity);
    Task<List<TEntity>> DeleteAsync(List<TEntity> entity);
    Task<List<TEntity>> UpdateAsync(List<TEntity> entity);
}