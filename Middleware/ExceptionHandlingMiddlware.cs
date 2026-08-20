using System.Net;
using System.Text.Json;

namespace EmployeeManagementSystem.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
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
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest);
        } catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict);
        } catch (KeyNotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound);
        } catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Forbidden);
        } catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occured.");

            await HandleExceptionAsync(
                context,
                ex,
                HttpStatusCode.InternalServerError,
                "Something went wrong on the server."
            );
        }
    }
    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string? customMessage = null
    )
    {
        _logger.LogWarning(exception, "Handle exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = customMessage ?? exception.Message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}