using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Interfaces.Repositories;

public interface IUserRepository : IAsyncRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}