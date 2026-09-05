using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Entities;
using NSubstitute;

namespace Enlyce.Application.Tests.PublicCatalog;

public sealed class GetPublicPropertyBySlugHandlerTests
{
    [Fact]
    public async Task HandleAsync_PublishedProperty_ReturnsContractResponse()
    {
        var repository = Substitute.For<IPublicPropertyReadRepository>();
        repository.GetBySlugAsync("apartamento-laureles", Arg.Any<CancellationToken>())
            .Returns(CreateDetail());
        var handler = new GetPublicPropertyBySlugHandler(repository);

        var result = await handler.HandleAsync(
            new GetPublicPropertyBySlugQuery("apartamento-laureles"));

        Assert.NotNull(result);
        Assert.Equal("Apartamento iluminado", result!.PublicTitle);
        Assert.Equal("Asesor L&C", result.Advisor.DisplayName);
        Assert.Null(result.AdministrationFee);
        Assert.Null(result.Features.Stratum);
        Assert.Empty(result.Features.Amenities);
    }

    [Fact]
    public async Task HandleAsync_MissingProperty_ReturnsNull()
    {
        var repository = Substitute.For<IPublicPropertyReadRepository>();
        repository.GetBySlugAsync("missing", Arg.Any<CancellationToken>())
            .Returns((PublicPropertyDetailData?)null);
        var handler = new GetPublicPropertyBySlugHandler(repository);

        var result = await handler.HandleAsync(new GetPublicPropertyBySlugQuery("missing"));

        Assert.Null(result);
    }

    private static PublicPropertyDetailData CreateDetail() => new(
        Guid.NewGuid(),
        "apartamento-laureles",
        "Apartamento iluminado",
        "Cerca de servicios.",
        TipoInmueble.Apartamento,
        ModalidadInmueble.Venta,
        620_000_000m,
        "COP",
        "Medellín",
        "Laureles",
        6.2443m,
        -75.5934m,
        92,
        3,
        2,
        1,
        [new PublicPhotoData("https://media.example.test/cover.webp", "Sala", 0, true)],
        Guid.NewGuid(),
        "Asesor L&C",
        DateTime.UtcNow);
}
