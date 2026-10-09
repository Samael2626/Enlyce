using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public enum CommercialTaskStatus { Pending, Completed, Cancelled }
public enum CommercialTaskPriority { Low, Normal, High, Urgent }
public enum CommercialTaskType { Call, Message, Meeting, FollowUp, Other }

public sealed class CommercialTask
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public Guid? LeadId { get; private set; }
    public Guid AdvisorId { get; private set; }
    public CommercialTaskType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime DueAt { get; private set; }
    public DateTime? ReminderAt { get; private set; }
    public CommercialTaskPriority Priority { get; private set; }
    public CommercialTaskStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private CommercialTask() { }

    private CommercialTask(Guid id, Guid contactId, Guid? leadId, Guid advisorId,
        CommercialTaskType type, string title, string? description, DateTime dueAt,
        DateTime? reminderAt, CommercialTaskPriority priority, CommercialTaskStatus status,
        DateTime createdAt, DateTime updatedAt, DateTime? completedAt)
    {
        Id = id;
        ContactId = contactId;
        LeadId = leadId;
        AdvisorId = advisorId;
        Type = type;
        Title = title;
        Description = description;
        DueAt = dueAt;
        ReminderAt = reminderAt;
        Priority = priority;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        CompletedAt = completedAt;
    }

    public static CommercialTask Create(Guid contactId, Guid? leadId, Guid advisorId,
        CommercialTaskType type, string title, string? description, DateTime dueAt,
        DateTime? reminderAt, CommercialTaskPriority priority)
    {
        if (contactId == Guid.Empty || advisorId == Guid.Empty)
            throw new DomainError("El contacto y el asesor son obligatorios.");
        if (leadId == Guid.Empty)
            throw new DomainError("La oportunidad no es valida.");
        if (!Enum.IsDefined(type) || !Enum.IsDefined(priority))
            throw new DomainError("El tipo o la prioridad de la tarea no son validos.");
        ValidateTitle(title);
        ValidateTimes(dueAt, reminderAt);

        var now = DateTime.UtcNow;
        return new CommercialTask(Guid.NewGuid(), contactId, leadId, advisorId, type,
            title.Trim(), NormalizeDescription(description), dueAt, reminderAt, priority,
            CommercialTaskStatus.Pending, now, now, null);
    }

    public static CommercialTask Reconstitute(Guid id, Guid contactId, Guid? leadId,
        Guid advisorId, CommercialTaskType type, string title, string? description,
        DateTime dueAt, DateTime? reminderAt, CommercialTaskPriority priority,
        CommercialTaskStatus status, DateTime createdAt, DateTime updatedAt,
        DateTime? completedAt) =>
        new(id, contactId, leadId, advisorId, type, title, description, dueAt,
            reminderAt, priority, status, createdAt, updatedAt, completedAt);

    public void Reschedule(DateTime dueAt, DateTime? reminderAt)
    {
        EnsurePending();
        ValidateTimes(dueAt, reminderAt);
        DueAt = dueAt;
        ReminderAt = reminderAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        EnsurePending();
        Status = CommercialTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = CompletedAt.Value;
    }

    public void Cancel()
    {
        EnsurePending();
        Status = CommercialTaskStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (Status != CommercialTaskStatus.Pending)
            throw new DomainError("Solo se puede modificar una tarea pendiente.");
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainError("El titulo de la tarea es obligatorio.");
        if (title.Trim().Length > 200)
            throw new DomainError("El titulo no puede superar 200 caracteres.");
    }

    private static void ValidateTimes(DateTime dueAt, DateTime? reminderAt)
    {
        if (dueAt.Kind != DateTimeKind.Utc || reminderAt is { Kind: not DateTimeKind.Utc })
            throw new DomainError("Las fechas de la tarea deben estar en UTC.");
        if (reminderAt.HasValue && reminderAt > dueAt)
            throw new DomainError("El recordatorio no puede ser posterior al compromiso.");
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;
        if (description.Trim().Length > 2_000)
            throw new DomainError("La descripcion no puede superar 2000 caracteres.");
        return description.Trim();
    }
}

public sealed class CommercialTaskEvent
{
    public Guid Id { get; private set; }
    public Guid TaskId { get; private set; }
    public Guid ActorId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private CommercialTaskEvent() { }

    public static CommercialTaskEvent Create(Guid taskId, Guid actorId, string action, string? comment = null)
    {
        if (taskId == Guid.Empty || actorId == Guid.Empty)
            throw new DomainError("La tarea y el usuario son obligatorios.");
        if (string.IsNullOrWhiteSpace(action) || action.Trim().Length > 50)
            throw new DomainError("La accion de historial no es valida.");
        if (comment?.Trim().Length > 2_000)
            throw new DomainError("El comentario no puede superar 2000 caracteres.");

        return new CommercialTaskEvent
        {
            Id = Guid.NewGuid(), TaskId = taskId, ActorId = actorId,
            Action = action.Trim(), Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            OccurredAt = DateTime.UtcNow
        };
    }
}
