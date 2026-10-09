using Enlyce.Application.Commands.Lead;
using Enlyce.Application.Queries.Lead;
using Enlyce.Domain.Errors;
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
            ConsultarPipelineHandler handler,
            CancellationToken ct) =>
        {
            var advisorId = http.User.IsInRole("Administrador")
                ? null
                : EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            if (query.Desde is not null && query.Hasta is not null && query.Desde > query.Hasta)
                return Results.BadRequest(new { message = "El rango de fechas no es válido." });
            if (query.Page < 1 || query.PageSize is < 1 or > 100)
                return Results.BadRequest(new { message = "La página debe ser positiva y pageSize debe estar entre 1 y 100." });

            var assignedAdvisorId = http.User.IsInRole("Administrador") ? query.AsesorId : advisorId;
            var result = await handler.HandleAsync(new ConsultarPipelineQuery(
                query.Etapa, advisorId, query.Q, query.Operacion,
                assignedAdvisorId, query.Desde, query.Hasta, query.Page, query.PageSize), ct);
            return Results.Ok(result);
        })
        .WithName("ConsultarPipeline")
        .Produces<PipelineResponse>();

        group.MapGet("/{id:guid}/asignaciones", async Task<IResult> (
            Guid id,
            HttpContext http,
            ILeadRepository leads,
            ILeadAssignmentHistoryRepository history,
            IAsesorRepository advisors,
            CancellationToken ct) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, id, leads))
                return Results.Forbid();

            var records = await history.GetByLeadIdAsync(id, ct);
            var advisorNames = (await advisors.ObtenerTodosAsync())
                .ToDictionary(advisor => advisor.Id, advisor => advisor.Nombre);
            return Results.Ok(records.Select(item => new LeadAssignmentHistoryDto(
                item.Id,
                item.PreviousAdvisorId,
                item.PreviousAdvisorId is Guid previousId && advisorNames.TryGetValue(previousId, out var previousName) ? previousName : null,
                item.NewAdvisorId,
                advisorNames.GetValueOrDefault(item.NewAdvisorId),
                item.ChangedByAdvisorId,
                item.ChangedByAdvisorId is Guid actorId && advisorNames.TryGetValue(actorId, out var actorName) ? actorName : null,
                item.Reason,
                item.Source.ToString(),
                item.ChangedAt)));
        })
        .WithName("GetLeadAssignmentHistory");

        group.MapPut("/{id:guid}/mover-etapa", async Task<IResult> (
            Guid id,
            [FromBody] MoverEtapaRequest request,
            HttpContext http,
            ILeadRepository leads,
            MoverEtapaHandler handler) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, id, leads))
                return Results.Forbid();

            var actorId = EndpointAccess.ActorId(http.User);
            if (actorId is null)
                return Results.Forbid();

            var ok = await handler.HandleAsync(new MoverEtapaCommand(
                id, request.NuevaEtapa, actorId.Value, request.Reason));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .WithName("MoverEtapa")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/asignar-masivo", async Task<IResult> (
            [FromBody] BulkLeadAssignmentRequest request,
            HttpContext http,
            AsignarLeadHandler handler,
            CancellationToken ct) =>
        {
            if (request.LeadIds is null || request.LeadIds.Length == 0 || request.LeadIds.Length > 100)
                return Results.BadRequest(new { message = "Envía entre 1 y 100 oportunidades." });
            if (request.LeadIds.Any(id => id == Guid.Empty) || request.LeadIds.Distinct().Count() != request.LeadIds.Length)
                return Results.BadRequest(new { message = "Los identificadores deben ser válidos y únicos." });
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Results.BadRequest(new { message = "La razón es obligatoria." });

            var actorId = EndpointAccess.ActorId(http.User);
            if (actorId is null)
                return Results.Forbid();

            var results = new List<BulkLeadAssignmentItem>(request.LeadIds.Length);
            foreach (var leadId in request.LeadIds)
            {
                try
                {
                    var success = await handler.HandleAsync(
                        new AsignarLeadCommand(leadId, request.AsesorId, actorId.Value, request.Reason), ct);
                    results.Add(new BulkLeadAssignmentItem(
                        leadId, success, success ? null : "Oportunidad no encontrada."));
                }
                catch (DomainError error)
                {
                    results.Add(new BulkLeadAssignmentItem(leadId, false, error.Message));
                }
            }

            return Results.Ok(new BulkLeadAssignmentResponse(
                results,
                results.Count(item => item.Success),
                results.Count(item => !item.Success)));
        })
        .RequireAuthorization("Administrador")
        .WithName("AsignarLeadsMasivo")
        .Produces<BulkLeadAssignmentResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/asignar", async (
            Guid id,
            [FromBody] AsignarLeadRequest request,
            HttpContext http,
            AsignarLeadHandler handler) =>
        {
            var actorId = EndpointAccess.ActorId(http.User);
            if (actorId is null)
                return Results.Forbid();

            var ok = await handler.HandleAsync(new AsignarLeadCommand(id, request.AsesorId, actorId.Value, request.Reason));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .RequireAuthorization("Administrador")
        .WithName("AsignarLead")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/reasignar", async (
            Guid id,
            [FromBody] ReasignarLeadRequest request,
            HttpContext http,
            ReasignarLeadHandler handler) =>
        {
            var actorId = EndpointAccess.ActorId(http.User);
            if (actorId is null)
                return Results.Forbid();

            var ok = await handler.HandleAsync(new ReasignarLeadCommand(id, request.NuevoAsesorId, actorId.Value, request.Reason));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .RequireAuthorization("Administrador")
        .WithName("ReasignarLead")
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public record ConsultarPipelineRequest(
    string? Etapa = null,
    string? Q = null,
    string? Operacion = null,
    Guid? AsesorId = null,
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    int Page = 1,
    int PageSize = 50);
public record MoverEtapaRequest(string NuevaEtapa, string Reason = "Cambio desde pipeline");
public record AsignarLeadRequest(Guid AsesorId, string Reason);
public record ReasignarLeadRequest(Guid NuevoAsesorId, string Reason);
public sealed record BulkLeadAssignmentRequest(Guid[]? LeadIds, Guid AsesorId, string Reason);
public sealed record BulkLeadAssignmentItem(Guid LeadId, bool Success, string? Error);
public sealed record BulkLeadAssignmentResponse(
    IReadOnlyList<BulkLeadAssignmentItem> Results,
    int Succeeded,
    int Failed);
public record LeadAssignmentHistoryDto(Guid Id, Guid? PreviousAdvisorId, string? PreviousAdvisorName,
    Guid NewAdvisorId, string? NewAdvisorName, Guid? ChangedByAdvisorId, string? ChangedByName,
    string Reason, string Source, DateTime ChangedAt);
