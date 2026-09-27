using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class SecurityHardeningTests : IClassFixture<TestWebApplicationFactory>
{
    private const string Email = "security-admin@test.com";
    private const string Password = "Security123!";
    private readonly TestWebApplicationFactory _factory;

    public SecurityHardeningTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        if (!db.Asesores.Any(user => user.Correo.Value == Email))
        {
            db.Asesores.Add(Asesor.Crear("Security Admin", Enlyce.Domain.ValueObjects.Email.Create(Email),
                BCrypt.Net.BCrypt.HashPassword(Password), "Administrador"));
            db.SaveChanges();
        }
    }

    [Fact]
    public async Task Login_DoesNotExposeToken_AndLogoutRevokesCopiedCookieToken()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { Correo = Email, Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.DoesNotContain("\"token\"", await login.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var copiedToken = AuthCookieTestHelper.ReadToken(login);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", copiedToken);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/logout", new { })).StatusCode);

        var attacker = _factory.CreateClient();
        attacker.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", copiedToken);
        Assert.Equal(HttpStatusCode.Unauthorized, (await attacker.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Login_BlocksRepeatedFailures()
    {
        var client = _factory.CreateClient();
        HttpResponseMessage? response = null;
        for (var attempt = 0; attempt < 6; attempt++)
            response = await client.PostAsJsonAsync("/api/auth/login", new { Correo = "blocked@test.com", Password = "wrong" });

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task Api_ReturnsSecurityHeaders()
    {
        var response = await _factory.CreateClient().GetAsync("/api/health");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Contains("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
    }

    [Fact]
    public async Task LeadEndpoint_RejectsOversizedBody()
    {
        var body = "{\"nombre\":\"" + new string('A', 40_000) + "\",\"email\":\"large@test.com\",\"autorizacionDatos\":true}";
        var response = await _factory.CreateClient().PostAsync("/api/leads",
            new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Contains(response.StatusCode, new[]
        {
            HttpStatusCode.RequestEntityTooLarge,
            HttpStatusCode.BadRequest
        });
    }

    [Fact]
    public async Task RepeatedOpportunity_CreatesSingleActiveRecord()
    {
        var email = $"repeat-{Guid.NewGuid():N}@test.com";
        var request = new
        {
            Nombre = "Persona",
            Email = email,
            Telefono = "3101234567",
            Fuente = "security-test",
            AutorizacionDatos = true,
            TipoOperacion = "Venta"
        };

        var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/leads", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/leads", request)).StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        Assert.Equal(1, db.Leads.Count(lead => lead.Email.Value == email && lead.Activo));
    }
}
