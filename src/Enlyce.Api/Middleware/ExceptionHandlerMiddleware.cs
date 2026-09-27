using System.Net;
using System.Text.Json;
using Enlyce.Application.Billing;
using Enlyce.Domain.Errors;

namespace Enlyce.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
            if (ex is DomainError or UnauthorizedAccessException or InvalidOperationException or BadHttpRequestException)
                _logger.LogWarning("Solicitud rechazada: {ExceptionType}", ex.GetType().Name);
            else
                _logger.LogError(ex, "Excepcion no manejada: {ExceptionType}", ex.GetType().Name);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is BadHttpRequestException badRequest)
        {
            context.Response.Headers.CacheControl = "no-store";
            await Results.Problem(
                statusCode: badRequest.StatusCode,
                title: "Solicitud inválida",
                detail: "Uno o más parámetros no tienen el formato esperado.")
                .ExecuteAsync(context);
            return;
        }

        var (statusCode, message) = exception switch
        {
            DomainError e => (HttpStatusCode.BadRequest, e.Message),
            UnauthorizedAccessException e => (HttpStatusCode.Unauthorized, e.Message),
            PaymentGatewayUnavailableException e => (HttpStatusCode.ServiceUnavailable, e.Message),
            InvalidOperationException e => (HttpStatusCode.Conflict, e.Message),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)statusCode
        });

        await context.Response.WriteAsync(response);
    }
}
