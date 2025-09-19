using DevStack.BoardForgeAPI.Authorization;
using DevStack.BoardForgeAPI.Extensions;
using DevStack.BoardForgeAPI.Middlewares;
using DevStack.Infrastructure.BoardForge;
using DevStack.Infrastructure.BoardForge.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.WriteTo.Console();
    string currentDirectory = Directory.GetCurrentDirectory();
    string logFilePath = Path.Combine(currentDirectory, "Logs", "log-.json");
    loggerConfiguration.WriteTo.File(
        formatter: new Serilog.Formatting.Json.JsonFormatter(),
        logFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30
    );
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerSettings();

// Security settings
builder.Services.AddAuthorizationPolicies();
builder.Services.AddJwtAuthentication(builder.Configuration);

// Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<IAuthorizationHandler, TeamRoleAuthorizationHandler>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.UseSerilogRequestLogging();

// Trigger database seeding
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
