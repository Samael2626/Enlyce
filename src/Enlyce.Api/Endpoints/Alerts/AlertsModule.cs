using Enlyce.Api.Endpoints.Leads;
using Enlyce.Application.Abstractions;
using Enlyce.Application.Alerts;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Alerts;

public static class AlertsModule
{
    public static void MapSearchAlerts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/alertas").WithTags("Alertas de búsqueda");

        // Publico: el visitante deja sus criterios y queda como lead con demanda activa.
        group.MapPost("", async (
            [FromBody] CreateSearchAlertRequest request,
            HttpContext http,
            ICommandHandler<CreateSearchAlertCommand, CreateSearchAlertResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CreateSearchAlertCommand(
                request.Nombre, request.Email, request.Telefono, request.AutorizacionDatos,
                request.Canal, LeadClientIp.Resolve(http), request.Operation, request.PropertyType,
                request.Municipality, request.Neighborhood, request.MinPrice, request.MaxPrice,
                request.Bedrooms, request.Bathrooms, request.ParkingSpaces), ct);
            return Results.Accepted(value: result);
        })
        .AllowAnonymous()
        .RequireRateLimiting("public-leads")
        .WithMetadata(new RequestSizeLimitAttribute(16 * 1024))
        .WithName("CreateSearchAlert")
        .Produces<CreateSearchAlertResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // Publico: el enlace de baja llega por correo; el token firmado autoriza.
        group.MapPost("/{id:guid}/baja", async Task<IResult> (
            Guid id,
            [FromBody] UnsubscribeSearchAlertRequest request,
            ICommandHandler<UnsubscribeSearchAlertCommand, bool> handler,
            CancellationToken ct) =>
            await handler.HandleAsync(new UnsubscribeSearchAlertCommand(id, request.Token), ct)
                ? Results.NoContent()
                : Results.NotFound())
        .AllowAnonymous()
        .RequireRateLimiting("owner-details")
        .WithName("UnsubscribeSearchAlert")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record CreateSearchAlertRequest(
    string Nombre,
    string Email,
    string? Telefono,
    bool AutorizacionDatos,
    string? Canal,
    string? Operation,
    string? PropertyType,
    string? Municipality,
    string? Neighborhood,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? Bedrooms,
    int? Bathrooms,
    int? ParkingSpaces);

public sealed record UnsubscribeSearchAlertRequest(string? Token);
