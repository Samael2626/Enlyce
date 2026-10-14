namespace Enlyce.Domain.Entities;

public sealed class ContactEmailPermissionEvent
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public bool Allowed { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string? EvidenceReference { get; private set; }
    public string ActorId { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    private ContactEmailPermissionEvent() { }

    public static ContactEmailPermissionEvent Create(Guid contactId, bool allowed, string source,
        string? evidenceReference, string actorId)
    {
        if (contactId == Guid.Empty || string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("Contacto y actor son obligatorios.");
        if (string.IsNullOrWhiteSpace(source) || source.Trim().Length > 50)
            throw new ArgumentException("La fuente debe tener entre 1 y 50 caracteres.");
        if (allowed && string.IsNullOrWhiteSpace(evidenceReference))
            throw new ArgumentException("Para conceder el permiso se requiere referencia de evidencia.");
        if (evidenceReference?.Trim().Length > 500)
            throw new ArgumentException("La referencia no puede superar 500 caracteres.");

        return new ContactEmailPermissionEvent
        {
            Id = Guid.NewGuid(), ContactId = contactId, Allowed = allowed, Source = source.Trim(),
            EvidenceReference = string.IsNullOrWhiteSpace(evidenceReference) ? null : evidenceReference.Trim(),
            ActorId = actorId.Trim(), OccurredAt = DateTime.UtcNow
        };
    }
}
