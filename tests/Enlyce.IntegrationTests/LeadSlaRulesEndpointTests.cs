using System.Net;
using System.Net.Http.Json;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class LeadSlaRulesEndpointTests
{
    [Fact]
    public async Task AdministratorCanUpsertNormalizedRuleAndReadAllRules()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateAuthenticatedClient("Administrador").Client;

        var first = await client.PutAsJsonAsync("/api/configuracion/sla-leads", new
        {
            SourceKey = "  Website|utm=campaign-1  ",
            OperationType = " venta ",
            FirstResponseHours = 4,
            InactivityDays = (int?)12,
            Enabled = true
        });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var created = (await first.Content.ReadFromJsonAsync<LeadSlaRuleDto>())!;
        Assert.Equal("WEBSITE", created.SourceKey);
        Assert.Equal("Venta", created.OperationType);
        Assert.Equal(4, created.FirstResponseHours);
        Assert.Equal(12, created.InactivityDays);
        Assert.True(created.Enabled);
        Assert.Equal(DateTimeKind.Utc, created.UpdatedAtUtc.Kind);

        var second = await client.PutAsJsonAsync("/api/configuracion/sla-leads", new
        {
            SourceKey = "website|PublicationReview:invalid",
            OperationType = "VENTA",
            FirstResponseHours = 8,
            InactivityDays = (int?)null,
            Enabled = false
        });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var response = await client.GetFromJsonAsync<LeadSlaRulesResponse>("/api/configuracion/sla-leads");
        Assert.NotNull(response);
        var rule = Assert.Single(response.Rules);
        Assert.Equal("WEBSITE", rule.SourceKey);
        Assert.Equal("Venta", rule.OperationType);
        Assert.Equal(8, rule.FirstResponseHours);
        Assert.Null(rule.InactivityDays);
        Assert.False(rule.Enabled);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        Assert.Equal(1, await db.LeadSlaRules.CountAsync());
    }

    [Theory]
    [InlineData("", "Venta", 1, null)]
    [InlineData("Website", "Compra", 1, null)]
    [InlineData("Website", "Venta", 0, null)]
    [InlineData("Website", "Venta", 721, null)]
    [InlineData("Website", "Venta", 1, 0)]
    [InlineData("Website", "Venta", 1, 91)]
    public async Task RejectsInvalidRuleValues(string source, string operation, int hours, int? days)
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateAuthenticatedClient("Administrador").Client;

        var response = await client.PutAsJsonAsync("/api/configuracion/sla-leads", new
        {
            SourceKey = source,
            OperationType = operation,
            FirstResponseHours = hours,
            InactivityDays = days,
            Enabled = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SupportsWildcardKeysAndAdminOnlyAccess()
    {
        using var factory = new TestWebApplicationFactory();
        using var admin = factory.CreateAuthenticatedClient("Administrador").Client;
        using var advisor = factory.CreateAuthenticatedClient("Asesor").Client;
        using var anonymous = factory.CreateClient();

        var put = await admin.PutAsJsonAsync("/api/configuracion/sla-leads", new
        {
            SourceKey = "*",
            OperationType = "*",
            FirstResponseHours = 1,
            InactivityDays = (int?)null,
            Enabled = true
        });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden,
            (await advisor.GetAsync("/api/configuracion/sla-leads")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await advisor.PutAsJsonAsync("/api/configuracion/sla-leads", new
            {
                SourceKey = "*", OperationType = "*", FirstResponseHours = 2,
                InactivityDays = (int?)null, Enabled = true
            })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/configuracion/sla-leads")).StatusCode);
    }

    [Fact]
    public async Task DatabaseRejectsDuplicateNormalizedKey()
    {
        using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        db.LeadSlaRules.Add(Enlyce.Domain.Entities.LeadSlaRule.Create(
            " portal |utm=first ", "Venta", 2, null, true));
        await db.SaveChangesAsync();

        db.LeadSlaRules.Add(Enlyce.Domain.Entities.LeadSlaRule.Create(
            "PORTAL", " venta ", 3, null, true));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private sealed record LeadSlaRuleDto(string SourceKey, string OperationType,
        int FirstResponseHours, int? InactivityDays, bool Enabled, DateTime UpdatedAtUtc);
    private sealed record LeadSlaRulesResponse(IReadOnlyList<LeadSlaRuleDto> Rules);
}
