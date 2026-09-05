using System.Net;
using System.Text.Json;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class PublicCatalogEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly SemaphoreSlim SeedLock = new(1, 1);
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicCatalogEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        SeedCatalogAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetList_ReturnsPagedPublishedItemsAndCacheHeader()
    {
        var response = await _client.GetAsync("/api/public/inmuebles?page=1&pageSize=12");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TimeSpan.FromMinutes(5), response.Headers.CacheControl?.MaxAge);
        Assert.True(response.Headers.CacheControl?.Public);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(12, root.GetProperty("pageSize").GetInt32());
        Assert.Equal(2, root.GetProperty("total").GetInt32());
        Assert.Equal(2, root.GetProperty("items").GetArrayLength());

        AssertPropertyNames(root, "page", "pageSize", "total", "items");
        var item = root.GetProperty("items")[0];
        AssertPropertyNames(
            item,
            "id",
            "slug",
            "publicTitle",
            "propertyType",
            "operation",
            "price",
            "location",
            "areaSquareMeters",
            "bedrooms",
            "bathrooms",
            "parkingSpaces",
            "coverPhoto",
            "publishedAt");
        AssertPropertyNames(item.GetProperty("price"), "amount", "currency");
        AssertPropertyNames(
            item.GetProperty("location"),
            "municipality",
            "neighborhood",
            "approximateLatitude",
            "approximateLongitude");
        AssertPropertyNames(item.GetProperty("coverPhoto"), "url", "altText");
    }

    [Fact]
    public async Task GetList_WithFiltersAndSort_ReturnsExpectedItem()
    {
        var url = "/api/public/inmuebles?operation=Venta&propertyType=Apartamento" +
                  "&municipality=Medell%C3%ADn&neighborhood=Laureles" +
                  "&minPrice=600000000&maxPrice=700000000" +
                  "&minArea=80&maxArea=100&bedrooms=3&bathrooms=2" +
                  "&parkingSpaces=1&sort=priceAsc";

        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = Assert.Single(document.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(PublicCatalogTestData.PublishedApartmentSlug, item.GetProperty("slug").GetString());
    }

    [Fact]
    public async Task GetDetail_Published_ReturnsExactPublicShapeAndCacheHeader()
    {
        var response = await _client.GetAsync(
            $"/api/public/inmuebles/{PublicCatalogTestData.PublishedApartmentSlug}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TimeSpan.FromMinutes(5), response.Headers.CacheControl?.MaxAge);
        Assert.True(response.Headers.CacheControl?.Public);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal(PublicCatalogTestData.PublishedApartmentSlug, root.GetProperty("slug").GetString());
        Assert.Equal("Apartamento", root.GetProperty("propertyType").GetString());
        Assert.Equal("Venta", root.GetProperty("operation").GetString());
        Assert.Equal(3, root.GetProperty("photos").GetArrayLength());
        Assert.Equal("Asesor L&C", root.GetProperty("advisor").GetProperty("displayName").GetString());
        AssertPropertyNames(
            root,
            "id",
            "slug",
            "publicTitle",
            "publicDescription",
            "propertyType",
            "operation",
            "price",
            "administrationFee",
            "location",
            "features",
            "photos",
            "advisor",
            "publishedAt");
        AssertPropertyNames(
            root.GetProperty("features"),
            "areaSquareMeters",
            "bedrooms",
            "bathrooms",
            "parkingSpaces",
            "stratum",
            "amenities");
        AssertPropertyNames(
            root.GetProperty("photos")[0],
            "url",
            "altText",
            "order",
            "isCover");
        AssertPropertyNames(root.GetProperty("advisor"), "id", "displayName", "publicPhone");
    }

    [Fact]
    public async Task GetDetail_PausedOrMissing_ReturnsSameNotFoundResponse()
    {
        var paused = await _client.GetAsync(
            $"/api/public/inmuebles/{PublicCatalogTestData.PausedSlug}");
        var missing = await _client.GetAsync("/api/public/inmuebles/no-existe");

        Assert.Equal(HttpStatusCode.NotFound, paused.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(
            await missing.Content.ReadAsStringAsync(),
            await paused.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task PublicResponses_DoNotLeakInternalOrOwnerData()
    {
        var listJson = await _client.GetStringAsync("/api/public/inmuebles");
        var detailJson = await _client.GetStringAsync(
            $"/api/public/inmuebles/{PublicCatalogTestData.PublishedApartmentSlug}");

        foreach (var json in new[] { listJson, detailJson })
        {
            Assert.DoesNotContain(PublicCatalogTestData.PrivateStreet, json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(PublicCatalogTestData.OwnerName, json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("propietario", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("propertyId", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exactAddress", json, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=101")]
    [InlineData("?minPrice=900&maxPrice=100")]
    [InlineData("?minArea=200&maxArea=50")]
    [InlineData("?bedrooms=-1")]
    [InlineData("?bathrooms=-1")]
    [InlineData("?parkingSpaces=-1")]
    [InlineData("?operation=NoExiste")]
    [InlineData("?propertyType=NoExiste")]
    [InlineData("?sort=unknown")]
    public async Task GetList_InvalidQuery_ReturnsProblemDetails(string query)
    {
        var response = await _client.GetAsync($"/api/public/inmuebles{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, document.RootElement.GetProperty("status").GetInt32());
        Assert.True(document.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task GetList_MalformedNumericQuery_ReturnsProblemDetails()
    {
        var response = await _client.GetAsync("/api/public/inmuebles?page=no-es-numero");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, document.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Swagger_DescribesPublicEndpointsAndResponses()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");
        var listResponses = paths
            .GetProperty("/api/public/inmuebles")
            .GetProperty("get")
            .GetProperty("responses");
        var detailResponses = paths
            .GetProperty("/api/public/inmuebles/{slug}")
            .GetProperty("get")
            .GetProperty("responses");

        Assert.True(listResponses.TryGetProperty("200", out var listOk));
        Assert.True(listResponses.TryGetProperty("400", out _));
        Assert.Contains(
            "PublicPropertyPageResponse",
            listOk.GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema")
                .GetProperty("$ref")
                .GetString());
        Assert.True(detailResponses.TryGetProperty("200", out _));
        Assert.True(detailResponses.TryGetProperty("404", out _));
    }

    private async Task SeedCatalogAsync()
    {
        await SeedLock.WaitAsync();
        try
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            await PublicCatalogTestData.SeedAsync(db);
        }
        finally
        {
            SeedLock.Release();
        }
    }

    private static void AssertPropertyNames(JsonElement element, params string[] expected)
    {
        var actual = element.EnumerateObject()
            .Select(property => property.Name)
            .Order()
            .ToArray();

        Assert.Equal(expected.Order().ToArray(), actual);
    }
}
