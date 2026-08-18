using Enlyce.Application.Commands.Lead;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Visitas;

public static class VisitasModule
{
    public static void MapVisitas(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/visitas").WithTags("Visitas");

        group.MapPost("/", async (
            [FromBody] RegistrarVisitaRequest request,
            RegistrarVisitaHandler handler) =>
        {
            var ok = await handler.HandleAsync(new RegistrarVisitaCommand(
                request.LeadId, request.InmuebleId, request.AsesorId, request.FechaProgramada));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .WithName("RegistrarVisita")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/lead/{leadId:guid}", async (
            Guid leadId,
            IVisitaRepository repo) =>
        {
            var visitas = await repo.ObtenerPorLeadAsync(leadId);
            return Results.Ok(visitas);
        })
        .WithName("ObtenerVisitasPorLead")
        .Produces<List<Domain.Entities.Visita>>();

        group.MapGet("/asesor/{asesorId:guid}", async (
            Guid asesorId,
            [AsParameters] ConsultarVisitasAsesorRequest query,
            IVisitaRepository repo) =>
        {
            var visitas = await repo.ObtenerPorAsesorAsync(asesorId, query.Desde);
            return Results.Ok(visitas);
        })
        .WithName("ObtenerVisitasPorAsesor")
        .Produces<List<Domain.Entities.Visita>>();
    }
}

public record RegistrarVisitaRequest(
    Guid LeadId, Guid InmuebleId, Guid AsesorId, DateTime FechaProgramada);

public record ConsultarVisitasAsesorRequest(DateTime Desde);
