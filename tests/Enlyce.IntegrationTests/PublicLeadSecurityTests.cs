using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class PublicLeadSecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public PublicLeadSecurityTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RepeatContact_DoesNotRevealExistingOpportunity()
    {
        var email = $"private-{Guid.NewGuid():N}@test.com";
        var first = await CreateOwnerOpportunityAsync(email);
        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);

        var repeated = await CreateOwnerOpportunityAsync(email);
        var repeatedBody = await repeated.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Accepted, repeated.StatusCode);
        Assert.DoesNotContain(email, repeatedBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("esContactoRepetido", repeatedBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("continuationToken", repeatedBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LegacyOwnerDetailsRoute_CannotModifyOpportunity()
    {
        var email = $"protected-{Guid.NewGuid():N}@test.com";
        var created = await CreateOwnerOpportunityAsync(email);
        Assert.Equal(HttpStatusCode.Accepted, created.StatusCode);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var leadId = await db.Leads
            .Where(item => item.Email.Value == email)
            .Select(item => item.Id)
            .SingleAsync();

        var attack = await _client.PutAsJsonAsync(
            $"/api/leads/{leadId}/owner-details",
            new
            {
                Email = email,
                PropertyType = "House",
                City = "Bogota",
                Neighborhood = "Dato alterado",
                ExpectedPrice = 1m,
                Message = "Cambio no autorizado",
                PreferredContactChannel = "Email"
            });

        Assert.Equal(HttpStatusCode.NotFound, attack.StatusCode);
    }

    [Fact]
    public async Task ContinuationToken_CanBeUsedOnlyOnce()
    {
        var email = $"one-use-{Guid.NewGuid():N}@test.com";
        var created = await CreateOwnerOpportunityAsync(email);
        var submission = await created.Content.ReadFromJsonAsync<PublicLeadSubmissionResponse>();

        var first = await CompleteOwnerDetailsAsync(submission!.ContinuationToken, "Dato original");
        var replay = await CompleteOwnerDetailsAsync(submission.ContinuationToken, "Dato del atacante");

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, replay.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var neighborhood = await db.Leads
            .Where(item => item.Email.Value == email)
            .Select(item => item.OwnerPropertyNeighborhood)
            .SingleAsync();
        Assert.Equal("Dato original", neighborhood);
    }

    private Task<HttpResponseMessage> CreateOwnerOpportunityAsync(string email) =>
        _client.PostAsJsonAsync("/api/leads", new
        {
            Nombre = "Propietario Protegido",
            Email = email,
            Telefono = "3105551111",
            Fuente = "PruebaSeguridad",
            AutorizacionDatos = true,
            TipoOperacion = "Venta",
            OwnerService = "Sell"
        });

    private Task<HttpResponseMessage> CompleteOwnerDetailsAsync(string token, string neighborhood) =>
        _client.PutAsJsonAsync("/api/leads/owner-details", new
        {
            ContinuationToken = token,
            PropertyType = "House",
            City = "Medellin",
            Neighborhood = neighborhood,
            ExpectedPrice = 500_000_000m,
            Message = "Prueba de continuacion",
            PreferredContactChannel = "Email"
        });
}
