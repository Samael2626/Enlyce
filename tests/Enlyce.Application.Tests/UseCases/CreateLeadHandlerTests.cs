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
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private Lead? _savedLead;

    public CreateLeadHandlerTests()
    {
        _leadRepository.SaveAsync(Arg.Any<Lead>())
            .Returns(call => _savedLead = call.Arg<Lead>());
        _policyRepository.ObtenerActivaAsync()
            .Returns((PoliticaTratamiento?)null);
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

    private CreateLeadHandler CreateHandler() => new(
        _leadRepository,
        _publicationRepository,
        _consentRepository,
        _policyRepository,
        _emailSender);

    private static CreateLeadCommand CreateCommand(string? publicationId) => new(
        "Visitante",
        $"visit-{Guid.NewGuid():N}@test.com",
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
