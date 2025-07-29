using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace App.Api.Middleware;

public class ExceptionHandlingMiddleware
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
        catch (FormatException ex) when (ex.Message.Contains("is not a valid 24 digit hex string"))
        {
            //_logger.LogWarning("Invalid ObjectId provided: {Message}", ex.Message);

            var objectIdParamName = GetObjectIdParamName(context);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "One or more validation errors occurred.",
                status = 400,
                errors = new Dictionary<string, string[]>
                {
                    {
                        objectIdParamName ?? "id",
                        new[] { $"The {objectIdParamName ?? "id"} field must be a 24-character hex string." }
                    }
                },
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "An unexpected error occurred. Please try again later.",
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }

    /// <summary>
    /// Спроба витягнути ім'я параметра, в якому, ймовірно, був невалідний ObjectId
    /// </summary>
    private string? GetObjectIdParamName(HttpContext context)
    {
        var routeValues = context.Request.RouteValues;

        foreach (var kvp in routeValues)
        {
            var key = kvp.Key;
            var value = kvp.Value?.ToString();

            if (key == "controller" || key == "action")
                continue;

            if (!string.IsNullOrWhiteSpace(value) && value.Length < 24)
            {
                return key;
            }
        }

        return null;
    }

}
