using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.RealizedDeals;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class RealizedDealUpdateEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task PutDeal_Anonymous_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var response = await client.PutAsJsonAsync(
            $"/api/realized-deals/{Guid.NewGuid()}",
            new UpdateRealizedDealRequest(100_000_000m, 3_000_000m, "COP", new DateOnly(2026, 10, 20)));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutDeal_OtherAdvisor_ReturnsNotFound()
    {
        var dealAdvisor = await SeedAdvisorAsync();
        var dealId = await SeedDealAsync(dealAdvisor.Id, DateTime.UtcNow.AddHours(-1));

        var (otherAdvisorClient, _) = factory.CreateAuthenticatedClient("Asesor");
        using (otherAdvisorClient)
        {
            var response = await otherAdvisorClient.PutAsJsonAsync(
                $"/api/realized-deals/{dealId}",
                new UpdateRealizedDealRequest(120_000_000m, 3_600_000m, "COP", new DateOnly(2026, 10, 21)));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task PutDeal_ResponsibleAdvisorWithinThreeDays_UpdatesSuccessfully()
    {
        var (advisorClient, advisorUser) = factory.CreateAuthenticatedClient("Asesor");
        var dealId = await SeedDealAsync(advisorUser.Id, DateTime.UtcNow.AddDays(-1));

        using (advisorClient)
        {
            var response = await advisorClient.PutAsJsonAsync(
                $"/api/realized-deals/{dealId}",
                new UpdateRealizedDealRequest(180_000_000m, 5_400_000m, "COP", new DateOnly(2026, 10, 22)));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<RealizedDealCreatedResponse>();
            Assert.NotNull(updated);
            Assert.Equal(180_000_000m, updated.GrossAmount);
            Assert.Equal(5_400_000m, updated.CommissionAmount);
            Assert.Equal(new DateOnly(2026, 10, 22), updated.ClosedOn);
        }
    }

    [Fact]
    public async Task PutDeal_WhenExpiredPastThreeDays_ReturnsConflict()
    {
        var dealAdvisor = await SeedAdvisorAsync();
        var dealId = await SeedDealAsync(dealAdvisor.Id, DateTime.UtcNow.AddDays(-4));

        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        using (adminClient)
        {
            var response = await adminClient.PutAsJsonAsync(
                $"/api/realized-deals/{dealId}",
                new UpdateRealizedDealRequest(200_000_000m, 6_000_000m, "COP", new DateOnly(2026, 10, 22)));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
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

    private async Task<Guid> SeedDealAsync(Guid advisorId, DateTime recordedAtUtc)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();

        var property = Inmueble.Crear(
            "Propiedad Test",
            "Descripcion",
            TipoInmueble.Casa,
            ModalidadInmueble.Venta,
            Direccion.Crear("Calle 10", "Medellin", "Antioquia", "Colombia"),
            Dinero.Crear(200_000_000m, "COP"),
            100, 3, 2, 1,
            TestWebApplicationFactory.PropietarioSeedId);
        db.Inmuebles.Add(property);

        var contact = Contact.Create("Cliente Test", Email.Create($"{Guid.NewGuid():N}@test.com"));
        db.Contacts.Add(contact);

        var deal = RealizedDeal.Create(
            property.Id,
            contact.Id,
            null,
            advisorId,
            RealizedDealOperation.Sale,
            100_000_000m,
            3_000_000m,
            "COP",
            DateOnly.FromDateTime(recordedAtUtc),
            advisorId,
            recordedAtUtc);

        db.RealizedDeals.Add(deal);
        await db.SaveChangesAsync();
        return deal.Id;
    }
}
