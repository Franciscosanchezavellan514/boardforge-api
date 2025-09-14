using DevStack.Infrastructure.BoardForge.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DevStack.Infrastructure.BoardForge.Models;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Infrastructure.BoardForge.Services;
using DevStack.Infrastructure.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Domain.BoardForge.Interfaces.Services;
using DevStack.Infrastructure.BoardForge.Data.Repositories;

namespace DevStack.Infrastructure.BoardForge;

public static partial class InfrastructureConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Migrations output directory: /Infrastructure.BoardForge/Data/Migrations
        services.AddDbContext<BoardForgeDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("BoardForgeDatabase"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(BoardForgeDbContext).Assembly.FullName)));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AdminUserSeed>(configuration.GetSection(AdminUserSeed.SectionName));

        // Infrastructure Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<ITeamAuthorizationService, TeamAuthorizationService>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<IStringUtilsService, StringUtilsService>();

        // Unit of Work & Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}