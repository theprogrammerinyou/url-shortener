using System.Net;
using System.Text.Json;
using UrlShortener.Core.Exceptions;

namespace UrlShortener.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Message = exception.Message,
            Detail = exception is ArgumentException argEx ? argEx.ParamName : null
        };

        switch (exception)
        {
            case ConflictException:
            case InvalidOperationException: // Also often used for conflicts in this app
                response.StatusCode = (int)HttpStatusCode.Conflict;
                break;
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            case System.Security.SecurityException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                break;
            case Microsoft.EntityFrameworkCore.DbUpdateException:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new ErrorResponse { Message = "A database error occurred." };
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var payload = JsonSerializer.Serialize(errorResponse);
        return response.WriteAsync(payload);
    }

    private sealed class ErrorResponse
    {
        public string Message { get; init; } = string.Empty;
        public string? Detail { get; init; }
    }
}
