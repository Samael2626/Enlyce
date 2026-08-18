using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Entities;

public enum EstadoLead
{
    Nuevo,
    Contactado,
    Interesado,
    VisitaAgendada,
    Negociacion,
    CerradoGanado,
    CerradoPerdido
}

public enum MotivoCierre
{
    Ninguno,
    NoContesto,
    NoInteres,
    CompraOtro,
    PrecioAlto,
    CerradoGanado
}

public sealed class Lead
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public Email Email { get; private set; }
    public Telefono? Telefono { get; private set; }
    public string Fuente { get; private set; }
    public EstadoLead Estado { get; private set; }
    public MotivoCierre MotivoCierre { get; private set; }
    public string? NotasCierre { get; private set; }
    public Guid? AsesorAsignadoId { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaUltimoContacto { get; private set; }
    public DateTime? FechaAsignacion { get; private set; }
    public bool AutorizacionDatos { get; private set; }
    public bool Activo { get; private set; }

    // EF Core constructor
    private Lead() { }

    private Lead(Guid id, string nombre, Email email, Telefono? telefono,
        string fuente, EstadoLead estado, MotivoCierre motivoCierre,
        string? notasCierre, Guid? asesorAsignadoId, DateTime fechaCreacion,
        DateTime? fechaUltimoContacto, DateTime? fechaAsignacion,
        bool autorizacionDatos, bool activo)
    {
        Id = id;
        Nombre = nombre;
        Email = email;
        Telefono = telefono;
        Fuente = fuente;
        Estado = estado;
        MotivoCierre = motivoCierre;
        NotasCierre = notasCierre;
        AsesorAsignadoId = asesorAsignadoId;
        FechaCreacion = fechaCreacion;
        FechaUltimoContacto = fechaUltimoContacto;
        FechaAsignacion = fechaAsignacion;
        AutorizacionDatos = autorizacionDatos;
        Activo = activo;
    }

    public static Lead Crear(string nombre, Email email, Telefono? telefono,
        string fuente, bool autorizacionDatos)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainError("El nombre del lead no puede ser vacio.");

        if (!autorizacionDatos)
            throw new DomainError("Se requiere autorizacion de tratamiento de datos (Ley 1581).");

        return new Lead(
            Guid.NewGuid(),
            nombre.Trim(),
            email,
            telefono,
            string.IsNullOrWhiteSpace(fuente) ? "Manual" : fuente.Trim(),
            EstadoLead.Nuevo,
            MotivoCierre.Ninguno,
            null,
            null,
            DateTime.UtcNow,
            null,
            null,
            autorizacionDatos,
            true);
    }

    public static Lead Reconstituir(Guid id, string nombre, Email email, Telefono? telefono,
        string fuente, EstadoLead estado, MotivoCierre motivoCierre,
        string? notasCierre, Guid? asesorAsignadoId, DateTime fechaCreacion,
        DateTime? fechaUltimoContacto, DateTime? fechaAsignacion,
        bool autorizacionDatos, bool activo)
    {
        return new Lead(id, nombre, email, telefono, fuente, estado, motivoCierre,
            notasCierre, asesorAsignadoId, fechaCreacion, fechaUltimoContacto,
            fechaAsignacion, autorizacionDatos, activo);
    }

    public void AsignarAsesor(Guid asesorId)
    {
        if (Estado != EstadoLead.Nuevo)
            throw new DomainError("Solo se puede asignar asesor a leads en estado Nuevo.");

        AsesorAsignadoId = asesorId;
        FechaAsignacion = DateTime.UtcNow;
        Estado = EstadoLead.Contactado;
    }

    public void RegistrarContacto()
    {
        FechaUltimoContacto = DateTime.UtcNow;

        if (Estado == EstadoLead.Contactado)
            Estado = EstadoLead.Interesado;
    }

    public void AgendarVisita()
    {
        if (Estado < EstadoLead.Interesado)
            throw new DomainError("Un lead necesita al menos una interaccion antes de agendar visita.");

        Estado = EstadoLead.VisitaAgendada;
    }

    public void MoverANegociacion()
    {
        if (Estado != EstadoLead.VisitaAgendada)
            throw new DomainError("Solo se puede mover a negociacion desde Visita Agendada.");

        Estado = EstadoLead.Negociacion;
    }

    public void Cerrar(MotivoCierre motivo, string? notas = null)
    {
        if (motivo == MotivoCierre.Ninguno)
            throw new DomainError("Debe especificar un motivo de cierre.");

        Estado = motivo == MotivoCierre.CerradoGanado
            ? EstadoLead.CerradoGanado
            : EstadoLead.CerradoPerdido;

        MotivoCierre = motivo;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}
