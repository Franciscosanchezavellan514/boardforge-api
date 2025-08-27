using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Data.Repositories;

public class UserRepository(BoardForgeDbContext dbContext) : EfRepository<User>(dbContext), IUserRepository
{
    private readonly BoardForgeDbContext _dbContext = dbContext;

    public Task<User?> GetByEmailAsync(string email)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}