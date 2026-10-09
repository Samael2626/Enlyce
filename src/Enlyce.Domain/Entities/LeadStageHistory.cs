using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class LeadStageHistory
{
    public Guid Id { get; private set; }
    public Guid LeadId { get; private set; }
    public string PreviousStage { get; private set; } = string.Empty;
    public string NewStage { get; private set; } = string.Empty;
    public Guid ActorId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime ChangedAt { get; private set; }

    private LeadStageHistory() { }

    public static LeadStageHistory Create(
        Guid leadId,
        string previousStage,
        string newStage,
        Guid actorId,
        string reason,
        DateTime? changedAt = null)
    {
        if (leadId == Guid.Empty || actorId == Guid.Empty)
            throw new DomainError("Los identificadores del historial de etapa no son validos.");
        if (string.IsNullOrWhiteSpace(previousStage) || string.IsNullOrWhiteSpace(newStage) ||
            previousStage.Length > 80 || newStage.Length > 80 || previousStage == newStage)
            throw new DomainError("El cambio de etapa no es valido.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
            throw new DomainError("El motivo es obligatorio y no puede superar 500 caracteres.");

        var timestamp = changedAt ?? DateTime.UtcNow;
        if (timestamp.Kind != DateTimeKind.Utc)
            throw new DomainError("La fecha del historial debe estar en UTC.");

        return new LeadStageHistory
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            PreviousStage = previousStage.Trim(),
            NewStage = newStage.Trim(),
            ActorId = actorId,
            Reason = reason.Trim(),
            ChangedAt = timestamp
        };
    }
}
