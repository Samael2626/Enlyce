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

public enum OwnerInquiryService
{
    Sell,
    Rent,
    Manage
}

public sealed class Lead
{
    public Guid Id { get; internal set; }
    public string Nombre { get; internal set; }
    public Email Email { get; internal set; }
    public Telefono? Telefono { get; internal set; }
    public string Fuente { get; internal set; }
    public EstadoLead Estado { get; internal set; }
    public MotivoCierre MotivoCierre { get; internal set; }
    public string? NotasCierre { get; internal set; }
    public Guid? AsesorAsignadoId { get; internal set; }
    public DateTime FechaCreacion { get; internal set; }
    public DateTime? FechaUltimoContacto { get; internal set; }
    public DateTime? FechaAsignacion { get; internal set; }
    public bool AutorizacionDatos { get; internal set; }
    public bool Activo { get; internal set; }

    public string TipoOperacion { get; internal set; } = "Venta";
    public OwnerInquiryService? OwnerService { get; internal set; }
    public string EtapaPipeline { get; internal set; } = EtapasPipeline.LeadNuevo;
    public int InteraccionesCount { get; internal set; }
    public DateTime? FechaUltimaInteraccion { get; internal set; }
    public DateTime FechaActualizacion { get; internal set; }

    private Lead() { }

    private Lead(Guid id, string nombre, Email email, Telefono? telefono,
        string fuente, EstadoLead estado, MotivoCierre motivoCierre,
        string? notasCierre, Guid? asesorAsignadoId, DateTime fechaCreacion,
        DateTime? fechaUltimoContacto, DateTime? fechaAsignacion,
        bool autorizacionDatos, bool activo,
        string tipoOperacion, string etapaPipeline,
        int interaccionesCount, DateTime? fechaUltimaInteraccion,
        DateTime fechaActualizacion, OwnerInquiryService? ownerService)
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
        TipoOperacion = tipoOperacion;
        OwnerService = ownerService;
        EtapaPipeline = etapaPipeline;
        InteraccionesCount = interaccionesCount;
        FechaUltimaInteraccion = fechaUltimaInteraccion;
        FechaActualizacion = fechaActualizacion;
    }

    public static Lead Crear(string nombre, Email email, Telefono? telefono,
        string fuente, bool autorizacionDatos, string tipoOperacion = "Venta",
        OwnerInquiryService? ownerService = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainError("El nombre del lead no puede ser vacio.");

        if (!autorizacionDatos)
            throw new DomainError("Se requiere autorizacion de tratamiento de datos (Ley 1581).");

        if (!EtapasPipeline.EsTipoOperacionValida(tipoOperacion))
            throw new DomainError($"Tipo de operacion no valido: {tipoOperacion}");

        ValidateOwnerService(ownerService, tipoOperacion);

        var now = DateTime.UtcNow;

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
            now,
            null,
            null,
            autorizacionDatos,
            true,
            tipoOperacion,
            EtapasPipeline.LeadNuevo,
            0,
            null,
            now,
            ownerService);
    }

    public static Lead Reconstituir(Guid id, string nombre, Email email, Telefono? telefono,
        string fuente, EstadoLead estado, MotivoCierre motivoCierre,
        string? notasCierre, Guid? asesorAsignadoId, DateTime fechaCreacion,
        DateTime? fechaUltimoContacto, DateTime? fechaAsignacion,
        bool autorizacionDatos, bool activo,
        string? tipoOperacion = null, string? etapaPipeline = null,
        int interaccionesCount = 0, DateTime? fechaUltimaInteraccion = null,
        DateTime? fechaActualizacion = null,
        OwnerInquiryService? ownerService = null)
    {
        return new Lead(id, nombre, email, telefono, fuente, estado, motivoCierre,
            notasCierre, asesorAsignadoId, fechaCreacion, fechaUltimoContacto,
            fechaAsignacion, autorizacionDatos, activo,
            tipoOperacion ?? "Venta",
            etapaPipeline ?? EtapasPipeline.LeadNuevo,
            interaccionesCount,
            fechaUltimaInteraccion,
            fechaActualizacion ?? fechaCreacion,
            ownerService);
    }

    private static void ValidateOwnerService(
        OwnerInquiryService? ownerService,
        string operationType)
    {
        if (ownerService is null)
            return;

        if (!Enum.IsDefined(ownerService.Value))
            throw new DomainError($"Servicio de propietario no valido: {ownerService}");

        var expectedOperation = ownerService == OwnerInquiryService.Sell
            ? "Venta"
            : "Arriendo";

        if (operationType != expectedOperation)
            throw new DomainError(
                $"El servicio {ownerService} requiere TipoOperacion {expectedOperation}.");
    }

    public void AsignarAsesor(Guid asesorId)
    {
        AsesorAsignadoId = asesorId;
        FechaAsignacion = DateTime.UtcNow;
        Estado = EstadoLead.Contactado;
        FechaActualizacion = DateTime.UtcNow;
    }

    public void RegistrarContacto()
    {
        FechaUltimoContacto = DateTime.UtcNow;
        FechaActualizacion = DateTime.UtcNow;

        if (Estado == EstadoLead.Contactado)
            Estado = EstadoLead.Interesado;
    }

    public void MoverEtapa(string nuevaEtapa)
    {
        if (!TransicionesPipeline.EsTransicionValida(EtapaPipeline, nuevaEtapa, TipoOperacion))
            throw new DomainError($"Transicion no valida: {EtapaPipeline} -> {nuevaEtapa}");

        if (nuevaEtapa == EtapasPipeline.VisitaAgendada && InteraccionesCount == 0)
            throw new DomainError("Se requiere al menos una interaccion antes de agendar visita.");

        EtapaPipeline = nuevaEtapa;
        FechaActualizacion = DateTime.UtcNow;

        if (nuevaEtapa == EtapasPipeline.CerradoGanado)
            Estado = EstadoLead.CerradoGanado;
        else if (nuevaEtapa == EtapasPipeline.CerradoPerdido)
            Estado = EstadoLead.CerradoPerdido;
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

    public void IncrementarInteracciones()
    {
        InteraccionesCount++;
        FechaUltimaInteraccion = DateTime.UtcNow;
        FechaActualizacion = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}
