using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.UseCases.CreateLead;

public class CreateLeadHandler : ICommandHandler<CreateLeadCommand, CreateLeadResponse>
{
    private const int MaxSourceLength = 100;
    private readonly ILeadRepository _leadRepo;
    private readonly IPropertyPublicationRepository _publicationRepo;
    private readonly IConsentimientoRepository _consentimientoRepo;
    private readonly IPoliticaTratamientoRepository _politicaRepo;
    private readonly IEmailSender _emailSender;

    public CreateLeadHandler(
        ILeadRepository leadRepo,
        IPropertyPublicationRepository publicationRepo,
        IConsentimientoRepository consentimientoRepo,
        IPoliticaTratamientoRepository politicaRepo,
        IEmailSender emailSender)
    {
        _leadRepo = leadRepo;
        _publicationRepo = publicationRepo;
        _consentimientoRepo = consentimientoRepo;
        _politicaRepo = politicaRepo;
        _emailSender = emailSender;
    }

    public async Task<CreateLeadResponse> HandleAsync(CreateLeadCommand command, CancellationToken ct = default)
    {
        var email = Email.Create(command.Email);
        var telefono = command.Telefono is not null ? Telefono.Create(command.Telefono) : null;
        var ownerService = ParseOwnerService(command.OwnerService);
        var publicationId = ParsePublicationId(command.PublicationId);

        if (await _leadRepo.ExistsByEmailAsync(email))
            throw new InvalidOperationException($"Ya existe un lead con email {command.Email}");

        PropertyPublication? publication = null;
        var requiresPublicationReview = !string.IsNullOrWhiteSpace(command.PublicationId);

        if (publicationId.HasValue)
        {
            publication = await _publicationRepo.GetPublishedByIdAsync(publicationId.Value);
            requiresPublicationReview = publication is null;
        }

        var source = BuildSource(
            command.Fuente,
            command.PublicationId,
            requiresPublicationReview);
        var lead = Lead.Crear(command.Nombre, email, telefono, source,
            command.AutorizacionDatos, command.TipoOperacion, ownerService, publicationId);

        if (publication is not null)
            lead.AsignarAsesor(publication.AdvisorId);

        var saved = await _leadRepo.SaveAsync(lead);

        if (command.AutorizacionDatos)
        {
            var politica = await _politicaRepo.ObtenerActivaAsync();
            if (politica is not null)
            {
                var consentimiento = Consentimiento.Registrar(
                    saved.Id,
                    politica.TextoCompleto,
                    politica.Version,
                    "formulario_web");
                await _consentimientoRepo.AgregarAsync(consentimiento);
            }
        }

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
            return baseSource[..Math.Min(baseSource.Length, MaxSourceLength)];

        var reference = publicationReference?.Trim() ?? "missing";
        var marker = $"PublicationReview:{reference}";
        if (marker.Length >= MaxSourceLength)
            return marker[..MaxSourceLength];

        var availableBaseLength = MaxSourceLength - marker.Length - 1;
        var trimmedBase = baseSource[..Math.Min(baseSource.Length, availableBaseLength)];
        return $"{trimmedBase}|{marker}";
    }
}
