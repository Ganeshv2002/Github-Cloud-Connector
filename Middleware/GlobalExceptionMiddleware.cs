using System.Net;
using System.Text.Json;
using GithubCloudConnector.DTOs;
using GithubCloudConnector.Exceptions;

namespace GithubCloudConnector.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (GitHubApiException ex)
        {
            _logger.LogWarning(ex, "GitHub API exception on {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            await WriteErrorAsync(context, (int)ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorAsync(context, (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new ErrorResponse(message), SerializerOptions);
        return context.Response.WriteAsync(body);
    }
}
