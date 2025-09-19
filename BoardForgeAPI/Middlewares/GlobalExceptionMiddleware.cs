using System.Net.Mime;
using DevStack.BoardForgeAPI.Models;

namespace DevStack.BoardForgeAPI.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception exception)
        {
            var httpResponse = HttpErrorResponse.From(exception);
            if (httpResponse.StatusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception.Message);
            }

            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = httpResponse.StatusCode;
            await context.Response.WriteAsJsonAsync(httpResponse);
        }
    }
}