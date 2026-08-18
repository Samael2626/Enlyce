using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class Interaccion
{
    public Guid Id { get; private set; }
    public Guid LeadId { get; private set; }
    public Guid AsesorId { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string? Resumen { get; private set; }
    public DateTime Fecha { get; private set; }

    private Interaccion() { }

    public static Interaccion Registrar(Guid leadId, Guid asesorId, string tipo, string? resumen = null)
    {
        if (leadId == Guid.Empty) throw new DomainError("El lead_id es obligatorio.");
        if (asesorId == Guid.Empty) throw new DomainError("El asesor_id es obligatorio.");
        if (string.IsNullOrWhiteSpace(tipo)) throw new DomainError("El tipo es obligatorio.");

        return new Interaccion
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            AsesorId = asesorId,
            Tipo = tipo,
            Resumen = resumen,
            Fecha = DateTime.UtcNow
        };
    }

    public static Interaccion Reconstituir(
        Guid id, Guid leadId, Guid asesorId, string tipo, string? resumen, DateTime fecha)
    {
        return new Interaccion
        {
            Id = id,
            LeadId = leadId,
            AsesorId = asesorId,
            Tipo = tipo,
            Resumen = resumen,
            Fecha = fecha
        };
    }
}
