using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Enlyce.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.IntegrationTests;

public sealed class PublicPropertyRepositoryTests
{
    [Fact]
    public async Task SearchAsync_DefaultQuery_ReturnsOnlyPublishedProperties()
    {
        await using var harness = await CreateHarnessAsync();

        var result = await harness.Repository.SearchAsync(new PublicPropertySearchCriteria());

        Assert.Equal(2, result.Total);
        Assert.DoesNotContain(result.Items, item => item.Slug == PublicCatalogTestData.PausedSlug);
        Assert.All(result.Items, item => Assert.NotNull(item.CoverPhoto));
    }

    [Fact]
    public async Task SearchAsync_AllFilters_ReturnsMatchingProperty()
    {
        await using var harness = await CreateHarnessAsync();
        var criteria = new PublicPropertySearchCriteria
        {
            Operation = ModalidadInmueble.Venta,
            PropertyType = TipoInmueble.Apartamento,
            Municipality = "medellín",
            Neighborhood = "laureles",
            MinPrice = 600_000_000m,
            MaxPrice = 700_000_000m,
            MinArea = 80,
            MaxArea = 100,
            MinBedrooms = 3,
            MinBathrooms = 2,
            MinParkingSpaces = 1
        };

        var result = await harness.Repository.SearchAsync(criteria);

        var item = Assert.Single(result.Items);
        Assert.Equal(PublicCatalogTestData.PublishedApartmentSlug, item.Slug);
    }

    [Fact]
    public async Task SearchAsync_PriceDescendingAndPagination_AreStable()
    {
        await using var harness = await CreateHarnessAsync();
        var criteria = new PublicPropertySearchCriteria
        {
            Page = 1,
            PageSize = 1,
            Sort = PublicPropertySort.PriceDescending
        };

        var result = await harness.Repository.SearchAsync(criteria);

        var item = Assert.Single(result.Items);
        Assert.Equal(2, result.Total);
        Assert.Equal(PublicCatalogTestData.PublishedApartmentSlug, item.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_PublishedReturnsDetail_PausedReturnsNull()
    {
        await using var harness = await CreateHarnessAsync();

        var published = await harness.Repository.GetBySlugAsync(
            PublicCatalogTestData.PublishedApartmentSlug);
        var paused = await harness.Repository.GetBySlugAsync(PublicCatalogTestData.PausedSlug);

        Assert.NotNull(published);
        Assert.Equal(3, published!.Photos.Count);
        Assert.Equal([0, 1, 2], published.Photos.Select(photo => photo.Order));
        Assert.Null(paused);
    }

    private static async Task<RepositoryHarness> CreateHarnessAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EnlyceDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new EnlyceDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await PublicCatalogTestData.SeedAsync(db);
        return new RepositoryHarness(connection, db, new PublicPropertyRepository(db));
    }

    private sealed class RepositoryHarness(
        SqliteConnection connection,
        EnlyceDbContext db,
        PublicPropertyRepository repository) : IAsyncDisposable
    {
        public PublicPropertyRepository Repository { get; } = repository;

        public async ValueTask DisposeAsync()
        {
            await db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
