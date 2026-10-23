using System.Net;
using System.Net.Http.Json;
using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class MarketAnalysisEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private const string BaseUrl = "/api/analytics/market-analysis";

    [Fact]
    public async Task GetMarketAnalysis_Anonymous_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync(BaseUrl);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMarketAnalysis_AdvisorRole_ReturnsForbidden()
    {
        var (advisorClient, _) = factory.CreateAuthenticatedClient("Asesor");
        using (advisorClient)
        {
            var response = await advisorClient.GetAsync(BaseUrl);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task GetMarketAnalysis_InvalidParameters_ReturnsBadRequest()
    {
        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        using (adminClient)
        {
            var invalidOp = await adminClient.GetAsync($"{BaseUrl}?operation=Invalida");
            Assert.Equal(HttpStatusCode.BadRequest, invalidOp.StatusCode);

            var negativeArea = await adminClient.GetAsync($"{BaseUrl}?targetAreaM2=-10");
            Assert.Equal(HttpStatusCode.BadRequest, negativeArea.StatusCode);
        }
    }

    [Fact]
    public async Task GetMarketAnalysis_ValidQuery_ComputesMetricsAndValuation()
    {
        var uniqueZone = $"Zone_{Guid.NewGuid():N}";
        var advisor = await SeedAdvisorAsync();

        // Seed 2 active properties in uniqueZone
        await SeedPropertyAsync("Apto 1", uniqueZone, 100, 500_000_000m, EstadoInmueble.Disponible);
        await SeedPropertyAsync("Apto 2", uniqueZone, 120, 600_000_000m, EstadoInmueble.Disponible);

        // Seed 1 closed deal in uniqueZone
        var propertyForDeal = await SeedPropertyAsync("Apto Vendido", uniqueZone, 100, 500_000_000m, EstadoInmueble.Vendido);
        await SeedDealAsync(propertyForDeal.Id, advisor.Id, 480_000_000m, 14_400_000m);

        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        using (adminClient)
        {
            var url = $"{BaseUrl}?city=Medellin&neighborhood={uniqueZone}&targetAreaM2=90&operation=Venta";
            var response = await adminClient.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<MarketComparativeAnalysisResult>();

            Assert.NotNull(result);
            // Active Metrics: 2 properties (500M and 600M -> average 550M, price/m2: 5000000 and 5000000 -> 5000000)
            Assert.Equal(2, result.ActiveMetrics.SampleSize);
            Assert.Equal(550_000_000m, result.ActiveMetrics.AveragePrice);
            Assert.Equal(5_000_000m, result.ActiveMetrics.AveragePricePerM2);

            // Closed Metrics: 1 deal (480M for 100m2 -> 4.800.000/m2, discount vs 500M = -4%)
            Assert.Equal(1, result.ClosedMetrics.SampleSize);
            Assert.Equal(480_000_000m, result.ClosedMetrics.AverageGrossAmount);
            Assert.Equal(4_800_000m, result.ClosedMetrics.AveragePricePerM2);
            Assert.NotNull(result.ClosedMetrics.AverageDiscountPercentage);
            Assert.Equal(-4m, result.ClosedMetrics.AverageDiscountPercentage);

            // Valuation for 90m2
            Assert.NotNull(result.ValuationEstimate);
            Assert.Equal(90, result.ValuationEstimate.TargetAreaM2);
            Assert.True(result.ValuationEstimate.TargetEstimate > 0);
            Assert.True(result.ValuationEstimate.LowEstimate < result.ValuationEstimate.TargetEstimate);
            Assert.True(result.ValuationEstimate.HighEstimate > result.ValuationEstimate.TargetEstimate);

            // Comparables
            Assert.NotEmpty(result.ActiveComparables);
            Assert.NotEmpty(result.ClosedComparables);
        }
    }

    private async Task<Asesor> SeedAdvisorAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var advisor = Asesor.Crear("Advisor Test", Email.Create($"{Guid.NewGuid():N}@test.com"), "hash", "Asesor");
        db.Asesores.Add(advisor);
        await db.SaveChangesAsync();
        return advisor;
    }

    private async Task<Inmueble> SeedPropertyAsync(string name, string neighborhood, int areaM2, decimal price, EstadoInmueble estado)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();

        var property = Inmueble.Reconstituir(
            Guid.NewGuid(),
            name,
            "Descripcion analitica mercado",
            TipoInmueble.Apartamento,
            ModalidadInmueble.Venta,
            estado,
            Direccion.Crear("Calle 10", "Medellin", neighborhood, "Colombia", "Antioquia"),
            Dinero.Crear(price, "COP"),
            areaM2, 3, 2, 1, 0,
            TestWebApplicationFactory.PropietarioSeedId,
            DateTime.UtcNow.AddDays(-10),
            estado == EstadoInmueble.Vendido ? DateTime.UtcNow : null,
            true);

        db.Inmuebles.Add(property);
        await db.SaveChangesAsync();
        return property;
    }

    private async Task<Guid> SeedDealAsync(Guid propertyId, Guid advisorId, decimal grossAmount, decimal commission)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();

        var contact = Contact.Create("Cliente Comprador", Email.Create($"{Guid.NewGuid():N}@test.com"));
        db.Contacts.Add(contact);

        var deal = RealizedDeal.Create(
            propertyId,
            contact.Id,
            null,
            advisorId,
            RealizedDealOperation.Sale,
            grossAmount,
            commission,
            "COP",
            DateOnly.FromDateTime(DateTime.UtcNow),
            advisorId,
            DateTime.UtcNow);

        db.RealizedDeals.Add(deal);
        await db.SaveChangesAsync();
        return deal.Id;
    }
}
