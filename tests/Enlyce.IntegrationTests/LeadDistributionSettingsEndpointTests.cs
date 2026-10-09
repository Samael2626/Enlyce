using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class LeadDistributionSettingsEndpointTests
{
    [Fact]
    public async Task Administrator_CanChangeAndReadDistributionRule()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateAuthenticatedClient("Administrador").Client;

        var update = await client.PutAsJsonAsync("/api/configuracion/reparto-leads", new
        {
            Rule = "RoundRobin"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("RoundRobin", (await update.Content.ReadFromJsonAsync<RuleResponse>())!.Rule);

        var read = await client.GetFromJsonAsync<RuleResponse>("/api/configuracion/reparto-leads");
        Assert.Equal("RoundRobin", read!.Rule);

        var invalid = await client.PutAsJsonAsync("/api/configuracion/reparto-leads", new
        {
            Rule = "Random"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var advisor = Asesor.Crear("Asesor de rotación", Email.Create($"rotation-{Guid.NewGuid():N}@test.com"),
            BCrypt.Net.BCrypt.HashPassword("OnlyForTests123!"), "Asesor");
        db.Asesores.Add(advisor);
        await db.SaveChangesAsync();

        var leadEmail = $"lead-{Guid.NewGuid():N}@test.com";
        var leadResponse = await client.PostAsJsonAsync("/api/leads", new
        {
            Nombre = "Lead de rotación",
            Email = leadEmail,
            Telefono = (string?)null,
            Fuente = "Website",
            AutorizacionDatos = true,
            TipoOperacion = "Venta"
        });
        leadResponse.EnsureSuccessStatusCode();
        var persistedLead = await db.Leads.SingleAsync(item => item.Email.Value == leadEmail);
        Assert.Equal(advisor.Id, persistedLead!.AsesorAsignadoId);
        var history = await client.GetFromJsonAsync<HistoryResponse[]>(
            $"/api/pipeline/{persistedLead.Id}/asignaciones");
        Assert.NotNull(history);
        Assert.Equal(advisor.Id, history.Single().NewAdvisorId);
        Assert.Equal("AutomaticRoundRobin", history.Single().Source);
    }

    private sealed record RuleResponse(string Rule);
    private sealed record HistoryResponse(Guid NewAdvisorId, string Source);
}
