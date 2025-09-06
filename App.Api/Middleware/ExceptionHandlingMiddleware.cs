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
            await HandleErrorAsync(context, "The id field must be a 24-character hex string.", 400);
        }
        catch (ArgumentException ex)
        {
            var field = string.IsNullOrWhiteSpace(ex.ParamName) ? "value" : ex.ParamName;
            await HandleErrorAsync(context, ex.Message, 400);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            var duplicateField = GetDuplicateFieldFromMessage(ex.Message) ?? "UnknownField";
            await HandleErrorAsync(context, $"{duplicateField} must be unique.", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleErrorAsync(context, "Internal Server Error", 500);
        }
    }

    private async Task HandleErrorAsync(HttpContext context, string message, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(message);
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
        var fieldName = indexName.Split('_').FirstOrDefault();

        return fieldName;
    }
}
