using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;

namespace DevStack.Infrastructure.BoardForge.Data.Repositories;

public class UserRepository(BoardForgeDbContext dbContext) : EfRepository<User>(dbContext), IUserRepository { /* Add any user-specific methods here */ }