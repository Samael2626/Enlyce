using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using NSubstitute;

namespace Enlyce.Application.Tests.UseCases;

public sealed class CreateLeadHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly IPropertyPublicationRepository _publicationRepository =
        Substitute.For<IPropertyPublicationRepository>();
    private readonly IConsentimientoRepository _consentRepository =
        Substitute.For<IConsentimientoRepository>();
    private readonly IPoliticaTratamientoRepository _policyRepository =
        Substitute.For<IPoliticaTratamientoRepository>();
    private readonly IInteraccionRepository _interaccionRepository =
        Substitute.For<IInteraccionRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private Lead? _savedLead;
    private Consentimiento? _savedConsent;

    public CreateLeadHandlerTests()
    {
        _leadRepository.SaveAsync(Arg.Any<Lead>())
            .Returns(call => _savedLead = call.Arg<Lead>());
        _policyRepository.ObtenerActivaAsync()
            .Returns((PoliticaTratamiento?)null);
        _consentRepository.AgregarAsync(Arg.Do<Consentimiento>(item => _savedConsent = item));
    }

    [Fact]
    public async Task Handle_WithPublishedPublication_AssignsItsAdvisor()
    {
        var publication = CreatePublication(PublicationStatus.Published);
        _publicationRepository.GetPublishedByIdAsync(publication.Id)
            .Returns(publication);

        var result = await CreateHandler().HandleAsync(CreateCommand(publication.Id.ToString()));
        var saved = GetSavedLead();

        Assert.Equal(publication.Id, result.PublicationId);
        Assert.Equal(publication.Id, saved.PublicationId);
        Assert.Equal(publication.AdvisorId, saved.AsesorAsignadoId);
        Assert.Equal(EstadoLead.Contactado, saved.Estado);
    }

    [Fact]
    public async Task Handle_WithMissingPublication_CreatesUnassignedLeadForReview()
    {
        var publicationId = Guid.NewGuid();
        _publicationRepository.GetPublishedByIdAsync(publicationId)
            .Returns((PropertyPublication?)null);

        var result = await CreateHandler().HandleAsync(CreateCommand(publicationId.ToString()));
        var saved = GetSavedLead();

        Assert.Equal(publicationId, result.PublicationId);
        Assert.Equal(publicationId, saved.PublicationId);
        Assert.Null(saved.AsesorAsignadoId);
        Assert.Contains($"PublicationReview:{publicationId}", saved.Fuente);
    }

    [Fact]
    public async Task Handle_WithoutPublicationId_CreatesUnassignedLeadWithoutLookup()
    {
        var result = await CreateHandler().HandleAsync(CreateCommand(null));
        var saved = GetSavedLead();

        Assert.Null(result.PublicationId);
        Assert.Null(saved.PublicationId);
        Assert.Null(saved.AsesorAsignadoId);
        await _publicationRepository.DidNotReceiveWithAnyArgs()
            .GetPublishedByIdAsync(default);
    }

    [Fact]
    public async Task Handle_WithMalformedPublicationId_CreatesLeadForReview()
    {
        var result = await CreateHandler().HandleAsync(CreateCommand("not-a-guid"));
        var saved = GetSavedLead();

        Assert.Null(result.PublicationId);
        Assert.Null(saved.PublicationId);
        Assert.Null(saved.AsesorAsignadoId);
        Assert.Contains("PublicationReview:not-a-guid", saved.Fuente);
    }

    [Fact]
    public async Task Handle_RepeatedEmailWithoutPublication_AddsInteractionInsteadOfFailing()
    {
        var existing = CreateExistingLead(publicationId: null, advisorId: Guid.NewGuid());
        _leadRepository.GetByEmailAsync(Arg.Any<Domain.ValueObjects.Email>()).Returns(existing);

        var result = await CreateHandler().HandleAsync(CreateCommand(null, "repite@test.com"));

        Assert.True(result.EsContactoRepetido);
        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(1, existing.InteraccionesCount);
        await _interaccionRepository.Received(1).AgregarAsync(
            Arg.Is<Interaccion>(item => item.LeadId == existing.Id && item.Tipo == "ContactoWeb"));
    }

    [Fact]
    public async Task Handle_RepeatedEmailSamePublication_DoesNotDuplicateTheLead()
    {
        var publication = CreatePublication(PublicationStatus.Published);
        _publicationRepository.GetPublishedByIdAsync(publication.Id).Returns(publication);

        var existing = CreateExistingLead(publication.Id, publication.AdvisorId);
        _leadRepository.GetByEmailAsync(Arg.Any<Domain.ValueObjects.Email>()).Returns(existing);

        var result = await CreateHandler().HandleAsync(
            CreateCommand(publication.Id.ToString(), "repite@test.com"));

        Assert.True(result.EsContactoRepetido);
        Assert.Equal(existing.Id, result.Id);
        await _interaccionRepository.Received(1).AgregarAsync(Arg.Any<Interaccion>());
    }

    [Fact]
    public async Task Handle_RepeatedEmailOtherPublication_CreatesASecondLead()
    {
        var publication = CreatePublication(PublicationStatus.Published);
        _publicationRepository.GetPublishedByIdAsync(publication.Id).Returns(publication);

        // El lead previo miraba otro inmueble: es otra oportunidad comercial.
        var existing = CreateExistingLead(Guid.NewGuid(), Guid.NewGuid());
        _leadRepository.GetByEmailAsync(Arg.Any<Domain.ValueObjects.Email>()).Returns(existing);

        var result = await CreateHandler().HandleAsync(
            CreateCommand(publication.Id.ToString(), "repite@test.com"));

        var saved = GetSavedLead();
        Assert.False(result.EsContactoRepetido);
        Assert.NotEqual(existing.Id, saved.Id);
        Assert.Equal(publication.Id, saved.PublicationId);
        await _interaccionRepository.DidNotReceiveWithAnyArgs().AgregarAsync(default!);
    }

    [Fact]
    public async Task Handle_RepeatedEmailWithoutAdvisor_StillCountsTheContact()
    {
        var existing = CreateExistingLead(publicationId: null, advisorId: null);
        _leadRepository.GetByEmailAsync(Arg.Any<Domain.ValueObjects.Email>()).Returns(existing);

        var result = await CreateHandler().HandleAsync(CreateCommand(null, "repite@test.com"));

        Assert.True(result.EsContactoRepetido);
        Assert.Equal(1, existing.InteraccionesCount);
        // Sin asesor no hay a quien atribuir la interaccion.
        await _interaccionRepository.DidNotReceiveWithAnyArgs().AgregarAsync(default!);
    }

    [Fact]
    public async Task Handle_WithDeclaredChannelAndIp_AuditsBothInTheConsent()
    {
        GivenActivePolicy();

        await CreateHandler().HandleAsync(CreateCommand(null) with
        {
            Canal = "sitio_web",
            DireccionIp = "201.184.20.7",
        });

        var consent = Assert.IsType<Consentimiento>(_savedConsent);
        Assert.Equal("sitio_web", consent.Metodo);
        Assert.Equal("201.184.20.7", consent.DireccionIp);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithoutChannel_AuditsItAsUnknown(string? canal)
    {
        GivenActivePolicy();

        await CreateHandler().HandleAsync(CreateCommand(null) with { Canal = canal });

        var consent = Assert.IsType<Consentimiento>(_savedConsent);
        Assert.Equal("desconocido", consent.Metodo);
        Assert.Null(consent.DireccionIp);
    }

    [Fact]
    public async Task Handle_RepeatContact_AlsoAuditsChannelAndIp()
    {
        GivenActivePolicy();
        var existing = CreateExistingLead(publicationId: null, advisorId: Guid.NewGuid());
        _leadRepository.GetByEmailAsync(Arg.Any<Domain.ValueObjects.Email>()).Returns(existing);

        await CreateHandler().HandleAsync(CreateCommand(null, "repite@test.com") with
        {
            Canal = "funcional_legacy",
            DireccionIp = "190.7.1.9",
        });

        var consent = Assert.IsType<Consentimiento>(_savedConsent);
        Assert.Equal("funcional_legacy", consent.Metodo);
        Assert.Equal("190.7.1.9", consent.DireccionIp);
        Assert.Equal(existing.Id, consent.LeadId);
    }

    [Fact]
    public async Task Handle_SourceWithUtm_KeepsItWhenItFits()
    {
        var fuente = "Website:finca-cabuyal|utm=google/cpc/finca";

        await CreateHandler().HandleAsync(CreateCommand(null) with { Fuente = fuente });

        Assert.Equal(fuente, GetSavedLead().Fuente);
    }

    [Fact]
    public async Task Handle_SourceWithOversizedUtm_TrimsTheCampaignNotTheProperty()
    {
        var baseSource = "Website:apartamento-laureles-estadio";
        var fuente = $"{baseSource}|utm={new string('c', 200)}";

        await CreateHandler().HandleAsync(CreateCommand(null) with { Fuente = fuente });
        var saved = GetSavedLead().Fuente;

        Assert.Equal(100, saved.Length);
        // El inmueble sobrevive entero; lo que se recorta es la campana.
        Assert.StartsWith($"{baseSource}|utm=", saved);
    }

    [Fact]
    public async Task Handle_SourceWhereUtmMarkerDoesNotFit_DropsTheCampaignEntirely()
    {
        // La base ocupa casi todo el limite: no cabe ni "|utm=".
        var baseSource = new string('a', 97);
        var fuente = $"{baseSource}|utm=google/cpc/x";

        await CreateHandler().HandleAsync(CreateCommand(null) with { Fuente = fuente });

        Assert.Equal(baseSource, GetSavedLead().Fuente);
    }

    [Fact]
    public async Task Handle_SourceWithUtmAndPublicationReview_KeepsTheReviewMarker()
    {
        var publicationId = Guid.NewGuid();
        _publicationRepository.GetPublishedByIdAsync(publicationId)
            .Returns((PropertyPublication?)null);

        var fuente = $"Website:una-finca|utm={new string('c', 120)}";

        await CreateHandler().HandleAsync(
            CreateCommand(publicationId.ToString()) with { Fuente = fuente });
        var saved = GetSavedLead().Fuente;

        Assert.True(saved.Length <= 100);
        Assert.Contains($"PublicationReview:{publicationId}", saved);
    }

    private void GivenActivePolicy() =>
        _policyRepository.ObtenerActivaAsync().Returns(
            PoliticaTratamiento.Crear("v1", "Texto completo de la politica.", DateTime.UtcNow));

    private static Lead CreateExistingLead(Guid? publicationId, Guid? advisorId)
    {
        var lead = Lead.Crear(
            "Visitante recurrente",
            Domain.ValueObjects.Email.Create("repite@test.com"),
            null,
            "Website:anterior",
            true,
            "Venta",
            null,
            publicationId);

        if (advisorId.HasValue)
            lead.AsignarAsesor(advisorId.Value);

        return lead;
    }

    private CreateLeadHandler CreateHandler() => new(
        _leadRepository,
        _publicationRepository,
        _consentRepository,
        _policyRepository,
        _interaccionRepository,
        _emailSender);

    private static CreateLeadCommand CreateCommand(string? publicationId, string? email = null) => new(
        "Visitante",
        email ?? $"visit-{Guid.NewGuid():N}@test.com",
        "3101234567",
        "Website:apartamento-laureles",
        true,
        "Venta",
        null,
        publicationId);

    private Lead GetSavedLead()
    {
        _leadRepository.Received(1).SaveAsync(Arg.Any<Lead>());
        return Assert.IsType<Lead>(_savedLead);
    }

    private static PropertyPublication CreatePublication(PublicationStatus status)
    {
        var publication = PropertyPublication.Reconstitute(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "apartamento-laureles",
            "Apartamento Laureles",
            "Descripcion",
            status,
            500_000_000m,
            "COP",
            "Medellin",
            "Laureles",
            6.2m,
            -75.5m,
            false,
            DateTime.UtcNow,
            DateTime.UtcNow,
            []);
        return publication;
    }
}
