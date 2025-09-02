
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Interfaces;
using DevStack.Infrastructure.BoardForge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DevStack.Infrastructure.BoardForge.Data;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly BoardForgeDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminUserSeed _adminUserSeed;

    public DatabaseSeeder(BoardForgeDbContext context, IPasswordHasher passwordHasher, IOptions<AdminUserSeed> adminOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _adminUserSeed = adminOptions.Value;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();
        await SeedUserAsync();
    }

    private async Task SeedUserAsync()
    {
        // If users already exist, skip seeding.
        if (await _context.Users.AnyAsync()) return;

        var requests = new List<UserRequest>
        {
            new() { Email = "itachi.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "sasuke.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "madara.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "obito.uchiha@devstack.com", Password = "UchihaClan123!" },
            new() { Email = "kakashi.uchiha@devstack.com", Password = "UchihaClan123!" }
        };

        if (_adminUserSeed.IsValid)
        {
            var adminReq = new UserRequest { Email = _adminUserSeed.Email, Password = _adminUserSeed.Password };
            requests.Add(adminReq);
        }

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
