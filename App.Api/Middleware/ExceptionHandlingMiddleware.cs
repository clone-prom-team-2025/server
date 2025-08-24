using System.Text.Json;
using MongoDB.Driver;

namespace App.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

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
            await HandleValidationErrorAsync(context, "id", "The id field must be a 24-character hex string.");
        }
        catch (ArgumentException ex)
        {
            // Використовуємо paramName, якщо є, інакше дефолтне значення
            var field = string.IsNullOrWhiteSpace(ex.ParamName) ? "value" : ex.ParamName;
            await HandleValidationErrorAsync(context, field, ex.Message);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            var duplicateField = GetDuplicateFieldFromMessage(ex.Message) ?? "UnknownField";
            await HandleValidationErrorAsync(context, duplicateField, $"{duplicateField} must be unique.");
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

    private string? GetObjectIdParamName(HttpContext context)
    {
        var routeValues = context.Request.RouteValues;

        foreach (var kvp in routeValues)
        {
            var key = kvp.Key;
            var value = kvp.Value?.ToString();

            if (key == "controller" || key == "action")
                continue;

            if (!string.IsNullOrWhiteSpace(value) && value.Length < 24) return key;
        }

        return null;
    }

    private async Task HandleValidationErrorAsync(HttpContext context, string field, string message)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            errors = new Dictionary<string, string[]>
            {
                { field, new[] { message } }
            },
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }

    private string? GetDuplicateFieldFromMessage(string message)
    {
        var prefix = "index: ";
        var startIndex = message.IndexOf(prefix);
        if (startIndex == -1) return null;

        startIndex += prefix.Length;
        var endIndex = message.IndexOf(" ", startIndex);
        if (endIndex == -1) return null;

        var indexName = message[startIndex..endIndex];
        var fieldName = indexName.Split('_').FirstOrDefault(); // For example: CategoryId_1 -> CategoryId

        return fieldName;
    }
}