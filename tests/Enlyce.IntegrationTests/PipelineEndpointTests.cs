using System.Net;
using System.Net.Http.Json;
using Enlyce.Application.UseCases.GetLeadById;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.Queries.Lead;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Enlyce.IntegrationTests;
using Microsoft.EntityFrameworkCore;
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
        _client = factory.CreateAuthenticatedClient().Client;
    }

    private async Task<Guid> CreateLeadAsync(string tipoOperacion = "Venta")
    {
        var email = $"pipeline_{Guid.NewGuid():N}@test.com";
        var response = await _client.PostAsJsonAsync("/api/leads", new
        {
            Nombre = "Lead Pipeline",
            Email = email,
            Telefono = (string?)null,
            Fuente = "Test",
            AutorizacionDatos = true,
            TipoOperacion = tipoOperacion
        });
        response.EnsureSuccessStatusCode();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        return await db.Leads
            .Where(item => item.Email.Value == email)
            .Select(item => item.Id)
            .SingleAsync();
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

    private Guid SeedLeadWithoutAssignment()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var lead = Lead.Crear("Sin asignar", Email.Create($"unassigned-{Guid.NewGuid():N}@test.com"),
            null, "Test", true);
        db.Leads.Add(lead);
        db.SaveChanges();
        return lead.Id;
    }

    [Fact]
    public async Task ConsultarPipeline_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/pipeline");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ConsultarPipeline_FiltersBySearchStageOperationAdvisorAndCreationDates()
    {
        var advisorA = SeedAsesor();
        var advisorB = SeedAsesor();
        var createdAt = new DateTime(2026, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        var suffix = Guid.NewGuid().ToString("N");
        var targetName = $"PipelineFilter{suffix}";
        var targetEmail = $"pipeline-filter-{suffix}@test.com";
        var target = Lead.Reconstituir(
            Guid.NewGuid(), targetName, Email.Create(targetEmail), Telefono.Create("3105551234"),
            "Web", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, advisorA,
            createdAt, null, createdAt, true, true, "Venta", EtapasPipeline.LeadNuevo);
        var otherAdvisorLead = Lead.Reconstituir(
            Guid.NewGuid(), "Other advisor", Email.Create($"other-{suffix}@test.com"), null,
            "Web", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, advisorB,
            createdAt, null, createdAt, true, true, "Venta", EtapasPipeline.LeadNuevo);
        var otherOperationLead = Lead.Reconstituir(
            Guid.NewGuid(), "Other operation", Email.Create($"rent-{suffix}@test.com"), null,
            "Web", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, advisorA,
            createdAt, null, createdAt, true, true, "Arriendo", EtapasPipeline.LeadNuevo);
        var outsideDateLead = Lead.Reconstituir(
            Guid.NewGuid(), targetName, Email.Create($"outside-{suffix}@test.com"), Telefono.Create("3105551234"),
            "Web", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, advisorA,
            createdAt.AddDays(-1), null, createdAt.AddDays(-1), true, true, "Venta", EtapasPipeline.LeadNuevo);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.Leads.AddRange(target, otherAdvisorLead, otherOperationLead, outsideDateLead);
            await db.SaveChangesAsync();
        }

        var url = $"/api/pipeline?q={targetName}&etapa={EtapasPipeline.LeadNuevo}" +
            $"&operacion=Venta&asesorId={advisorA}&desde=2026-06-15&hasta=2026-06-15";
        var result = await _client.GetFromJsonAsync<PipelineResponse>(url);

        Assert.NotNull(result);
        Assert.Single(result!.Leads);
        Assert.Equal(target.Id, result.Leads[0].Id);

        var emailSearch = await _client.GetFromJsonAsync<PipelineResponse>(
            $"/api/pipeline?q={Uri.EscapeDataString(targetEmail)}");
        Assert.NotNull(emailSearch);
        Assert.Contains(emailSearch!.Leads, lead => lead.Id == target.Id);
    }

    [Fact]
    public async Task ConsultarPipeline_PaginatesWithStablePageMetadataAndTotal()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var createdAt = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);
        var leads = Enumerable.Range(0, 3).Select(index => Lead.Reconstituir(
            Guid.NewGuid(), $"Page{suffix}", Email.Create($"page-{index}-{suffix}@test.com"), null,
            "Test", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, null,
            createdAt, null, null, true, true, "Venta", EtapasPipeline.LeadNuevo)).ToArray();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.Leads.AddRange(leads);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/pipeline?q=Page{suffix}&page=2&pageSize=2");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PipelineResponse>();

        Assert.NotNull(result);
        Assert.Equal(3, result!.Total);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Single(result.Leads);
        var repeated = await _client.GetFromJsonAsync<PipelineResponse>(
            $"/api/pipeline?q=Page{suffix}&page=2&pageSize=2");
        Assert.NotNull(repeated);
        Assert.Equal(result.Leads[0].Id, repeated!.Leads[0].Id);
    }

    [Fact]
    public async Task AsignarMasivo_ReturnsPerLeadResultsAndRecordsAudit()
    {
        var leadA = SeedLeadWithoutAssignment();
        var leadB = SeedLeadWithoutAssignment();
        var advisorId = SeedAsesor();
        var missingLead = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync("/api/pipeline/asignar-masivo", new
        {
            LeadIds = new[] { leadA, leadB, missingLead },
            AsesorId = advisorId,
            Reason = "Distribución por zona"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkAssignmentResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Succeeded);
        Assert.Equal(1, result.Failed);
        Assert.Contains(result.Results, item => item.LeadId == missingLead && !item.Success);
        foreach (var leadId in new[] { leadA, leadB })
        {
            var history = await _client.GetFromJsonAsync<AssignmentHistoryDto[]>(
                $"/api/pipeline/{leadId}/asignaciones");
            Assert.NotNull(history);
            Assert.Single(history!);
            Assert.Equal("Distribución por zona", history[0].Reason);
            Assert.Equal(advisorId, history[0].NewAdvisorId);
        }
    }

    [Fact]
    public async Task AsignarMasivo_RejectsMoreThanOneHundredIds()
    {
        var response = await _client.PutAsJsonAsync("/api/pipeline/asignar-masivo", new
        {
            LeadIds = Enumerable.Range(0, 101).Select(_ => Guid.NewGuid()).ToArray(),
            AsesorId = SeedAsesor(),
            Reason = "Lote grande"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
            AsesorId = asesorId,
            Reason = "Cobertura de zona"
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AsignarLead_RechazaAsesorInactivo()
    {
        var leadId = await CreateLeadAsync();
        var advisor = Asesor.Reconstituir(Guid.NewGuid(), "Asesor inactivo",
            Email.Create($"inactive-{Guid.NewGuid():N}@test.com"), "test-hash", "Asesor",
            DateTime.UtcNow, false);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.Asesores.Add(advisor);
            await db.SaveChangesAsync();
        }

        var response = await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/asignar", new
        {
            AsesorId = advisor.Id,
            Reason = "Asignacion para prueba"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReasignarLead_ReturnsOk()
    {
        var leadId = await CreateLeadAsync();
        var asesor1 = SeedAsesor();
        var asesor2 = SeedAsesor();

        await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/asignar", new
        {
            AsesorId = asesor1,
            Reason = "Asignacion inicial"
        });

        var response = await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/reasignar", new
        {
            NuevoAsesorId = asesor2,
            Reason = "Cambio de responsable"
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AssignmentHistory_StoresActorReasonAndPreviousAndNewAdvisor()
    {
        var leadId = SeedLeadWithoutAssignment();
        var firstAdvisor = SeedAsesor();
        var secondAdvisor = SeedAsesor();

        var firstAssignment = await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/asignar", new
        {
            AsesorId = firstAdvisor,
            Reason = "Cobertura inicial"
        });
        firstAssignment.EnsureSuccessStatusCode();
        var reassignment = await _client.PutAsJsonAsync($"/api/pipeline/{leadId}/reasignar", new
        {
            NuevoAsesorId = secondAdvisor,
            Reason = "Cambio de zona"
        });
        reassignment.EnsureSuccessStatusCode();

        var history = await _client.GetFromJsonAsync<AssignmentHistoryDto[]>(
            $"/api/pipeline/{leadId}/asignaciones");
        Assert.NotNull(history);
        Assert.Equal(2, history.Length);
        Assert.Null(history[0].PreviousAdvisorId);
        Assert.Equal(firstAdvisor, history[0].NewAdvisorId);
        Assert.Equal("ManualAssignment", history[0].Source);
        Assert.Equal("Cobertura inicial", history[0].Reason);
        Assert.Equal(secondAdvisor, history[1].NewAdvisorId);
        Assert.Equal(firstAdvisor, history[1].PreviousAdvisorId);
        Assert.Equal("Cambio de zona", history[1].Reason);
        Assert.Equal("ManualReassignment", history[1].Source);
        Assert.All(history, item => Assert.NotNull(item.ChangedByAdvisorId));
    }

    [Fact]
    public async Task AssignmentHistory_RecordsAutomaticWebAssignment()
    {
        SeedAsesor();
        var leadId = await CreateLeadAsync();

        var history = await _client.GetFromJsonAsync<AssignmentHistoryDto[]>(
            $"/api/pipeline/{leadId}/asignaciones");

        Assert.NotNull(history);
        Assert.NotEmpty(history);
        Assert.Equal("AutomaticLoadBalance", history[0].Source);
        Assert.Null(history[0].PreviousAdvisorId);
        Assert.Null(history[0].ChangedByAdvisorId);
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

    private sealed record AssignmentHistoryDto(Guid? PreviousAdvisorId, Guid NewAdvisorId,
        Guid? ChangedByAdvisorId, string Reason, string Source, DateTime ChangedAt);
    private sealed record BulkAssignmentResponse(
        BulkAssignmentItem[] Results, int Succeeded, int Failed);
    private sealed record BulkAssignmentItem(Guid LeadId, bool Success, string? Error);

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
            AsesorId = asesorId,
            Reason = "Flujo de prueba"
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
