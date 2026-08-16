using System.Text.Json;
using FirstGIG.BuildingBlocks.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FirstGIG.Host.Middleware;

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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failure: {Errors}", ex.Errors);
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errorsGrouped = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ValidationProblemDetails(errorsGrouped)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation failures have occurred.",
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred while processing your request.",
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
