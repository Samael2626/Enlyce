using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Enlyce.Application.Commands.Lead;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Visitas;

public static class VisitasModule
{
    public static void MapVisitas(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/visitas").WithTags("Visitas");

        group.MapGet("/", async Task<IResult> (
            HttpContext http,
            IVisitaRepository repo) =>
        {
            if (http.User.IsInRole("Administrador"))
                return Results.Ok(await repo.ObtenerVisitasAsync(null));

            if (!http.User.IsInRole("Asesor"))
                return Results.Forbid();

            var idClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? http.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(idClaim, out var advisorId))
                return Results.Forbid();

            return Results.Ok(await repo.ObtenerVisitasAsync(advisorId));
        })
        .RequireAuthorization()
        .WithName("ObtenerVisitas")
        .Produces<List<Domain.Entities.Visita>>();

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
