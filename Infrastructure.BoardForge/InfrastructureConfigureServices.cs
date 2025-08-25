using DevStack.Infrastructure.BoardForge.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge;

public static partial class InfrastructureConfigureServices
{
    public static IServiceCollection AddBoardForgeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Migrations output directory: /Infrastructure.BoardForge/Data/Migrations
        services.AddDbContext<BoardForgeDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("BoardForgeDatabase"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(BoardForgeDbContext).Assembly.FullName)));

        // services.AddScoped<IBoardForgeDbContext>(provider => provider.GetRequiredService<BoardForgeDbContext>());

        return services;
    }
}