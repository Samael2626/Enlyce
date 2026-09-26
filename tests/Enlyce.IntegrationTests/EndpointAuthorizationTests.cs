using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class EndpointAuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public EndpointAuthorizationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public void EveryApiRoute_HasExplicitAuthorizationDecision()
    {
        using var client = _factory.CreateClient();
        var routes = _factory.Services.GetRequiredService<EndpointDataSource>()
            .Endpoints.OfType<RouteEndpoint>()
            .Where(endpoint =>
            {
                var path = endpoint.RoutePattern.RawText?.Trim('/') ?? string.Empty;
                return path == "health" || path.StartsWith("api/", StringComparison.Ordinal);
            }).ToList();
        Assert.NotEmpty(routes);

        var publicRoutes = new HashSet<string>(StringComparer.Ordinal)
        {
            "health", "api/auth/login", "api/auth/seed", "api/leads",
            "api/leads/{id:guid}/owner-details",
            "api/public/inmuebles", "api/public/inmuebles/{slug}",
            "api/politica/activa", "api/webhooks/wompi"
        };

        foreach (var endpoint in routes)
        {
            var path = endpoint.RoutePattern.RawText!.Trim('/');
            var isPublic = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;
            var isProtected = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Count > 0;
            Assert.Equal(publicRoutes.Contains(path), isPublic);
            if (!isPublic)
                Assert.True(isProtected, $"Ruta privada sin autorizacion: {path}");
        }
    }

    [Theory]
    [InlineData("GET", "/api/leads/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "/api/auth/register")]
    [InlineData("POST", "/api/auth/logout")]
    [InlineData("GET", "/api/auth/me")]
    [InlineData("GET", "/api/inmuebles/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "/api/inmuebles")]
    [InlineData("POST", "/api/interacciones")]
    [InlineData("GET", "/api/interacciones/lead/11111111-1111-1111-1111-111111111111")]
    [InlineData("GET", "/api/alertas")]
    [InlineData("GET", "/api/pipeline")]
    [InlineData("PUT", "/api/pipeline/11111111-1111-1111-1111-111111111111/mover-etapa")]
    [InlineData("PUT", "/api/pipeline/11111111-1111-1111-1111-111111111111/asignar")]
    [InlineData("PUT", "/api/pipeline/11111111-1111-1111-1111-111111111111/reasignar")]
    [InlineData("GET", "/api/datos-personales/11111111-1111-1111-1111-111111111111")]
    [InlineData("DELETE", "/api/datos-personales/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "/api/visitas")]
    [InlineData("GET", "/api/visitas")]
    [InlineData("GET", "/api/visitas/lead/11111111-1111-1111-1111-111111111111")]
    [InlineData("GET", "/api/visitas/asesor/11111111-1111-1111-1111-111111111111?Desde=2020-01-01T00:00:00Z")]
    [InlineData("GET", "/api/billing/pricing")]
    [InlineData("POST", "/api/billing/checkout-sessions")]
    [InlineData("GET", "/api/billing/orders/ENL-11111111111111111111111111111111")]
    public async Task PrivateEndpoint_WithoutToken_ReturnsUnauthorized(string method, string path)
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/leads/{0}")]
    [InlineData("GET", "/api/interacciones/lead/{0}")]
    [InlineData("GET", "/api/datos-personales/{0}")]
    [InlineData("DELETE", "/api/datos-personales/{0}")]
    public async Task LeadResource_OtherAdvisor_ReturnsForbidden(string method, string pathFormat)
    {
        var (ownerClient, owner) = _factory.CreateAuthenticatedClient("Asesor");
        ownerClient.Dispose();
        var lead = SeedLead(owner.Id);
        var (otherClient, _) = _factory.CreateAuthenticatedClient("Asesor");
        using (otherClient)
        using (var request = new HttpRequestMessage(new HttpMethod(method), string.Format(pathFormat, lead.Id)))
        {
            var response = await otherClient.SendAsync(request);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task PersonalData_AssignedAdvisorCanReadAndSuppress()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var read = await client.GetAsync($"/api/datos-personales/{lead.Id}");
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);

            var delete = await client.DeleteAsync($"/api/datos-personales/{lead.Id}");
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        }
    }

    [Fact]
    public async Task PersonalData_AnonymousDeleteDoesNotChangeLead()
    {
        var lead = SeedLead(Guid.NewGuid());
        using var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/datos-personales/{lead.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(await IsLeadActiveAsync(lead.Id));
    }

    [Fact]
    public async Task PersonalData_OtherAdvisorDeleteDoesNotChangeLead()
    {
        var lead = SeedLead(Guid.NewGuid());
        var (client, _) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.DeleteAsync($"/api/datos-personales/{lead.Id}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        Assert.True(await IsLeadActiveAsync(lead.Id));
    }

    [Theory]
    [InlineData("/api/leads/{0}")]
    [InlineData("/api/interacciones/lead/{0}")]
    public async Task LeadResource_AssignedAdvisorCanRead(string pathFormat)
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var response = await client.GetAsync(string.Format(pathFormat, lead.Id));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Pipeline_AssignedAdvisorSeesOnlyOwnLeads()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var ownLead = SeedLead(user.Id);
            var otherLead = SeedLead(Guid.NewGuid());
            var response = await client.GetAsync("/api/pipeline");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(ownLead.Id.ToString(), body);
            Assert.DoesNotContain(otherLead.Id.ToString(), body);
        }
    }

    [Fact]
    public async Task Alerts_AssignedAdvisorSeesOnlyOwnLeads()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var ownLead = SeedLead(user.Id);
            var otherLead = SeedLead(Guid.NewGuid());
            var response = await client.GetAsync("/api/alertas");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(ownLead.Id.ToString(), body);
            Assert.DoesNotContain(otherLead.Id.ToString(), body);
        }
    }

    [Fact]
    public async Task Interaction_OtherAdvisorCannotRegisterForAssignedLead()
    {
        var (ownerClient, owner) = _factory.CreateAuthenticatedClient("Asesor");
        ownerClient.Dispose();
        var lead = SeedLead(owner.Id);
        var (client, other) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.PostAsJsonAsync("/api/interacciones", new
            {
                LeadId = lead.Id, AsesorId = other.Id, Tipo = "Llamada", Resumen = "No autorizado"
            });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Visit_OtherAdvisorCannotRegisterForAssignedLead()
    {
        var (ownerClient, owner) = _factory.CreateAuthenticatedClient("Asesor");
        ownerClient.Dispose();
        var lead = SeedLead(owner.Id);
        var (client, other) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.PostAsJsonAsync("/api/visitas", new
            {
                LeadId = lead.Id, InmuebleId = Guid.NewGuid(), AsesorId = other.Id,
                FechaProgramada = DateTime.UtcNow.AddDays(1)
            });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Theory]
    [InlineData("asignar")]
    [InlineData("reasignar")]
    public async Task PipelineAssignment_AdvisorCannotChangeAssignment(string action)
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var response = await client.PutAsJsonAsync($"/api/pipeline/{lead.Id}/{action}", new
            {
                AsesorId = Guid.NewGuid(), NuevoAsesorId = Guid.NewGuid()
            });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task PipelineMove_OtherAdvisorCannotMoveLead()
    {
        var (ownerClient, owner) = _factory.CreateAuthenticatedClient("Asesor");
        ownerClient.Dispose();
        var lead = SeedLead(owner.Id);
        var (client, _) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.PutAsJsonAsync($"/api/pipeline/{lead.Id}/mover-etapa", new
            {
                NuevaEtapa = EtapasPipeline.Contactado
            });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Interaction_AssignedAdvisorCanRegisterOwnInteraction()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var response = await client.PostAsJsonAsync("/api/interacciones", new
            {
                LeadId = lead.Id, AsesorId = user.Id, Tipo = "Llamada", Resumen = "Contacto valido"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Interaction_AssignedAdvisorCannotImpersonateAnotherAdvisor()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var response = await client.PostAsJsonAsync("/api/interacciones", new
            {
                LeadId = lead.Id, AsesorId = Guid.NewGuid(), Tipo = "Llamada", Resumen = "Suplantacion"
            });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Interaction_AssignedAdvisorSeesOnlyOwnHistory()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.Interacciones.AddRange(
                    Interaccion.Registrar(lead.Id, user.Id, "Llamada"),
                    Interaccion.Registrar(lead.Id, Guid.NewGuid(), "Correo"));
                db.SaveChanges();
            }

            var response = await client.GetAsync($"/api/interacciones/lead/{lead.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var history = json.RootElement.EnumerateArray().ToList();
            Assert.Single(history);
            Assert.Equal(user.Id, history[0].GetProperty("asesorId").GetGuid());
        }
    }

    [Fact]
    public async Task Visit_AssignedAdvisorCanRegisterOwnVisit()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var response = await client.PostAsJsonAsync("/api/visitas", new
            {
                LeadId = lead.Id, InmuebleId = Guid.NewGuid(), AsesorId = user.Id,
                FechaProgramada = DateTime.UtcNow.AddDays(1)
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Visit_AssignedAdvisorCannotImpersonateAnotherAdvisor()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var response = await client.PostAsJsonAsync("/api/visitas", new
            {
                LeadId = lead.Id, InmuebleId = Guid.NewGuid(), AsesorId = Guid.NewGuid(),
                FechaProgramada = DateTime.UtcNow.AddDays(1)
            });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task PipelineMove_AssignedAdvisorCanMoveLead()
    {
        var (client, user) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var lead = SeedLead(user.Id);
            var response = await client.PutAsJsonAsync($"/api/pipeline/{lead.Id}/mover-etapa", new
            {
                NuevaEtapa = EtapasPipeline.Contactado
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task PublicLeadCapture_WithoutToken_ReturnsCreated()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/leads", new CreateLeadRequest(
            "Lead Publico", $"public-{Guid.NewGuid():N}@test.com", null, "Website", true));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PublicCatalog_WithoutToken_ReturnsOk()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/public/inmuebles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PublicProperty_WithoutToken_ReturnsOk()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            await PublicCatalogTestData.SeedAsync(db);
        }
        using var client = _factory.CreateClient();
        var response = await client.GetAsync(
            $"/api/public/inmuebles/{PublicCatalogTestData.PublishedApartmentSlug}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/politica/activa")]
    [InlineData("/health")]
    public async Task PublicInformation_WithoutToken_ReturnsOk(string path)
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DevelopmentSeed_WithoutExplicitOptIn_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();
        Assert.True(_factory.Services.GetRequiredService<IWebHostEnvironment>().IsDevelopment());
        var response = await client.PostAsync("/api/auth/seed", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Lead SeedLead(Guid advisorId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var lead = Lead.Crear(
            "Lead Privado", Email.Create($"private-{Guid.NewGuid():N}@test.com"),
            null, "Test", true);
        lead.AsignarAsesor(advisorId);
        lead.RegistrarContacto();
        db.Leads.Add(lead);
        db.SaveChanges();
        return lead;
    }

    private async Task<bool> IsLeadActiveAsync(Guid leadId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        return await db.Leads.AsNoTracking()
            .Where(lead => lead.Id == leadId)
            .Select(lead => lead.Activo)
            .SingleAsync();
    }
}
