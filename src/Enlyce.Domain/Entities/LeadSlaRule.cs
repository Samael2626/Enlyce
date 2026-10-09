using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class LeadSlaRule
{
    public Guid Id { get; private set; }
    public string SourceKey { get; private set; } = string.Empty;
    public string OperationType { get; private set; } = string.Empty;
    public int FirstResponseHours { get; private set; }
    public int? InactivityDays { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private LeadSlaRule() { }

    public static LeadSlaRule Create(
        string sourceKey, string operationType, int firstResponseHours,
        int? inactivityDays, bool enabled, DateTime? updatedAtUtc = null)
    {
        var normalizedSource = NormalizeSourceKey(sourceKey);
        var normalizedOperation = NormalizeOperationType(operationType);
        ValidateThresholds(firstResponseHours, inactivityDays);
        var updatedAt = updatedAtUtc ?? DateTime.UtcNow;
        if (updatedAt.Kind != DateTimeKind.Utc)
            throw new DomainError("La fecha de actualizacion debe estar en UTC.");

        return new LeadSlaRule
        {
            Id = Guid.NewGuid(),
            SourceKey = normalizedSource,
            OperationType = normalizedOperation,
            FirstResponseHours = firstResponseHours,
            InactivityDays = inactivityDays,
            Enabled = enabled,
            UpdatedAtUtc = updatedAt
        };
    }

    public void Update(int firstResponseHours, int? inactivityDays, bool enabled)
    {
        ValidateThresholds(firstResponseHours, inactivityDays);
        FirstResponseHours = firstResponseHours;
        InactivityDays = inactivityDays;
        Enabled = enabled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static string NormalizeSourceKey(string sourceKey)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
            throw new DomainError("La fuente es obligatoria.");

        var normalized = sourceKey.Trim();
        if (normalized != "*")
        {
            var utmIndex = normalized.IndexOf("|utm=", StringComparison.OrdinalIgnoreCase);
            var reviewIndex = normalized.IndexOf("|PublicationReview:", StringComparison.OrdinalIgnoreCase);
            var suffixIndex = utmIndex < 0 ? reviewIndex
                : reviewIndex < 0 ? utmIndex
                : Math.Min(utmIndex, reviewIndex);
            if (suffixIndex >= 0)
                normalized = normalized[..suffixIndex].Trim();

            normalized = normalized.ToUpperInvariant();
        }

        if (normalized.Length is < 1 or > 100)
            throw new DomainError("La fuente normalizada debe tener entre 1 y 100 caracteres.");
        return normalized;
    }

    public static string NormalizeOperationType(string operationType)
    {
        if (string.IsNullOrWhiteSpace(operationType))
            throw new DomainError("El tipo de operacion es obligatorio.");

        return operationType.Trim().ToUpperInvariant() switch
        {
            "*" => "*",
            "VENTA" => "Venta",
            "ARRIENDO" => "Arriendo",
            _ => throw new DomainError("El tipo de operacion debe ser *, Venta o Arriendo.")
        };
    }

    private static void ValidateThresholds(int firstResponseHours, int? inactivityDays)
    {
        if (firstResponseHours is < 1 or > 720)
            throw new DomainError("Las horas de primera respuesta deben estar entre 1 y 720.");
        if (inactivityDays is < 1 or > 90)
            throw new DomainError("Los dias de inactividad deben estar entre 1 y 90.");
    }
}
