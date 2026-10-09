using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class AnalyticsEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task PublicEventIsAcceptedAndSessionIsHashed()
    {
        var sessionId = Guid.NewGuid();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/public/analytics/events", new
        {
            sessionId = sessionId.ToString(),
            @event = "page_view",
            path = "/inmuebles",
            propertySlug = (string?)null
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var stored = await db.WebAnalyticsEvents.OrderByDescending(item => item.OccurredAt).FirstAsync();
        Assert.NotEqual(sessionId.ToString(), stored.SessionHash);
        Assert.Equal(64, stored.SessionHash.Length);
    }

    [Fact]
    public async Task FunnelRequiresAdministratorAndReturnsOrderedSteps()
    {
        using var anonymous = factory.CreateClient();
        var unauthorized = await anonymous.GetAsync("/api/analytics/funnel");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var (administrator, _) = factory.CreateAuthenticatedClient();
        var response = await administrator.GetAsync("/api/analytics/funnel");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<FunnelResponse>();
        Assert.Equal(
            ["page_view", "search", "favorite", "form_started", "conversion"],
            payload!.Steps.Select(step => step.Event));
    }

    [Fact]
    public async Task FirstResponseMetricsUseFirstInteractionAdvisorAfterReassignmentAndIgnoreUnansweredLeads()
    {
        var from = new DateOnly(2020, 2, 3);
        var createdAt = from.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            var advisor = Asesor.Crear("Metric Advisor", Email.Create($"{Guid.NewGuid():N}@test.com"), "password-hash");
            var currentOwner = Asesor.Crear("Current Owner", Email.Create($"{Guid.NewGuid():N}@test.com"), "password-hash");
            db.Asesores.AddRange(advisor, currentOwner);
            var reassignedLeadOne = CreateLead("Web", createdAt, createdAt.AddHours(2), currentOwner.Id);
            var reassignedLeadTwo = CreateLead("Web", createdAt.AddHours(1), createdAt.AddHours(5), currentOwner.Id);
            db.Leads.AddRange(
                reassignedLeadOne,
                reassignedLeadTwo,
                CreateLead("Web", createdAt.AddHours(2), createdAt.AddHours(5), currentOwner.Id),
                CreateLead("Web", createdAt.AddHours(2), null),
                CreateLead("Web", from.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), createdAt));
            db.Interacciones.AddRange(
                Interaccion.Reconstituir(Guid.NewGuid(), reassignedLeadOne.Id, advisor.Id, "Llamada", null, createdAt.AddHours(2)),
                Interaccion.Reconstituir(Guid.NewGuid(), reassignedLeadTwo.Id, advisor.Id, "Correo", null, createdAt.AddHours(5)));
            await db.SaveChangesAsync();
        }

        var (administrator, _) = factory.CreateAuthenticatedClient();
        var response = await administrator.GetAsync("/api/analytics/first-response?from=2020-02-03&to=2020-02-03");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<FirstResponseResponse>();
        Assert.Equal(3, payload!.RespondedLeads);
        Assert.Equal(3, payload.AverageHours);
        Assert.Equal(3, Assert.Single(payload.BySource).RespondedLeads);
        Assert.Equal(2, Assert.Single(payload.ByAdvisor, item => item.Name == "Metric Advisor").RespondedLeads);
        Assert.Equal(1, Assert.Single(payload.ByAdvisor, item => item.Name == "Sin dato de asesor").RespondedLeads);
        Assert.DoesNotContain(payload.ByAdvisor, item => item.Name == "Current Owner");
    }

    private static Lead CreateLead(string source, DateTime createdAt, DateTime? firstContact, Guid? advisorId = null) =>
        Lead.Reconstituir(
            Guid.NewGuid(), "Metric lead", Email.Create($"{Guid.NewGuid():N}@test.com"), null,
            source, EstadoLead.Nuevo, MotivoCierre.Ninguno, null, advisorId, createdAt,
            null, null, true, true, fechaPrimerContacto: firstContact);

    private sealed record FunnelResponse(IReadOnlyList<FunnelStep> Steps);
    private sealed record FunnelStep(string Event, int UniqueSessions, int TotalEvents, decimal RateFromVisits);
    private sealed record FirstResponseResponse(int RespondedLeads, double? AverageHours, IReadOnlyList<FirstResponseGroup> ByAdvisor, IReadOnlyList<FirstResponseGroup> BySource);
    private sealed record FirstResponseGroup(string Name, int RespondedLeads, double AverageHours);
}
