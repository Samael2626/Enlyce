using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class LeadSlaAlertsEndpointTests
{
    [Fact]
    public async Task AppliesSpecificityAndDisabledSpecificRuleBlocksFallback()
    {
        using var factory = new TestWebApplicationFactory();
        var admin = factory.CreateAuthenticatedClient("Administrador");
        using var client = admin.Client;
        var advisor = admin.User;
        var now = DateTime.UtcNow;
        SeedRule(factory, "*", "*", 60, 1, true);
        SeedRule(factory, "PORTAL", "Venta", 300, 1, true);
        SeedRule(factory, "WEB", "*", 300, 1, true);
        SeedRule(factory, "*", "Arriendo", 300, 1, true);
        SeedRule(factory, "BLOCKED", "Venta", 60, 1, false);
        SeedLead(factory, advisor.Id, "Portal|utm=cpc", "Venta", now.AddHours(-4));
        SeedLead(factory, advisor.Id, "Web|PublicationReview:x", "Venta", now.AddHours(-4));
        SeedLead(factory, advisor.Id, "Other", "Arriendo", now.AddHours(-4));
        var blocked = SeedLead(factory, advisor.Id, "Blocked", "Venta", now.AddHours(-4));
        var catchAll = SeedLead(factory, advisor.Id, "Other", "Venta", now.AddHours(-4));

        var response = await GetSlaAsync(client, "/api/alertas/sla");

        Assert.NotNull(response);
        var alert = Assert.Single(response.Alerts);
        Assert.Equal(catchAll.Id, alert.LeadId);
        Assert.Equal("OTHER", alert.SourceKey);
        Assert.Equal("FirstResponse", alert.Kind);
        Assert.Equal(1, response.Total);
        Assert.DoesNotContain(response.Alerts, item => item.LeadId == blocked.Id);
    }

    [Fact]
    public async Task UsesEarliestAssignmentAndFallbackAndDoesNotRestartOnReassignment()
    {
        using var factory = new TestWebApplicationFactory();
        var admin = factory.CreateAuthenticatedClient("Administrador");
        using var client = admin.Client;
        var advisor = admin.User;
        var (_, secondAdvisor) = factory.CreateAuthenticatedClient("Asesor");
        var now = DateTime.UtcNow;
        SeedRule(factory, "*", "*", 2, null, true);
        var historyLead = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddHours(-1));
        AddAssignmentHistory(factory, historyLead.Id, advisor.Id, null, now.AddHours(-5));
        AddAssignmentHistory(factory, historyLead.Id, secondAdvisor.Id, advisor.Id, now.AddHours(-1));
        var fallbackLead = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddHours(-4), addHistory: false);
        var neverAssigned = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddHours(-6),
            assigned: false, addHistory: false);

        var response = await GetSlaAsync(client, "/api/alertas/sla");

        Assert.NotNull(response);
        Assert.Equal(2, response.Total);
        Assert.Contains(response.Alerts, item => item.LeadId == historyLead.Id &&
            item.StartedAtUtc < now.AddHours(-4));
        Assert.Contains(response.Alerts, item => item.LeadId == fallbackLead.Id);
        Assert.DoesNotContain(response.Alerts, item => item.LeadId == neverAssigned.Id);
    }

    [Fact]
    public async Task FirstResponseEndsFirstSlaThenInactivityUsesLastInteractionOrFirstResponse()
    {
        using var factory = new TestWebApplicationFactory();
        var admin = factory.CreateAuthenticatedClient("Administrador");
        using var client = admin.Client;
        var advisor = admin.User;
        var now = DateTime.UtcNow;
        SeedRule(factory, "*", "*", 1, 2, true);
        var staleInteraction = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddDays(-8),
            firstContactAt: now.AddDays(-5), lastInteractionAt: now.AddDays(-3));
        var firstContactFallback = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddDays(-8),
            firstContactAt: now.AddDays(-4));
        var unanswered = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddDays(-4));
        var response = await GetSlaAsync(client, "/api/alertas/sla");

        Assert.NotNull(response);
        Assert.Equal(3, response.Total);
        Assert.Contains(response.Alerts, item => item.LeadId == staleInteraction.Id &&
            item.Kind == "Inactivity" && item.StartedAtUtc >= now.AddDays(-3).AddSeconds(-1));
        Assert.Contains(response.Alerts, item => item.LeadId == firstContactFallback.Id &&
            item.Kind == "Inactivity");
        Assert.Contains(response.Alerts, item => item.LeadId == unanswered.Id &&
            item.Kind == "FirstResponse");
    }

    [Fact]
    public async Task FirstResponseSlaExpiresAtConfiguredMinuteThreshold()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateAuthenticatedClient("Administrador").Client;
        var advisor = factory.CreateAuthenticatedClient("Administrador").User;
        var now = DateTime.UtcNow;
        var assignedAt = now.AddMinutes(-46);
        SeedRule(factory, "*", "*", 45, null, true);
        var lead = SeedLead(factory, advisor.Id, "Source", "Venta", assignedAt);
        var earlyLead = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddMinutes(-44));

        var response = await GetSlaAsync(client, "/api/alertas/sla");

        Assert.NotNull(response);
        var alert = Assert.Single(response.Alerts);
        Assert.Equal(lead.Id, alert.LeadId);
        Assert.Equal(assignedAt.AddMinutes(45), alert.DueAtUtc);
        Assert.True(alert.OverdueMinutes >= 1);
        Assert.DoesNotContain(response.Alerts, item => item.LeadId == earlyLead.Id);
    }

    [Fact]
    public async Task ExcludesClosedAndInactiveAndScopesAdvisorToCurrentAssignments()
    {
        using var factory = new TestWebApplicationFactory();
        var advisorSession = factory.CreateAuthenticatedClient("Asesor");
        using var advisorClient = advisorSession.Client;
        var advisor = advisorSession.User;
        var otherSession = factory.CreateAuthenticatedClient("Asesor");
        using var otherClient = otherSession.Client;
        var otherAdvisor = otherSession.User;
        using var adminClient = factory.CreateAuthenticatedClient("Administrador").Client;
        var now = DateTime.UtcNow;
        SeedRule(factory, "*", "*", 1, null, true);
        var own = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddHours(-5));
        var other = SeedLead(factory, otherAdvisor.Id, "Source", "Venta", now.AddHours(-5));
        SeedLead(factory, advisor.Id, "Source", "Venta", now.AddHours(-5),
            stage: EtapasPipeline.CerradoPerdido);
        SeedLead(factory, advisor.Id, "Source", "Venta", now.AddHours(-5), active: false);

        var ownResponse = await GetSlaAsync(advisorClient,
            $"/api/alertas/sla?advisorId={otherAdvisor.Id}");
        var adminResponse = await GetSlaAsync(adminClient, "/api/alertas/sla");

        Assert.NotNull(ownResponse);
        Assert.Equal(1, ownResponse.Total);
        Assert.Equal(own.Id, Assert.Single(ownResponse.Alerts).LeadId);
        Assert.NotNull(adminResponse);
        Assert.Equal(2, adminResponse.Total);
        Assert.Contains(adminResponse.Alerts, item => item.LeadId == other.Id);
    }

    [Fact]
    public async Task ReturnsOnlyHundredMostOverdueAndKeepsFullTotal()
    {
        using var factory = new TestWebApplicationFactory();
        var admin = factory.CreateAuthenticatedClient("Administrador");
        using var client = admin.Client;
        var advisor = admin.User;
        var now = DateTime.UtcNow;
        SeedRule(factory, "*", "*", 1, null, true);
        var oldest = SeedLead(factory, advisor.Id, "Source", "Venta", now.AddDays(-20));
        for (var index = 0; index < 100; index++)
            SeedLead(factory, advisor.Id, "Source", "Venta", now.AddHours(-2));

        var response = await GetSlaAsync(client, "/api/alertas/sla");

        Assert.NotNull(response);
        Assert.Equal(101, response.Total);
        Assert.Equal(100, response.Alerts.Count);
        Assert.Equal(oldest.Id, response.Alerts[0].LeadId);
    }

    private static Lead SeedLead(TestWebApplicationFactory factory, Guid advisorId,
        string source, string operation, DateTime createdAt, DateTime? assignedAt = null,
        DateTime? firstContactAt = null, DateTime? lastInteractionAt = null,
        string stage = EtapasPipeline.LeadNuevo, bool active = true, bool addHistory = true,
        bool assigned = true)
    {
        var leadId = Guid.NewGuid();
        DateTime? effectiveAssignedAt = assigned ? assignedAt ?? createdAt : null;
        var assignedDate = effectiveAssignedAt;
        var lead = Lead.Reconstituir(leadId, "SLA lead", Email.Create($"{leadId:N}@test.com"), null,
            source, EstadoLead.Nuevo, MotivoCierre.Ninguno, null, effectiveAssignedAt is null ? null : advisorId,
            createdAt, null, effectiveAssignedAt, true, active, operation, stage, 0, lastInteractionAt,
            createdAt, fechaPrimerContacto: firstContactAt);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        db.Leads.Add(lead);
        if (addHistory && assignedDate.HasValue)
            db.LeadAssignmentHistory.Add(LeadAssignmentHistory.Create(leadId, null, advisorId, null,
                "Seed assignment", LeadAssignmentSource.Legacy, assignedDate.Value));
        db.SaveChanges();
        return lead;
    }

    private static void SeedRule(TestWebApplicationFactory factory, string source,
        string operation, int minutes, int? days, bool enabled)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        db.LeadSlaRules.Add(LeadSlaRule.Create(source, operation, minutes, days, enabled));
        db.SaveChanges();
    }

    private static void AddAssignmentHistory(TestWebApplicationFactory factory, Guid leadId,
        Guid newAdvisorId, Guid? previousAdvisorId, DateTime changedAt)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        db.LeadAssignmentHistory.Add(LeadAssignmentHistory.Create(leadId,
            previousAdvisorId, newAdvisorId, null, "Seed assignment", LeadAssignmentSource.Legacy, changedAt));
        db.SaveChanges();
    }

    private static async Task<SlaResponse?> GetSlaAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,
            $"SLA endpoint returned {(int)response.StatusCode}: {content}");
        return JsonSerializer.Deserialize<SlaResponse>(content,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private sealed record SlaResponse(IReadOnlyList<SlaAlert> Alerts, int Total);
    private sealed record SlaAlert(Guid LeadId, string Nombre, string Email, string OperationType,
        string SourceKey, string Stage, string Kind, DateTime StartedAtUtc, DateTime DueAtUtc,
        int OverdueMinutes);
}
