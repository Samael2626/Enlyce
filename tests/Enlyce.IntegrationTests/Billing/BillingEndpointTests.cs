using System.Net;
using System.Net.Http.Json;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests.Billing;

public sealed class BillingEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Pricing_Administrator_ReturnsServerCatalog()
    {
        var (client, _) = factory.CreateAuthenticatedClient();
        using (client)
        {
            var response = await client.GetAsync("/api/billing/pricing");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<BillingPricingResponse>();
            Assert.NotNull(body);
            Assert.Equal(8_900_000, body.BasePlanInCents);
            Assert.Equal(5, body.MaximumAdditionalAdvisors);
        }
    }

    [Fact]
    public async Task Checkout_WithoutWompiSecrets_FailsClosedAndDoesNotPersist()
    {
        var (client, _) = factory.CreateAuthenticatedClient();
        using (client)
        {
            var response = await client.PostAsJsonAsync("/api/billing/checkout-sessions", new
            {
                CompanyName = "L&C",
                TaxId = "901951636-2",
                CustomerEmail = "billing@lyc.com",
                AdditionalAdvisors = 0,
                IncludeSetup = true
            });

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        Assert.False(await db.PaymentOrders.AnyAsync());
    }

    private sealed record BillingPricingResponse(
        long BasePlanInCents,
        long AdditionalAdvisorInCents,
        long SetupInCents,
        long WhatsAppInCents,
        long PortalInCents,
        long AdvancedReportsInCents,
        int MaximumAdditionalAdvisors);
}
