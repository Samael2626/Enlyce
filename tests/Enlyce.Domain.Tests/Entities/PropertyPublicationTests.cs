using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public class PropertyPublicationTests
{
    [Fact]
    public void Create_WithValidData_CreatesDraftAndNormalizesSlug()
    {
        var publication = CreatePublication("  Apartamento en Medellín  ");

        Assert.Equal(PublicationStatus.Draft, publication.Status);
        Assert.Equal("apartamento-en-medellin", publication.Slug);
        Assert.False(publication.ExactAddressVisible);
        Assert.Null(publication.PublishedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Publish_WithFewerThanThreePhotos_ThrowsDomainError(int photoCount)
    {
        var publication = CreateReadyPublication(photoCount);

        Assert.Throws<DomainError>(() => publication.Publish());
    }

    [Theory]
    // Cuarto decimal 3: redondea hacia abajo.
    [InlineData(6.2443, -75.5934, 6.244, -75.593)]
    // Cuarto decimal 7 y 8: redondea hacia arriba en valor absoluto.
    [InlineData(6.2447, -75.5938, 6.245, -75.594)]
    // Seis decimales, la precision que admite la columna.
    [InlineData(6.244321, -75.593789, 6.244, -75.594)]
    public void SetPublicLocation_RoundsCoordinatesToThreeDecimals(
        double latitude, double longitude, double expectedLatitude, double expectedLongitude)
    {
        var publication = CreatePublication();

        publication.SetPublicLocation("Medellín", "Laureles", (decimal)latitude, (decimal)longitude);

        Assert.Equal((decimal)expectedLatitude, publication.ApproximateLatitude);
        Assert.Equal((decimal)expectedLongitude, publication.ApproximateLongitude);
    }

    [Fact]
    public void SetPublicLocation_RoundsHalfAwayFromZero()
    {
        var publication = CreatePublication();

        publication.SetPublicLocation("Medellín", "Laureles", 6.2445m, -75.5935m);

        Assert.Equal(6.245m, publication.ApproximateLatitude);
        Assert.Equal(-75.594m, publication.ApproximateLongitude);
    }

    [Fact]
    public void SetPublicLocation_KeepsCoordinatesThatAreAlreadyCoarse()
    {
        var publication = CreatePublication();

        publication.SetPublicLocation("Envigado", "Zúñiga", 6.178m, -75.584m);

        Assert.Equal(6.178m, publication.ApproximateLatitude);
        Assert.Equal(-75.584m, publication.ApproximateLongitude);
    }

    [Fact]
    public void Reconstitute_RoundsLegacyCoordinatesStoredBeforeTheRule()
    {
        var publication = PropertyPublication.Reconstitute(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "apartamento-laureles", "Apartamento", "Descripcion",
            PublicationStatus.Published, 500_000_000m, "COP",
            "Medellín", "Laureles",
            6.244321m, -75.593789m,
            false, DateTime.UtcNow, DateTime.UtcNow, []);

        Assert.Equal(6.244m, publication.ApproximateLatitude);
        Assert.Equal(-75.594m, publication.ApproximateLongitude);
    }

    [Fact]
    public void Publish_WithoutPositivePrice_ThrowsDomainError()
    {
        var publication = CreatePublication();
        publication.SetPublicLocation("Medellín", "Laureles", 6.2443m, -75.5934m);
        AddThreePhotos(publication);

        Assert.Throws<DomainError>(() => publication.Publish());
    }

    [Fact]
    public void Publish_WithoutPublicLocation_ThrowsDomainError()
    {
        var publication = CreatePublication();
        publication.SetPublicPrice(620_000_000m, "COP");
        AddThreePhotos(publication);

        Assert.Throws<DomainError>(() => publication.Publish());
    }

    [Fact]
    public void Publish_WithRequiredData_Publishes()
    {
        var publication = CreateReadyPublication(3);

        publication.Publish();

        Assert.Equal(PublicationStatus.Published, publication.Status);
        Assert.NotNull(publication.PublishedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("---")]
    [InlineData("  ")]
    public void Create_WithInvalidSlug_ThrowsDomainError(string slug)
    {
        Assert.Throws<DomainError>(() => CreatePublication(slug));
    }

    [Fact]
    public void Pause_FromPublished_PausesPublication()
    {
        var publication = CreatePublishedPublication();

        publication.Pause();

        Assert.Equal(PublicationStatus.Paused, publication.Status);
    }

    [Fact]
    public void Pause_FromDraft_ThrowsDomainError()
    {
        var publication = CreatePublication();

        Assert.Throws<DomainError>(() => publication.Pause());
    }

    [Fact]
    public void Publish_FromPaused_PublishesAgain()
    {
        var publication = CreatePublishedPublication();
        var firstPublishedAt = publication.PublishedAt;
        publication.Pause();

        publication.Publish();

        Assert.Equal(PublicationStatus.Published, publication.Status);
        Assert.Equal(firstPublishedAt, publication.PublishedAt);
    }

    [Fact]
    public void Withdraw_FromPublished_IsTerminal()
    {
        var publication = CreatePublishedPublication();

        publication.Withdraw();

        Assert.Equal(PublicationStatus.Withdrawn, publication.Status);
        Assert.Throws<DomainError>(() => publication.Publish());
        Assert.Throws<DomainError>(() => publication.Pause());
        Assert.Throws<DomainError>(() => publication.Withdraw());
    }

    [Fact]
    public void Withdraw_FromPaused_WithdrawsPublication()
    {
        var publication = CreatePublishedPublication();
        publication.Pause();

        publication.Withdraw();

        Assert.Equal(PublicationStatus.Withdrawn, publication.Status);
    }

    [Fact]
    public void AddPhoto_WithDuplicateOrder_ThrowsDomainError()
    {
        var publication = CreatePublication();
        publication.AddPhoto("https://media.example.test/one.webp", "Sala", 0, true);

        Assert.Throws<DomainError>(() =>
            publication.AddPhoto("https://media.example.test/two.webp", "Cocina", 0));
    }

    [Fact]
    public void AddPhoto_WithSecondCover_ThrowsDomainError()
    {
        var publication = CreatePublication();
        publication.AddPhoto("https://media.example.test/one.webp", "Sala", 0, true);

        Assert.Throws<DomainError>(() =>
            publication.AddPhoto("https://media.example.test/two.webp", "Cocina", 1, true));
    }

    [Fact]
    public void Reconstitute_DoesNotApplyPublishingInvariants()
    {
        var publication = PropertyPublication.Reconstitute(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "legacy-publication",
            "Publicación histórica",
            string.Empty,
            PublicationStatus.Published,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            DateTime.UtcNow,
            DateTime.UtcNow,
            []);

        Assert.Equal(PublicationStatus.Published, publication.Status);
        Assert.Empty(publication.Photos);
    }

    private static PropertyPublication CreatePublication(string slug = "apartamento-laureles") =>
        PropertyPublication.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            slug,
            "Apartamento iluminado",
            "Cerca de servicios y vías principales.");

    private static PropertyPublication CreateReadyPublication(int photoCount)
    {
        var publication = CreatePublication();
        publication.SetPublicPrice(620_000_000m, "cop");
        publication.SetPublicLocation("Medellín", "Laureles", 6.2443m, -75.5934m);

        for (var index = 0; index < photoCount; index++)
        {
            publication.AddPhoto(
                $"https://media.example.test/{index}.webp",
                $"Foto {index + 1}",
                index,
                index == 0);
        }

        return publication;
    }

    private static PropertyPublication CreatePublishedPublication()
    {
        var publication = CreateReadyPublication(3);
        publication.Publish();
        return publication;
    }

    private static void AddThreePhotos(PropertyPublication publication)
    {
        for (var index = 0; index < 3; index++)
        {
            publication.AddPhoto(
                $"https://media.example.test/{index}.webp",
                $"Foto {index + 1}",
                index,
                index == 0);
        }
    }
}
