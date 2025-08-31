using DevStack.BoardForgeAPI.Authorization;
using DevStack.BoardForgeAPI.Extensions;
using DevStack.BoardForgeAPI.Middlewares;
using DevStack.Infrastructure.BoardForge;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

builder.Services.AddSingleton<IAuthorizationHandler, TeamRoleAuthorizationHandler>();

var app = builder.Build();

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
