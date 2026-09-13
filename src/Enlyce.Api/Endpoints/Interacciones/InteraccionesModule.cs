using Enlyce.Application.Commands.Lead;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Interacciones;

public static class InteraccionesModule
{
    public static void MapInteracciones(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/interacciones").WithTags("Interacciones").RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] RegistrarInteraccionRequest request,
            HttpContext http,
            ILeadRepository leads,
            RegistrarInteraccionHandler handler) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, request.LeadId, leads) ||
                !EndpointAccess.CanActAsAdvisor(http.User, request.AsesorId))
                return Results.Forbid();

            var ok = await handler.HandleAsync(new RegistrarInteraccionCommand(
                request.LeadId, request.AsesorId, request.Tipo, request.Resumen));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .WithName("RegistrarInteraccion")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/lead/{leadId:guid}", async Task<IResult> (
            Guid leadId,
            HttpContext http,
            ILeadRepository leads,
            IInteraccionRepository repo) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, leadId, leads))
                return Results.Forbid();

            var interacciones = await repo.ObtenerPorLeadAsync(leadId);
            var advisorId = EndpointAccess.AdvisorId(http.User);
            return Results.Ok(advisorId is null
                ? interacciones
                : interacciones.Where(item => item.AsesorId == advisorId).ToList());
        })
        .WithName("ObtenerInteraccionesPorLead")
        .Produces<List<Domain.Entities.Interaccion>>();
    }
}

public record RegistrarInteraccionRequest(
    Guid LeadId, Guid AsesorId, string Tipo, string? Resumen);
