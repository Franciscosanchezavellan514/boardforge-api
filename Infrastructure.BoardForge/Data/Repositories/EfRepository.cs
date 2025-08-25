using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;

namespace DevStack.Infrastructure.BoardForge.Data.Repositories;

public class EfRepository<TEntity> : IAsyncRepository<TEntity> where TEntity : BaseEntity
{
    public Task<TEntity> AddAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<List<TEntity>> AddAsync(List<TEntity> entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<List<TEntity>> DeleteAsync(List<TEntity> entity)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TEntity>> ListAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<List<TEntity>> UpdateAsync(List<TEntity> entity)
    {
        throw new NotImplementedException();
    }
}