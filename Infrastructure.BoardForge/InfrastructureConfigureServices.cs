using DevStack.Infrastructure.BoardForge.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DevStack.Infrastructure.BoardForge.Models;
using DevStack.Application.Endpoint.Interfaces;
using DevStack.Infrastructure.BoardForge.Services;

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
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}