using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.Alerts;
using Enlyce.Application.Abstractions;
using Enlyce.Application.Alerts;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class SearchAlertsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public SearchAlertsEndpointTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateAlert_PersistsLeadAndActiveDemandWithCriteria()
    {
        using var client = _factory.CreateClient();
        var email = $"alerta-{Guid.NewGuid():N}@test.com";

        var response = await client.PostAsJsonAsync("/api/public/alertas", Request(email));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateSearchAlertResponse>();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var demand = await db.CustomerDemands.AsNoTracking().SingleAsync(item => item.Id == body!.AlertId);
        Assert.True(demand.Active);
        Assert.Equal(ModalidadInmueble.Arriendo, demand.Operation);
        Assert.Equal(TipoInmueble.Apartamento, demand.PropertyType);
        Assert.Equal(string.Empty, demand.City);
        Assert.Equal(3_000_000m, demand.MaximumPrice);
        Assert.Equal(2, demand.Bedrooms);
        var lead = await db.Leads.AsNoTracking().SingleAsync(item => item.Id == demand.LeadId);
        Assert.Equal(CreateSearchAlertHandler.Source, lead.Fuente);
    }

    [Fact]
    public async Task CreateAlert_WithoutConsent_IsRejected()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/public/alertas",
            Request($"sin-{Guid.NewGuid():N}@test.com") with { AutorizacionDatos = false });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unsubscribe_RequiresSignedToken()
    {
        using var client = _factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/public/alertas",
            Request($"baja-{Guid.NewGuid():N}@test.com"))).Content.ReadFromJsonAsync<CreateSearchAlertResponse>();
        var id = created!.AlertId;

        var forged = await client.PostAsJsonAsync($"/api/public/alertas/{id}/baja", new UnsubscribeSearchAlertRequest("abc"));
        Assert.Equal(HttpStatusCode.NotFound, forged.StatusCode);

        var url = _factory.Services.GetRequiredService<IPublicLinks>().UnsubscribeAlertUrl(id);
        var token = url[(url.IndexOf("token=", StringComparison.Ordinal) + 6)..];
        var valid = await client.PostAsJsonAsync($"/api/public/alertas/{id}/baja", new UnsubscribeSearchAlertRequest(token));
        Assert.Equal(HttpStatusCode.NoContent, valid.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        Assert.False((await db.CustomerDemands.AsNoTracking().SingleAsync(item => item.Id == id)).Active);
    }

    private static CreateSearchAlertRequest Request(string email) => new(
        "Ana Alerta", email, null, true, "sitio_web", "Arriendo", "Apartamento",
        null, null, null, 3_000_000m, 2, null, null);
}
