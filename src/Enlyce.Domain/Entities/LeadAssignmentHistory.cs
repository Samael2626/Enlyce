using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public enum LeadAssignmentSource
{
    Publication,
    AutomaticLoadBalance,
    AutomaticRoundRobin,
    ManualAssignment,
    ManualReassignment,
    Legacy
}

public sealed class LeadAssignmentHistory
{
    public Guid Id { get; private set; }
    public Guid LeadId { get; private set; }
    public Guid? PreviousAdvisorId { get; private set; }
    public Guid NewAdvisorId { get; private set; }
    public Guid? ChangedByAdvisorId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public LeadAssignmentSource Source { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private LeadAssignmentHistory() { }

    public static LeadAssignmentHistory Create(
        Guid leadId,
        Guid? previousAdvisorId,
        Guid newAdvisorId,
        Guid? changedByAdvisorId,
        string reason,
        LeadAssignmentSource source,
        DateTime? changedAt = null)
    {
        if (leadId == Guid.Empty || newAdvisorId == Guid.Empty ||
            previousAdvisorId == Guid.Empty || changedByAdvisorId == Guid.Empty)
            throw new DomainError("Los identificadores del historial de asignacion no son validos.");
        if (!Enum.IsDefined(source))
            throw new DomainError("El origen de asignacion no es valido.");
        var isManual = source is LeadAssignmentSource.ManualAssignment or LeadAssignmentSource.ManualReassignment;
        if (isManual != changedByAdvisorId.HasValue)
            throw new DomainError("El actor debe corresponder al origen del cambio.");
        if (source == LeadAssignmentSource.ManualAssignment && previousAdvisorId.HasValue ||
            source == LeadAssignmentSource.ManualReassignment && !previousAdvisorId.HasValue)
            throw new DomainError("El tipo de cambio no corresponde al asesor anterior.");
        if (previousAdvisorId == newAdvisorId)
            throw new DomainError("El historial requiere un cambio real de responsable.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
            throw new DomainError("El motivo es obligatorio y no puede superar 500 caracteres.");

        var timestamp = changedAt ?? DateTime.UtcNow;
        if (timestamp.Kind != DateTimeKind.Utc)
            throw new DomainError("La fecha del historial debe estar en UTC.");

        return new LeadAssignmentHistory
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            PreviousAdvisorId = previousAdvisorId,
            NewAdvisorId = newAdvisorId,
            ChangedByAdvisorId = changedByAdvisorId,
            Reason = reason.Trim(),
            Source = source,
            ChangedAt = timestamp
        };
    }
}
