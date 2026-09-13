using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class VisitasEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private const string TestPassword = "Visitas123!";
    private readonly TestWebApplicationFactory _factory;

    public VisitasEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetVisits_AdminSeesAllWithExpectedFields()
    {
        var data = SeedVisits();
        using var client = await AuthenticatedClientAsync(data.Admin.Correo.Value);

        var response = await client.GetAsync("/api/visitas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        var visits = json.RootElement.EnumerateArray().ToList();
        Assert.Contains(visits, visit => visit.GetProperty("id").GetGuid() == data.FirstVisit.Id);
        var completed = Assert.Single(visits, visit => visit.GetProperty("id").GetGuid() == data.SecondVisit.Id);
        Assert.Equal("Realizada", completed.GetProperty("estado").GetString());
        Assert.Equal(
            ["asesorId", "estado", "fechaProgramada", "fechaRealizada", "feedback", "id", "inmuebleId", "leadId"],
            completed.EnumerateObject().Select(property => property.Name).OrderBy(name => name).ToArray());
    }

    [Fact]
    public async Task GetVisits_AdvisorSeesOnlyOwnVisits()
    {
        var data = SeedVisits();
        using var client = await AuthenticatedClientAsync(data.FirstAdvisor.Correo.Value);

        var response = await client.GetAsync("/api/visitas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var visits = json.RootElement.EnumerateArray().ToList();
        Assert.Contains(visits, visit => visit.GetProperty("id").GetGuid() == data.FirstVisit.Id);
        Assert.DoesNotContain(visits, visit => visit.GetProperty("id").GetGuid() == data.SecondVisit.Id);
        Assert.All(visits, visit => Assert.Equal(data.FirstAdvisor.Id, visit.GetProperty("asesorId").GetGuid()));
    }

    [Fact]
    public async Task GetVisits_WithoutAuthenticationReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/visitas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private (Asesor Admin, Asesor FirstAdvisor, Visita FirstVisit, Visita SecondVisit) SeedVisits()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var hash = BCrypt.Net.BCrypt.HashPassword(TestPassword);
        var admin = Asesor.Crear("Admin Visitas", Email.Create($"admin-visitas-{Guid.NewGuid():N}@test.com"), hash, "Administrador");
        var firstAdvisor = Asesor.Crear("Asesor Uno", Email.Create($"asesor-uno-{Guid.NewGuid():N}@test.com"), hash);
        var secondAdvisor = Asesor.Crear("Asesor Dos", Email.Create($"asesor-dos-{Guid.NewGuid():N}@test.com"), hash);
        var lead = Lead.Crear("Lead Visitas", Email.Create($"lead-visitas-{Guid.NewGuid():N}@test.com"), null, "Test", true);
        var firstVisit = Visita.Programar(lead.Id, Guid.NewGuid(), firstAdvisor.Id, DateTime.UtcNow.AddDays(1));
        var secondVisit = Visita.Reconstituir(
            Guid.NewGuid(), lead.Id, Guid.NewGuid(), secondAdvisor.Id,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, "Visita realizada", "Realizada");

        db.Asesores.AddRange(admin, firstAdvisor, secondAdvisor);
        db.Leads.Add(lead);
        db.Visitas.AddRange(firstVisit, secondVisit);
        db.SaveChanges();

        return (admin, firstAdvisor, firstVisit, secondVisit);
    }

    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { Correo = email, Password = TestPassword });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = json.RootElement.GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
