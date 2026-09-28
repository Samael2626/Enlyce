using System.Net;
using System.Net.Http.Json;
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

    private sealed record FunnelResponse(IReadOnlyList<FunnelStep> Steps);
    private sealed record FunnelStep(string Event, int UniqueSessions, int TotalEvents, decimal RateFromVisits);
}
