using System.Net;
using System.Net.Http.Json;
using Enlyce.Application.UseCases.GetLeadById;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Enlyce.IntegrationTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public class PipelineEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public PipelineEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateLeadAsync(string tipoOperacion = "Venta")
    {
        var response = await _client.PostAsJsonAsync("/api/leads", new
        {
            Nombre = "Lead Pipeline",
            Email = $"pipeline_{Guid.NewGuid():N}@test.com",
            Telefono = (string?)null,
            Fuente = "Test",
            AutorizacionDatos = true,
            TipoOperacion = tipoOperacion
        });
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<CreateLeadResponse>();
        return content!.Id;
    }

    private Guid SeedAsesor()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var correo = Email.Create($"asesor_{Guid.NewGuid():N}@test.com");
        var hash = BCrypt.Net.BCrypt.HashPassword("Test1234!");
        var asesor = Asesor.Crear("Asesor Test", correo, hash, "Asesor");
        db.Asesores.Add(asesor);
        db.SaveChanges();
        return asesor.Id;
    }

    [Fact]
    public async Task ConsultarPipeline_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/pipeline");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task MoverEtapa_ExistingLead_ReturnsOk()
    {
        var leadId = await CreateLeadAsync();

        var response = await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/mover-etapa", new
        {
            NuevaEtapa = EtapasPipeline.Contactado
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task MoverEtapa_NonExistingLead_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/pipeline/{Guid.NewGuid()}/mover-etapa", new
        {
            NuevaEtapa = EtapasPipeline.Contactado
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AsignarLead_ReturnsOk()
    {
        var leadId = await CreateLeadAsync();
        var asesorId = SeedAsesor();

        var response = await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/asignar", new
        {
            AsesorId = asesorId
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ReasignarLead_ReturnsOk()
    {
        var leadId = await CreateLeadAsync();
        var asesor1 = SeedAsesor();
        var asesor2 = SeedAsesor();

        await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/asignar", new
        {
            AsesorId = asesor1
        });

        var response = await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/reasignar", new
        {
            NuevoAsesorId = asesor2
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RegistrarInteraccion_ReturnsOk()
    {
        var leadId = await CreateLeadAsync();
        var asesorId = SeedAsesor();

        var response = await _client.PostAsJsonAsync("/api/interacciones", new
        {
            LeadId = leadId,
            AsesorId = asesorId,
            Tipo = "Llamada",
            Resumen = "Primer contacto"
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RegistrarInteraccion_NonExistingLead_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/interacciones", new
        {
            LeadId = Guid.NewGuid(),
            AsesorId = Guid.NewGuid(),
            Tipo = "Llamada",
            Resumen = (string?)null
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ObtenerInteraccionesPorLead_ReturnsOk()
    {
        var leadId = await CreateLeadAsync();

        var response = await _client.GetAsync($"/api/interacciones/lead/{leadId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ObtenerAlertas_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/alertas");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task FullFlow_CrearLeadMoverEtapa()
    {
        var leadId = await CreateLeadAsync();
        var asesorId = SeedAsesor();

        await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/asignar", new
        {
            AsesorId = asesorId
        });

        await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/mover-etapa", new
        {
            NuevaEtapa = EtapasPipeline.Contactado
        });

        await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/mover-etapa", new
        {
            NuevaEtapa = EtapasPipeline.Cualificado
        });

        await _client.PostAsJsonAsync("/api/interacciones", new
        {
            LeadId = leadId,
            AsesorId = asesorId,
            Tipo = "Llamada",
            Resumen = "Interesado en zona norte"
        });

        await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/mover-etapa", new
        {
            NuevaEtapa = EtapasPipeline.VisitaAgendada
        });

        var lead = await _client.GetFromJsonAsync<GetLeadByIdResponse>($"/api/leads/{leadId}");
        Assert.Equal(EtapasPipeline.VisitaAgendada, lead!.EtapaPipeline);
    }
}
