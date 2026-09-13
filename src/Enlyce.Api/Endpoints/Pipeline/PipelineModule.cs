using Enlyce.Application.Commands.Lead;
using Enlyce.Application.Queries.Lead;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Pipeline;

public static class PipelineModule
{
    public static void MapPipeline(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pipeline").WithTags("Pipeline").RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [AsParameters] ConsultarPipelineRequest query,
            HttpContext http,
            ConsultarPipelineHandler handler) =>
        {
            var advisorId = http.User.IsInRole("Administrador")
                ? null
                : EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            var result = await handler.HandleAsync(new ConsultarPipelineQuery(query.Etapa, advisorId));
            return Results.Ok(result);
        })
        .WithName("ConsultarPipeline")
        .Produces<PipelineResponse>();

        group.MapPut("/{id:guid}/mover-etapa", async Task<IResult> (
            Guid id,
            [FromBody] MoverEtapaRequest request,
            HttpContext http,
            ILeadRepository leads,
            MoverEtapaHandler handler) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, id, leads))
                return Results.Forbid();

            var ok = await handler.HandleAsync(new MoverEtapaCommand(id, request.NuevaEtapa));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .WithName("MoverEtapa")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/asignar", async (
            Guid id,
            [FromBody] AsignarLeadRequest request,
            AsignarLeadHandler handler) =>
        {
            var ok = await handler.HandleAsync(new AsignarLeadCommand(id, request.AsesorId));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .RequireAuthorization("Administrador")
        .WithName("AsignarLead")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/reasignar", async (
            Guid id,
            [FromBody] ReasignarLeadRequest request,
            ReasignarLeadHandler handler) =>
        {
            var ok = await handler.HandleAsync(new ReasignarLeadCommand(id, request.NuevoAsesorId));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .RequireAuthorization("Administrador")
        .WithName("ReasignarLead")
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public record ConsultarPipelineRequest(string? Etapa = null);
public record MoverEtapaRequest(string NuevaEtapa);
public record AsignarLeadRequest(Guid AsesorId);
public record ReasignarLeadRequest(Guid NuevoAsesorId);
