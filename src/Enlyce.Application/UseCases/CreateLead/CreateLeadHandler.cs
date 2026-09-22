using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.UseCases.CreateLead;

// Criterio de correos repetidos (2026-09-21)
//
// Antes, un segundo lead con el mismo correo lanzaba InvalidOperationException
// y el visitante recibia un error generico. Un comprador que consulta dos
// inmuebles, o un propietario que vuelve meses despues, quedaban fuera.
//
// Ahora:
//   - Correo nuevo                          -> lead nuevo.
//   - Correo repetido y otra publicacion    -> lead nuevo. Es otra oportunidad
//     comercial y necesita su propia ficha en el pipeline.
//   - Correo repetido, misma publicacion o
//     consulta sin publicacion              -> interaccion sobre el lead que ya
//     existe, mas IncrementarInteracciones. No se duplica la ficha.
//
// La respuesta marca EsContactoRepetido para que la web pueda decirlo sin
// inventarse que creo algo nuevo.
public class CreateLeadHandler : ICommandHandler<CreateLeadCommand, CreateLeadResponse>
{
    private const int MaxSourceLength = 100;
    private const string UnknownChannel = "desconocido";
    private const string UtmMarker = "|utm=";
    private const int MaxChannelLength = 50;
    private readonly ILeadRepository _leadRepo;
    private readonly IPropertyPublicationRepository _publicationRepo;
    private readonly IConsentimientoRepository _consentimientoRepo;
    private readonly IPoliticaTratamientoRepository _politicaRepo;
    private readonly IInteraccionRepository _interaccionRepo;
    private readonly IEmailSender _emailSender;

    public CreateLeadHandler(
        ILeadRepository leadRepo,
        IPropertyPublicationRepository publicationRepo,
        IConsentimientoRepository consentimientoRepo,
        IPoliticaTratamientoRepository politicaRepo,
        IInteraccionRepository interaccionRepo,
        IEmailSender emailSender)
    {
        _leadRepo = leadRepo;
        _publicationRepo = publicationRepo;
        _consentimientoRepo = consentimientoRepo;
        _politicaRepo = politicaRepo;
        _interaccionRepo = interaccionRepo;
        _emailSender = emailSender;
    }

    public async Task<CreateLeadResponse> HandleAsync(CreateLeadCommand command, CancellationToken ct = default)
    {
        var email = Email.Create(command.Email);
        var telefono = command.Telefono is not null ? Telefono.Create(command.Telefono) : null;
        var ownerService = ParseOwnerService(command.OwnerService);
        var publicationId = ParsePublicationId(command.PublicationId);

        PropertyPublication? publication = null;
        var requiresPublicationReview = !string.IsNullOrWhiteSpace(command.PublicationId);

        if (publicationId.HasValue)
        {
            publication = await _publicationRepo.GetPublishedByIdAsync(publicationId.Value);
            requiresPublicationReview = publication is null;
        }

        // Criterio de duplicados por correo (ver comentario al final de la clase).
        // Un correo repetido no es un error: es una persona que vuelve.
        var existing = await _leadRepo.GetByEmailAsync(email);
        if (existing is not null && !IsDistinctOpportunity(existing, publicationId))
            return await RegisterRepeatContactAsync(existing, command, publication, ct);

        var source = BuildSource(
            command.Fuente,
            command.PublicationId,
            requiresPublicationReview);
        var lead = Lead.Crear(command.Nombre, email, telefono, source,
            command.AutorizacionDatos, command.TipoOperacion, ownerService, publicationId);

        if (publication is not null)
            lead.AsignarAsesor(publication.AdvisorId);

        var saved = await _leadRepo.SaveAsync(lead);

        await RegisterConsentAsync(saved.Id, command);

        await _emailSender.SendAsync(
            command.Email,
            "Bienvenido a Enlyce",
            $"Hola {command.Nombre}, tu lead fue registrado exitosamente.");

        return new CreateLeadResponse(
            saved.Id, saved.Nombre, saved.Email.Value,
            saved.Estado.ToString(), saved.FechaCreacion,
            saved.OwnerService?.ToString(),
            saved.PublicationId);
    }

    // Interes por una publicacion distinta a la que ya trae el lead: es otra
    // oportunidad comercial y merece su propia ficha en el pipeline. Sin
    // publicacion, o con la misma, se trata como recontacto.
    private static bool IsDistinctOpportunity(Lead existing, Guid? publicationId) =>
        publicationId.HasValue && existing.PublicationId != publicationId;

    private async Task<CreateLeadResponse> RegisterRepeatContactAsync(
        Lead existing,
        CreateLeadCommand command,
        PropertyPublication? publication,
        CancellationToken ct)
    {
        var advisorId = publication?.AdvisorId ?? existing.AsesorAsignadoId;

        if (advisorId.HasValue)
        {
            var resumen = string.IsNullOrWhiteSpace(command.Fuente)
                ? "Nuevo contacto desde la web."
                : $"Nuevo contacto desde la web. Origen: {command.Fuente.Trim()}";

            await _interaccionRepo.AgregarAsync(
                Interaccion.Registrar(existing.Id, advisorId.Value, "ContactoWeb", resumen));
        }

        // Sin asesor asignado no hay a quien atribuir la interaccion, pero el
        // contador si debe moverse: es la senal de que la persona insiste.
        existing.IncrementarInteracciones();
        var saved = await _leadRepo.SaveAsync(existing);

        await RegisterConsentAsync(saved.Id, command);

        return new CreateLeadResponse(
            saved.Id, saved.Nombre, saved.Email.Value,
            saved.Estado.ToString(), saved.FechaCreacion,
            saved.OwnerService?.ToString(),
            saved.PublicationId,
            EsContactoRepetido: true);
    }

    private async Task RegisterConsentAsync(Guid leadId, CreateLeadCommand command)
    {
        if (!command.AutorizacionDatos)
            return;

        // Cada autorizacion se audita, tambien en el recontacto: la Ley 1581
        // pide poder demostrar cuando y como se otorgo, no solo que existe.
        var politica = await _politicaRepo.ObtenerActivaAsync();
        if (politica is null)
            return;

        await _consentimientoRepo.AgregarAsync(Consentimiento.Registrar(
            leadId,
            politica.TextoCompleto,
            politica.Version,
            NormalizeChannel(command.Canal),
            command.DireccionIp));
    }

    // El canal lo declara cada origen. No hay lista cerrada todavia: forzarla
    // ahora romperia integraciones futuras sin aportar nada a la auditoria, que
    // lo que necesita es saber que dijo el origen, no validarlo.
    private static string NormalizeChannel(string? canal)
    {
        if (string.IsNullOrWhiteSpace(canal))
            return UnknownChannel;

        var trimmed = canal.Trim();
        return trimmed[..Math.Min(trimmed.Length, MaxChannelLength)];
    }

    private static OwnerInquiryService? ParseOwnerService(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!Enum.TryParse<OwnerInquiryService>(value, false, out var ownerService) ||
            !Enum.IsDefined(ownerService))
            throw new DomainError($"Servicio de propietario no valido: {value}");

        return ownerService;
    }

    private static Guid? ParsePublicationId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Guid.TryParse(value, out var publicationId) && publicationId != Guid.Empty
            ? publicationId
            : null;
    }

    private static string BuildSource(
        string? source,
        string? publicationReference,
        bool requiresReview)
    {
        var baseSource = string.IsNullOrWhiteSpace(source) ? "Manual" : source.Trim();

        if (!requiresReview)
            return FitWithinLimit(baseSource, MaxSourceLength);

        var reference = publicationReference?.Trim() ?? "missing";
        var marker = $"PublicationReview:{reference}";
        if (marker.Length >= MaxSourceLength)
            return marker[..MaxSourceLength];

        var availableBaseLength = MaxSourceLength - marker.Length - 1;
        return $"{FitWithinLimit(baseSource, availableBaseLength)}|{marker}";
    }

    // La campana es el dato mas prescindible de la fuente: identificar el
    // inmueble importa mas que saber de que anuncio vino. Por eso el segmento
    // |utm= se recorta primero y se descarta entero antes de tocar el resto.
    private static string FitWithinLimit(string source, int limit)
    {
        if (limit <= 0)
            return string.Empty;

        if (source.Length <= limit)
            return source;

        var utmIndex = source.IndexOf(UtmMarker, StringComparison.Ordinal);
        if (utmIndex < 0)
            return source[..limit];

        var withoutUtm = source[..utmIndex];
        if (withoutUtm.Length >= limit)
            return withoutUtm[..limit];

        // Si ni siquiera cabe el marcador completo, la campana se descarta
        // entera: mejor sin dato que con un "|ut" que nadie sabe leer.
        if (limit - withoutUtm.Length <= UtmMarker.Length)
            return withoutUtm;

        return source[..limit];
    }
}
