using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class Visita
{
    public Guid Id { get; private set; }
    public Guid LeadId { get; private set; }
    public Guid InmuebleId { get; private set; }
    public Guid AsesorId { get; private set; }
    public DateTime FechaProgramada { get; private set; }
    public DateTime? FechaRealizada { get; private set; }
    public string? Feedback { get; private set; }
    public string Estado { get; private set; } = "Programada";

    private Visita() { }

    public static Visita Programar(Guid leadId, Guid inmuebleId, Guid asesorId, DateTime fechaProgramada)
    {
        if (leadId == Guid.Empty) throw new DomainError("El lead_id es obligatorio.");
        if (inmuebleId == Guid.Empty) throw new DomainError("El inmueble_id es obligatorio.");
        if (asesorId == Guid.Empty) throw new DomainError("El asesor_id es obligatorio.");

        return new Visita
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            InmuebleId = inmuebleId,
            AsesorId = asesorId,
            FechaProgramada = fechaProgramada,
            Estado = "Programada"
        };
    }

    public static Visita Reconstituir(
        Guid id, Guid leadId, Guid inmuebleId, Guid asesorId,
        DateTime fechaProgramada, DateTime? fechaRealizada,
        string? feedback, string estado)
    {
        return new Visita
        {
            Id = id,
            LeadId = leadId,
            InmuebleId = inmuebleId,
            AsesorId = asesorId,
            FechaProgramada = fechaProgramada,
            FechaRealizada = fechaRealizada,
            Feedback = feedback,
            Estado = estado
        };
    }

    public void MarcarRealizada(string? feedback = null)
    {
        EnsureProgramada();
        if (feedback?.Trim().Length > 2_000)
            throw new DomainError("La retroalimentacion no puede superar 2000 caracteres.");
        FechaRealizada = DateTime.UtcNow;
        Feedback = string.IsNullOrWhiteSpace(feedback) ? null : feedback.Trim();
        Estado = "Realizada";
    }

    public void Cancelar()
    {
        EnsureProgramada();
        Estado = "Cancelada";
    }

    public void Reprogramar(DateTime nuevaFecha)
    {
        EnsureProgramada();
        if (nuevaFecha.Kind != DateTimeKind.Utc)
            throw new DomainError("La fecha de visita debe estar en UTC.");
        FechaProgramada = nuevaFecha;
    }

    private void EnsureProgramada()
    {
        if (Estado != "Programada")
            throw new DomainError("Solo se pueden modificar visitas programadas.");
    }
}
