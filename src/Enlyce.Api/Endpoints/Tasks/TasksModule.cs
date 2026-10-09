using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Tasks;

public static class TasksModule
{
    public static void MapTasks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tareas").WithTags("Tareas comerciales").RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [AsParameters] ListTasksRequest request,
            HttpContext http,
            ICommercialTaskRepository tasks,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            if (!ValidRange(request.From, request.To))
                return Results.BadRequest(new { error = "El rango debe usar fechas UTC y no superar 366 dias." });

            var advisorId = http.User.IsInRole("Administrador") ? null : EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            if (request.ContactId.HasValue && advisorId.HasValue &&
                !(await leads.GetByContactIdAsync(request.ContactId.Value, ct))
                    .Any(lead => lead.AsesorAsignadoId == advisorId))
                return Results.Forbid();

            var items = await tasks.GetForAdvisorAsync(advisorId, request.From, request.To, request.ContactId, ct);
            return Results.Ok(items.Select(ToDto));
        })
        .WithName("ListCommercialTasks");

        group.MapGet("/alertas", async Task<IResult> (
            DateTime? through, HttpContext http, ICommercialTaskRepository tasks, CancellationToken ct) =>
        {
            var advisorId = http.User.IsInRole("Administrador") ? null : EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();
            var cutoff = through ?? DateTime.UtcNow.AddDays(7);
            if (cutoff.Kind != DateTimeKind.Utc || cutoff > DateTime.UtcNow.AddDays(31))
                return Results.BadRequest(new { error = "La fecha limite debe estar en UTC y dentro de 31 dias." });

            var items = await tasks.GetPendingAlertsAsync(advisorId, cutoff, ct);
            return Results.Ok(items.Select(ToDto));
        }).WithName("ListCommercialTaskAlerts");

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateTaskRequest request,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            IAsesorRepository advisors,
            ICommercialTaskRepository tasks,
            CancellationToken ct) =>
        {
            var actorId = GetActorId(http.User);
            var relatedLeads = await leads.GetByContactIdAsync(request.ContactId, ct);
            var relatedLead = request.LeadId.HasValue
                ? relatedLeads.SingleOrDefault(item => item.Id == request.LeadId.Value)
                : null;
            if (request.LeadId.HasValue && relatedLead is null)
                return Results.BadRequest(new { error = "La oportunidad no pertenece al contacto." });

            var advisorId = EndpointAccess.AdvisorId(http.User) ?? request.AdvisorId ?? relatedLead?.AsesorAsignadoId;
            if (actorId is null || advisorId is null ||
                await advisors.ObtenerPorIdAsync(advisorId.Value) is not { Activo: true, Rol: "Asesor" })
                return Results.BadRequest(new { error = "El asesor asignado no es valido." });

            if (await contacts.GetByIdAsync(request.ContactId, ct) is null)
                return Results.NotFound();

            if (!http.User.IsInRole("Administrador") && !relatedLeads.Any(lead => lead.AsesorAsignadoId == actorId))
                return Results.Forbid();

            if (request.LeadId.HasValue)
            {
                if (!await EndpointAccess.CanAccessLeadAsync(http.User, relatedLead!.Id, leads))
                    return Results.Forbid();
            }

            if (!TryParse(request.Type, out CommercialTaskType type) ||
                !TryParse(request.Priority, out CommercialTaskPriority priority))
                return Results.BadRequest(new { error = "El tipo o la prioridad no son validos." });

            var task = CommercialTask.Create(request.ContactId, request.LeadId, advisorId.Value,
                type, request.Title, request.Description, request.DueAt, request.ReminderAt, priority);
            var taskEvent = CommercialTaskEvent.Create(task.Id, actorId.Value, "Created");
            await tasks.AddAsync(task, taskEvent, ct);
            return Results.Created($"/api/tareas/{task.Id}", ToDto(task));
        })
        .WithName("CreateCommercialTask");

        group.MapPut("/{id:guid}/reprogramar", async Task<IResult> (
            Guid id,
            [FromBody] RescheduleTaskRequest request,
            HttpContext http,
            ICommercialTaskRepository tasks,
            CancellationToken ct) =>
        {
            var task = await tasks.GetByIdAsync(id, ct);
            if (task is null)
                return Results.NotFound();
            if (!CanAccessTask(http.User, task))
                return Results.Forbid();

            task.Reschedule(request.DueAt, request.ReminderAt);
            await tasks.SaveAsync(task, CommercialTaskEvent.Create(id, GetActorId(http.User)!.Value, "Rescheduled"), ct);
            return Results.Ok(ToDto(task));
        })
        .WithName("RescheduleCommercialTask");

        group.MapPut("/{id:guid}/completar", async Task<IResult> (
            Guid id,
            HttpContext http,
            ICommercialTaskRepository tasks,
            CancellationToken ct) =>
        {
            var task = await tasks.GetByIdAsync(id, ct);
            if (task is null)
                return Results.NotFound();
            if (!CanAccessTask(http.User, task))
                return Results.Forbid();

            task.Complete();
            await tasks.SaveAsync(task, CommercialTaskEvent.Create(id, GetActorId(http.User)!.Value, "Completed"), ct);
            return Results.Ok(ToDto(task));
        })
        .WithName("CompleteCommercialTask");

        group.MapPut("/{id:guid}/cancelar", async Task<IResult> (
            Guid id,
            HttpContext http,
            ICommercialTaskRepository tasks,
            CancellationToken ct) =>
        {
            var task = await tasks.GetByIdAsync(id, ct);
            if (task is null)
                return Results.NotFound();
            if (!CanAccessTask(http.User, task))
                return Results.Forbid();

            task.Cancel();
            await tasks.SaveAsync(task, CommercialTaskEvent.Create(id, GetActorId(http.User)!.Value, "Cancelled"), ct);
            return Results.Ok(ToDto(task));
        })
        .WithName("CancelCommercialTask");

        group.MapPost("/{id:guid}/comentarios", async Task<IResult> (
            Guid id,
            [FromBody] AddTaskCommentRequest request,
            HttpContext http,
            ICommercialTaskRepository tasks,
            CancellationToken ct) =>
        {
            var task = await tasks.GetByIdAsync(id, ct);
            if (task is null)
                return Results.NotFound();
            if (!CanAccessTask(http.User, task))
                return Results.Forbid();

            var item = CommercialTaskEvent.Create(id, GetActorId(http.User)!.Value, "Comment", request.Comment);
            await tasks.AddEventAsync(item, ct);
            return Results.Created($"/api/tareas/{id}/historial", ToEventDto(item));
        })
        .WithName("AddCommercialTaskComment");

        group.MapGet("/{id:guid}/historial", async Task<IResult> (
            Guid id,
            HttpContext http,
            ICommercialTaskRepository tasks,
            CancellationToken ct) =>
        {
            var task = await tasks.GetByIdAsync(id, ct);
            if (task is null)
                return Results.NotFound();
            if (!CanAccessTask(http.User, task))
                return Results.Forbid();

            var events = await tasks.GetEventsAsync(id, ct);
            return Results.Ok(events.Select(ToEventDto));
        })
        .WithName("GetCommercialTaskHistory");
    }

    private static CommercialTaskDto ToDto(CommercialTask task) => new(
        task.Id, task.ContactId, task.LeadId, task.AdvisorId, task.Type.ToString(), task.Title,
        task.Description, task.DueAt, task.ReminderAt, task.Priority.ToString(),
        task.Status.ToString(), task.CreatedAt, task.UpdatedAt, task.CompletedAt);

    private static CommercialTaskEventDto ToEventDto(CommercialTaskEvent item) =>
        new(item.Id, item.TaskId, item.ActorId, item.Action, item.Comment, item.OccurredAt);

    private static bool CanAccessTask(ClaimsPrincipal user, CommercialTask task) =>
        user.IsInRole("Administrador") || EndpointAccess.AdvisorId(user) == task.AdvisorId;

    private static Guid? GetActorId(ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static bool ValidRange(DateTime? from, DateTime? to) =>
        (!from.HasValue || from.Value.Kind == DateTimeKind.Utc) &&
        (!to.HasValue || to.Value.Kind == DateTimeKind.Utc) &&
        (!from.HasValue || !to.HasValue || to.Value > from.Value && to.Value - from.Value <= TimeSpan.FromDays(366));

    private static bool TryParse<T>(string value, out T parsed) where T : struct, Enum =>
        Enum.TryParse(value, true, out parsed) && Enum.IsDefined(parsed);
}

public sealed record ListTasksRequest(DateTime? From = null, DateTime? To = null, Guid? ContactId = null);
public sealed record CreateTaskRequest(Guid ContactId, Guid? LeadId, Guid? AdvisorId, string Type,
    string Title, string? Description, DateTime DueAt, DateTime? ReminderAt, string Priority);
public sealed record RescheduleTaskRequest(DateTime DueAt, DateTime? ReminderAt);
public sealed record AddTaskCommentRequest(string Comment);
public sealed record CommercialTaskDto(Guid Id, Guid ContactId, Guid? LeadId, Guid AdvisorId,
    string Type, string Title, string? Description, DateTime DueAt, DateTime? ReminderAt,
    string Priority, string Status, DateTime CreatedAt, DateTime UpdatedAt, DateTime? CompletedAt);
public sealed record CommercialTaskEventDto(Guid Id, Guid TaskId, Guid ActorId, string Action,
    string? Comment, DateTime OccurredAt);
