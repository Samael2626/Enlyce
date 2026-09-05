using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Entities;
using NSubstitute;

namespace Enlyce.Application.Tests.PublicCatalog;

public sealed class GetPublicPropertiesHandlerTests
{
    private readonly IPublicPropertyReadRepository _repository =
        Substitute.For<IPublicPropertyReadRepository>();

    [Fact]
    public async Task HandleAsync_ValidQuery_MapsCriteriaAndResponse()
    {
        _repository.SearchAsync(
                Arg.Any<PublicPropertySearchCriteria>(),
                Arg.Any<CancellationToken>())
            .Returns(new PublicPropertyPageData(
                1,
                [CreateListItem()]));
        var handler = new GetPublicPropertiesHandler(_repository);
        var query = new GetPublicPropertiesQuery(
            2,
            6,
            "Venta",
            "Apartamento",
            "Medellín",
            "Laureles",
            300_000_000m,
            800_000_000m,
            60,
            120,
            2,
            2,
            1,
            "priceAsc");

        var result = await handler.HandleAsync(query);

        Assert.Equal(2, result.Page);
        Assert.Equal(6, result.PageSize);
        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
        await _repository.Received(1).SearchAsync(
            Arg.Is<PublicPropertySearchCriteria>(criteria =>
                criteria.Page == 2 &&
                criteria.PageSize == 6 &&
                criteria.Operation == ModalidadInmueble.Venta &&
                criteria.PropertyType == TipoInmueble.Apartamento &&
                criteria.Municipality == "Medellín" &&
                criteria.Sort == PublicPropertySort.PriceAscending),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 12, null, null, null, null, "page")]
    [InlineData(1, 0, null, null, null, null, "pageSize")]
    [InlineData(1, 101, null, null, null, null, "pageSize")]
    [InlineData(1, 12, "NoExiste", null, null, null, "operation")]
    [InlineData(1, 12, null, "NoExiste", null, null, "propertyType")]
    [InlineData(1, 12, null, null, "unknown", null, "sort")]
    public async Task HandleAsync_InvalidSimpleParameter_ThrowsValidationException(
        int page,
        int pageSize,
        string? operation,
        string? propertyType,
        string? sort,
        decimal? minPrice,
        string expectedField)
    {
        var handler = new GetPublicPropertiesHandler(_repository);
        var query = new GetPublicPropertiesQuery(
            page,
            pageSize,
            operation,
            propertyType,
            null,
            null,
            minPrice,
            null,
            null,
            null,
            null,
            null,
            null,
            sort);

        var exception = await Assert.ThrowsAsync<PublicCatalogValidationException>(
            () => handler.HandleAsync(query));

        Assert.Contains(expectedField, exception.Errors.Keys);
        await _repository.DidNotReceive().SearchAsync(
            Arg.Any<PublicPropertySearchCriteria>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_InvertedRanges_ReportsBothErrors()
    {
        var handler = new GetPublicPropertiesHandler(_repository);
        var query = new GetPublicPropertiesQuery(
            1,
            12,
            null,
            null,
            null,
            null,
            900m,
            100m,
            200,
            50,
            null,
            null,
            null,
            null);

        var exception = await Assert.ThrowsAsync<PublicCatalogValidationException>(
            () => handler.HandleAsync(query));

        Assert.Contains("priceRange", exception.Errors.Keys);
        Assert.Contains("areaRange", exception.Errors.Keys);
    }

    private static PublicPropertyListData CreateListItem() => new(
        Guid.NewGuid(),
        "apartamento-laureles",
        "Apartamento iluminado",
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
        new PublicPhotoData("https://media.example.test/cover.webp", "Sala", 0, true),
        DateTime.UtcNow);
}
