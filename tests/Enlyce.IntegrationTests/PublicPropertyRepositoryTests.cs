using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Enlyce.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Enlyce.IntegrationTests;

public sealed class PublicPropertyRepositoryTests
{
    [Fact]
    public async Task SearchAsync_DefaultQuery_ExcludesPausedAndUnpublishedProperties()
    {
        await using var harness = await CreateHarnessAsync();

        var result = await harness.Repository.SearchAsync(new PublicPropertySearchCriteria());

        Assert.Equal(2, result.Total);
        Assert.DoesNotContain(result.Items, item => item.Slug == PublicCatalogTestData.PausedSlug);
        Assert.DoesNotContain(result.Items, item => item.Slug.StartsWith("crm-"));
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
    public async Task GetBySlugAsync_PausedAndCrmFallbackReturnNull()
    {
        await using var harness = await CreateHarnessAsync();

        var published = await harness.Repository.GetBySlugAsync(
            PublicCatalogTestData.PublishedApartmentSlug);
        var paused = await harness.Repository.GetBySlugAsync(PublicCatalogTestData.PausedSlug);
        var pausedPropertyId = await harness.Db.PropertyPublications
            .Where(item => item.Slug == PublicCatalogTestData.PausedSlug)
            .Select(item => item.PropertyId).SingleAsync();
        var crmDetail = await harness.Repository.GetBySlugAsync(
            $"crm-{pausedPropertyId:N}");

        Assert.NotNull(published);
        Assert.Equal(3, published!.Photos.Count);
        Assert.Equal([0, 1, 2], published.Photos.Select(photo => photo.Order));
        Assert.Null(paused);
        Assert.Null(crmDetail);
        Assert.Null(await harness.Repository.GetVisibleSlugByPropertyIdAsync(pausedPropertyId));
    }

    [Fact]
    public async Task PublishingDisabledByDefault_BlocksListDetailAndLinks()
    {
        await using var harness = await CreateHarnessAsync();
        var repository = new PublicPropertyRepository(harness.Db);
        Assert.Empty((await repository.SearchAsync(new PublicPropertySearchCriteria())).Items);
        Assert.Null(await repository.GetBySlugAsync(PublicCatalogTestData.PublishedApartmentSlug));
        var propertyId = await harness.Db.PropertyPublications
            .Where(item => item.Slug == PublicCatalogTestData.PublishedApartmentSlug)
            .Select(item => item.PropertyId).SingleAsync();
        Assert.Null(await repository.GetVisibleSlugByPropertyIdAsync(propertyId));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DemoAccounts_AreExcludedEvenWithPublishingEnabled(bool demoAdvisor)
    {
        await using var harness = await CreateHarnessAsync();
        if (demoAdvisor)
        {
            var email = "asesor.demo@enlyce.test";
            await harness.Db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"Asesores\" SET \"Correo\" = {email}");
        }
        else
        {
            var email = "catalogo.demo@enlyce.test";
            await harness.Db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"Propietarios\" SET \"Email\" = {email}");
        }
        Assert.Empty((await harness.Repository.SearchAsync(new PublicPropertySearchCriteria())).Items);
        Assert.Null(await harness.Repository.GetBySlugAsync(PublicCatalogTestData.PublishedApartmentSlug));
        var propertyId = await harness.Db.PropertyPublications
            .Where(item => item.Slug == PublicCatalogTestData.PublishedApartmentSlug)
            .Select(item => item.PropertyId).SingleAsync();
        Assert.Null(await harness.Repository.GetVisibleSlugByPropertyIdAsync(propertyId));
    }

    [Fact]
    public async Task SoldProperty_IsExcludedFromListDetailAndLinks()
    {
        await using var harness = await CreateHarnessAsync();
        var publication = await harness.Db.PropertyPublications
            .SingleAsync(item => item.Slug == PublicCatalogTestData.PublishedApartmentSlug);
        var property = await harness.Db.Inmuebles.SingleAsync(item => item.Id == publication.PropertyId);
        property.MarcarVendido();
        await harness.Db.SaveChangesAsync();

        Assert.Equal(1, (await harness.Repository.SearchAsync(new PublicPropertySearchCriteria())).Total);
        Assert.Null(await harness.Repository.GetBySlugAsync(publication.Slug));
        Assert.Null(await harness.Repository.GetVisibleSlugByPropertyIdAsync(property.Id));
    }

    [Fact]
    public async Task FiltersAndSorting_UsePublicPriceAndLocation()
    {
        await using var harness = await CreateHarnessAsync();
        var publication = await harness.Db.PropertyPublications
            .SingleAsync(item => item.Slug == PublicCatalogTestData.PublishedApartmentSlug);
        publication.SetPublicPrice(4_000_000m);
        publication.SetPublicLocation("Sabaneta", "Centro", 6.15m, -75.61m);
        await harness.Db.SaveChangesAsync();
        var result = await harness.Repository.SearchAsync(new PublicPropertySearchCriteria
        {
            Municipality = "sabaneta", Neighborhood = "centro", MaxPrice = 4_100_000m
        });
        Assert.Equal(publication.Slug, Assert.Single(result.Items).Slug);
        var ordered = await harness.Repository.SearchAsync(new PublicPropertySearchCriteria
        {
            Sort = PublicPropertySort.PriceAscending, PageSize = 1
        });
        Assert.Equal(publication.Slug, Assert.Single(ordered.Items).Slug);
    }

    private static async Task<RepositoryHarness> CreateHarnessAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        connection.CreateFunction<string?, int>("char_length", value => value?.Length ?? 0);
        var options = new DbContextOptionsBuilder<EnlyceDbContext>()
            .UseSqlite(connection)
            .ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>()
            .Options;
        var db = new EnlyceDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await PublicCatalogTestData.SeedAsync(db);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["PublicCatalog:Enabled"] = "true" }).Build();
        return new RepositoryHarness(connection, db, new PublicPropertyRepository(db, configuration));
    }

    private sealed class RepositoryHarness(
        SqliteConnection connection,
        EnlyceDbContext db,
        PublicPropertyRepository repository) : IAsyncDisposable
    {
        public PublicPropertyRepository Repository { get; } = repository;
        public EnlyceDbContext Db { get; } = db;

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
