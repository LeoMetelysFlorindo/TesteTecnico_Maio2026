using System.Net;
using System.Text.Json;

namespace ProductApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(ex, "An unhandled exception occurred");

            // Prepare response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Different response based on environment
            var response = _env.IsDevelopment()
                ? new ErrorResponse(
                    StatusCode: context.Response.StatusCode,
                    Message: ex.Message,
                    Details: ex.StackTrace)
                : new ErrorResponse(
                    StatusCode: context.Response.StatusCode,
                    Message: "An internal error occurred.",
                    Details: null);

            // Serialize with camelCase options
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}

// DTO for error responses
public record ErrorResponse(int StatusCode, string Message, string? Details);

// Extension to register middleware
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}