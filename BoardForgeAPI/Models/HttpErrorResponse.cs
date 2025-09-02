using System.Net;
using DevStack.Domain.BoardForge.Exceptions;

namespace DevStack.BoardForgeAPI.Models;

public class HttpErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = [];

    public HttpErrorResponse(int statusCode, string message, List<string> errors)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }

    public HttpErrorResponse(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }


    public static HttpErrorResponse From(Exception exception)
    {
        return exception switch
        {
            ApplicationException e => new HttpErrorResponse(StatusCodes.Status400BadRequest, e.Message),
            UnauthorizedAccessException e => new HttpErrorResponse(StatusCodes.Status401Unauthorized, e.Message),
            ArgumentException e => new HttpErrorResponse(StatusCodes.Status400BadRequest, e.Message),
            EntityNotFoundException e => new HttpErrorResponse(StatusCodes.Status404NotFound, e.Message),
            KeyNotFoundException e => new HttpErrorResponse(StatusCodes.Status404NotFound, e.Message),
            // Add more exceptions here...
            _ => new HttpErrorResponse(StatusCodes.Status500InternalServerError, "Internal server error. Please retry later."),
        };
    }
}