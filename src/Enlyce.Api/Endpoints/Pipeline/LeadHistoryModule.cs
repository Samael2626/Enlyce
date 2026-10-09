using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.Pipeline;

public static class LeadHistoryModule
{
    public static void MapLeadHistory(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pipeline/{id:guid}/historial", async Task<IResult> (
            Guid id,
            HttpContext http,
            ILeadRepository leads,
            ILeadAssignmentHistoryRepository assignments,
            ILeadStageHistoryRepository stages,
            IAsesorRepository advisors,
            CancellationToken ct) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, id, leads))
                return Results.Forbid();

            var names = (await advisors.ObtenerTodosAsync()).ToDictionary(item => item.Id, item => item.Nombre);
            var assignmentEvents = (await assignments.GetByLeadIdAsync(id, ct)).Select(item => new LeadHistoryDto(
                item.Id,
                "Asignacion",
                item.PreviousAdvisorId is Guid previous && names.TryGetValue(previous, out var previousName) ? previousName : "Sin asesor",
                names.GetValueOrDefault(item.NewAdvisorId, "Asesor no disponible"),
                item.ChangedByAdvisorId is Guid actor && names.TryGetValue(actor, out var actorName) ? actorName : "Sistema",
                item.Reason,
                item.ChangedAt));
            var stageEvents = (await stages.GetByLeadIdAsync(id, ct)).Select(item => new LeadHistoryDto(
                item.Id,
                "Etapa",
                item.PreviousStage,
                item.NewStage,
                names.GetValueOrDefault(item.ActorId, "Usuario no disponible"),
                item.Reason,
                item.ChangedAt));

            return Results.Ok(assignmentEvents.Concat(stageEvents)
                .OrderBy(item => item.OccurredAt)
                .ThenBy(item => item.Id));
        })
        .RequireAuthorization()
        .WithName("GetLeadHistory")
        .Produces<IReadOnlyList<LeadHistoryDto>>();
    }
}

public sealed record LeadHistoryDto(
    Guid Id,
    string Type,
    string From,
    string To,
    string Actor,
    string Reason,
    DateTime OccurredAt);
