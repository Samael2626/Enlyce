using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public class Ley1581EndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    private static readonly string AdminCorreo = "admin@test.com";
    private static readonly string AdminPassword = "Admin123!";

    public Ley1581EndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient().Client;

        SeedAdmin(factory);
    }

    private void SeedAdmin(TestWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();

        if (db.Asesores.Any(a => a.Correo.Value == AdminCorreo))
            return;

        var correo = Email.Create(AdminCorreo);
        var hash = BCrypt.Net.BCrypt.HashPassword(AdminPassword);
        var admin = Asesor.Crear("Admin Test", correo, hash, "Administrador");
        db.Asesores.Add(admin);
        db.SaveChanges();
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var loginRequest = new { Correo = AdminCorreo, Password = AdminPassword };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<Guid> CreateLeadAsync(string email = "lead@test.com", bool autorizacion = true)
    {
        var token = await LoginAsAdminAsync();
        SetAuthHeader(token);

        var request = new
        {
            Nombre = "Lead Test",
            Email = email,
            Telefono = "3101234567",
            Fuente = "Web",
            AutorizacionDatos = autorizacion
        };
        var response = await _client.PostAsJsonAsync("/api/leads", request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return Guid.Parse(doc.RootElement.GetProperty("id").GetString()!);
    }

    // --- Politica Tests ---

    [Fact]
    public async Task GetPoliticaActiva_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/politica/activa");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.Equal("1.0", doc.RootElement.GetProperty("version").GetString());
        Assert.True(doc.RootElement.TryGetProperty("textoCompleto", out _));
    }

    // --- Consulta Datos Tests ---

    [Fact]
    public async Task ConsultarDatosLead_WithConsent_ReturnsData()
    {
        var leadId = await CreateLeadAsync("consent@test.com");

        var response = await _client.GetAsync($"/api/datos-personales/{leadId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("autorizacionDatos").GetBoolean());
        Assert.True(doc.RootElement.TryGetProperty("consentimiento", out _));
    }

    [Fact]
    public async Task ConsultarDatosLead_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/datos-personales/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Supresion Tests ---

    [Fact]
    public async Task SuprimirDatosLead_Existing_ReturnsNoContent()
    {
        var leadId = await CreateLeadAsync("suprimir@test.com");

        var response = await _client.DeleteAsync($"/api/datos-personales/{leadId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Ley 1581: el lead sigue accesible para fines legales/historicos
        var consulta = await _client.GetAsync($"/api/datos-personales/{leadId}");
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
    }

    [Fact]
    public async Task SuprimirDatosLead_NonExisting_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/datos-personales/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Consentimiento Automatico ---

    [Fact]
    public async Task CreateLead_WithAutorizacion_CreatesConsentimiento()
    {
        var leadId = await CreateLeadAsync("autoconsent@test.com", autorizacion: true);

        var response = await _client.GetAsync($"/api/datos-personales/{leadId}");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("consentimiento", out var consent));
        Assert.Equal("1.0", consent.GetProperty("versionPolitica").GetString());
    }

    [Fact]
    public async Task CreateLead_WithoutAutorizacion_NoConsentimiento()
    {
        var token = await LoginAsAdminAsync();
        SetAuthHeader(token);

        var request = new
        {
            Nombre = "Sin Consent",
            Email = "sinconsent@test.com",
            AutorizacionDatos = false
        };

        // Lead.Crear requires autorizacionDatos = true (domain validation)
        var response = await _client.PostAsJsonAsync("/api/leads", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
