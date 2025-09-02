using DevStack.Application.BoardForge.Services;
using DevStack.Application.BoardForge.Interfaces;

namespace DevStack.BoardForgeAPI.Extensions;

public static class ApplicationConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUsersService, UsersService>();
        
        return services;
    }
}