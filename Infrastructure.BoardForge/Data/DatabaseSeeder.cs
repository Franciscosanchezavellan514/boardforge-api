
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Data;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly BoardForgeDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(BoardForgeDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();
        await SeedUserAsync();
    }

    private async Task SeedUserAsync()
    {
        var requests = new List<UserRequest>
        {
            new() { Email = "itachi.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "sasuke.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "madara.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "obito.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "kakashi.uchiha@devstack.com", Password = "UchihaClan123!" }
        };

        List<User> users = new();
        foreach (var request in requests)
        {
            var (hashedPassword, salt) = _passwordHasher.HashPassword(request.Password);
            var user = new User
            {
                Email = request.Email,
                PasswordHash = hashedPassword,
                Salt = Convert.ToBase64String(salt),
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                DisplayName = request.Email.Split('@')[0]
            };
            users.Add(user);
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }
}
