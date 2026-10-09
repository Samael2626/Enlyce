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
            if (request.Page < 1 || request.PageSize is < 1 or > 100 ||
                ((long)request.Page - 1) * request.PageSize > int.MaxValue)
                return Results.BadRequest(new { error = "La pagina debe ser positiva y pageSize debe estar entre 1 y 100." });
            if (request.Query?.Length > 200)
                return Results.BadRequest(new { error = "La busqueda no puede superar 200 caracteres." });
            if (request.Status is not null && !TryParse(request.Status, out CommercialTaskStatus _))
                return Results.BadRequest(new { error = "El estado no es valido." });
            if (request.Priority is not null && !TryParse(request.Priority, out CommercialTaskPriority _))
                return Results.BadRequest(new { error = "La prioridad no es valida." });

            var advisorId = http.User.IsInRole("Administrador") ? null : EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            if (request.ContactId.HasValue && advisorId.HasValue &&
                !(await leads.GetByContactIdAsync(request.ContactId.Value, ct))
                    .Any(lead => lead.AsesorAsignadoId == advisorId))
                return Results.Forbid();

            var result = await tasks.SearchAsync(advisorId, request.Query,
                request.Status is null ? null : Enum.Parse<CommercialTaskStatus>(request.Status, true),
                request.Priority is null ? null : Enum.Parse<CommercialTaskPriority>(request.Priority, true),
                request.From, request.To, request.ContactId, request.Page, request.PageSize, ct);
            return Results.Ok(new TaskSearchResponse(result.Total, result.Items.Select(ToDto).ToArray()));
        })
        .WithName("ListCommercialTasks");

        group.MapPost("/bulk/completar", (
            [FromBody] BulkTaskRequest request,
            HttpContext http,
            ICommercialTaskRepository tasks,
            CancellationToken ct) => ApplyBulkActionAsync(request, true, http, tasks, ct))
        .WithName("BulkCompleteCommercialTasks");

        group.MapPost("/bulk/cancelar", (
            [FromBody] BulkTaskRequest request,
            HttpContext http,
            ICommercialTaskRepository tasks,
            CancellationToken ct) => ApplyBulkActionAsync(request, false, http, tasks, ct))
        .WithName("BulkCancelCommercialTasks");

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

    private static async Task<IResult> ApplyBulkActionAsync(
        BulkTaskRequest request, bool complete, HttpContext http,
        ICommercialTaskRepository tasks, CancellationToken ct)
    {
        if (request.Ids is null || request.Ids.Count is < 1 or > 100 || request.Ids.Contains(Guid.Empty))
            return Results.BadRequest(new { error = "Se requieren entre 1 y 100 IDs de tareas validos." });

        var actorId = GetActorId(http.User);
        var isAdministrator = http.User.IsInRole("Administrador");
        var advisorId = isAdministrator ? null : EndpointAccess.AdvisorId(http.User);
        if (actorId is null || (!isAdministrator && (!http.User.IsInRole("Asesor") || advisorId is null)))
            return Results.Forbid();

        var eligibleTasks = await tasks.GetByIdsAsync(request.Ids.Distinct().ToArray(), advisorId, ct);
        var taskById = eligibleTasks.ToDictionary(task => task.Id);
        var changedTasks = new List<CommercialTask>();
        var events = new List<CommercialTaskEvent>();
        var seen = new HashSet<Guid>();
        var results = new List<BulkTaskResult>(request.Ids.Count);

        foreach (var id in request.Ids)
        {
            if (!seen.Add(id))
            {
                results.Add(new BulkTaskResult(id, "Duplicate"));
                continue;
            }
            if (!taskById.TryGetValue(id, out var task))
            {
                results.Add(new BulkTaskResult(id, "NotFound"));
                continue;
            }
            if (task.Status != CommercialTaskStatus.Pending)
            {
                results.Add(new BulkTaskResult(id, "NotPending"));
                continue;
            }

            if (complete)
                task.Complete();
            else
                task.Cancel();
            changedTasks.Add(task);
            events.Add(CommercialTaskEvent.Create(id, actorId.Value,
                complete ? "Completed" : "Cancelled", "Accion masiva"));
            results.Add(new BulkTaskResult(id, complete ? "Completed" : "Cancelled"));
        }

        if (changedTasks.Count > 0)
            await tasks.SaveManyAsync(changedTasks, events, ct);
        return Results.Ok(new BulkTaskResponse(results));
    }

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

public sealed record ListTasksRequest(
    string? Query = null, string? Status = null, string? Priority = null,
    DateTime? From = null, DateTime? To = null, Guid? ContactId = null,
    int Page = 1, int PageSize = 20);
public sealed record TaskSearchResponse(int Total, IReadOnlyList<CommercialTaskDto> Items);
public sealed record BulkTaskRequest(IReadOnlyList<Guid> Ids);
public sealed record BulkTaskResult(Guid Id, string Result);
public sealed record BulkTaskResponse(IReadOnlyList<BulkTaskResult> Results);
public sealed record CreateTaskRequest(Guid ContactId, Guid? LeadId, Guid? AdvisorId, string Type,
    string Title, string? Description, DateTime DueAt, DateTime? ReminderAt, string Priority);
public sealed record RescheduleTaskRequest(DateTime DueAt, DateTime? ReminderAt);
public sealed record AddTaskCommentRequest(string Comment);
public sealed record CommercialTaskDto(Guid Id, Guid ContactId, Guid? LeadId, Guid AdvisorId,
    string Type, string Title, string? Description, DateTime DueAt, DateTime? ReminderAt,
    string Priority, string Status, DateTime CreatedAt, DateTime UpdatedAt, DateTime? CompletedAt);
public sealed record CommercialTaskEventDto(Guid Id, Guid TaskId, Guid ActorId, string Action,
    string? Comment, DateTime OccurredAt);
