using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public class AuthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    private static readonly string AdminCorreo = "admin@test.com";
    private static readonly string AdminPassword = "Admin123!";

    public AuthEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

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

    [Fact]
    public async Task Register_AsAdministrador_ReturnsCreated()
    {
        var token = await LoginAsAdminAsync();
        SetAuthHeader(token);

        var request = new { Nombre = "Nuevo Asesor", Correo = "nuevo@test.com", Password = "Test123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var request = new { Correo = AdminCorreo, Password = AdminPassword };
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("token", out _));
        Assert.True(doc.RootElement.TryGetProperty("rol", out _));
        Assert.True(doc.RootElement.TryGetProperty("nombre", out _));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var request = new { Correo = AdminCorreo, Password = "WrongPassword" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistingUser_ReturnsUnauthorized()
    {
        var request = new { Correo = "noexiste@test.com", Password = "Any123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_Authenticated_ReturnsOk()
    {
        var token = await LoginAsAdminAsync();
        SetAuthHeader(token);

        var response = await _client.PostAsJsonAsync("/api/auth/logout", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Me_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_Authenticated_ReturnsOk()
    {
        var token = await LoginAsAdminAsync();
        SetAuthHeader(token);

        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.Equal(AdminCorreo, doc.RootElement.GetProperty("email").GetString());
        Assert.Equal("Administrador", doc.RootElement.GetProperty("rol").GetString());
    }

    [Fact]
    public async Task Register_WithoutAuth_ReturnsUnauthorized()
    {
        var request = new { Nombre = "Sin Auth", Correo = "sin@test.com", Password = "Test123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
