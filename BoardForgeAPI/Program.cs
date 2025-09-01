using DevStack.BoardForgeAPI.Authorization;
using DevStack.BoardForgeAPI.Extensions;
using DevStack.BoardForgeAPI.Middlewares;
using DevStack.Infrastructure.BoardForge;
using DevStack.Infrastructure.BoardForge.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Trigger database seeding
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
