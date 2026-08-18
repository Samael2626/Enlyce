using Enlyce.Application.Commands.Lead;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Interacciones;

public static class InteraccionesModule
{
    public static void MapInteracciones(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/interacciones").WithTags("Interacciones");

        group.MapPost("/", async (
            [FromBody] RegistrarInteraccionRequest request,
            RegistrarInteraccionHandler handler) =>
        {
            var ok = await handler.HandleAsync(new RegistrarInteraccionCommand(
                request.LeadId, request.AsesorId, request.Tipo, request.Resumen));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .WithName("RegistrarInteraccion")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/lead/{leadId:guid}", async (
            Guid leadId,
            IInteraccionRepository repo) =>
        {
            var interacciones = await repo.ObtenerPorLeadAsync(leadId);
            return Results.Ok(interacciones);
        })
        .WithName("ObtenerInteraccionesPorLead")
        .Produces<List<Domain.Entities.Interaccion>>();
    }
}

public record RegistrarInteraccionRequest(
    Guid LeadId, Guid AsesorId, string Tipo, string? Resumen);
