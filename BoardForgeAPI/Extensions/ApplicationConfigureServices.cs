using DevStack.Application.BoardForge.Services;
using DevStack.Application.Endpoint.Interfaces;

namespace DevStack.BoardForgeAPI.Extensions;

public static class ApplicationConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITeamsService, TeamsService>();
        return services;
    }
}