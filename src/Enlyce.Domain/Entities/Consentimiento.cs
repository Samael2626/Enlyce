using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class Consentimiento
{
    public Guid Id { get; private set; }
    public Guid LeadId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string? DireccionIp { get; private set; }
    public string TextoConsentido { get; private set; } = string.Empty;
    public string VersionPolitica { get; private set; } = string.Empty;
    public string Metodo { get; private set; } = string.Empty;

    private Consentimiento() { }

    public static Consentimiento Registrar(
        Guid leadId,
        string textoConsentido,
        string versionPolitica,
        string metodo,
        string? direccionIp = null)
    {
        if (leadId == Guid.Empty)
            throw new DomainError("El lead_id es obligatorio.");
        if (string.IsNullOrWhiteSpace(textoConsentido))
            throw new DomainError("El texto consentido es obligatorio.");
        if (string.IsNullOrWhiteSpace(versionPolitica))
            throw new DomainError("La version de politica es obligatoria.");

        return new Consentimiento
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            Fecha = DateTime.UtcNow,
            DireccionIp = direccionIp,
            TextoConsentido = textoConsentido,
            VersionPolitica = versionPolitica,
            Metodo = metodo
        };
    }

    public static Consentimiento Reconstituir(
        Guid id, Guid leadId, DateTime fecha, string? direccionIp,
        string textoConsentido, string versionPolitica, string metodo)
    {
        return new Consentimiento
        {
            Id = id,
            LeadId = leadId,
            Fecha = fecha,
            DireccionIp = direccionIp,
            TextoConsentido = textoConsentido,
            VersionPolitica = versionPolitica,
            Metodo = metodo
        };
    }
}
