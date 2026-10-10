using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.Billing;
using Enlyce.Domain.Billing;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests.Billing;

public sealed class PaymentOrderStatusHistoryEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task OrderStatus_ReturnsProviderTransitionHistoryToAdministrator()
    {
        var order = PaymentOrder.Create(
            "L&C", "901951636-2", "billing@lyc.com",
            new SubscriptionSelection(0, false, false, false, false),
            DateTime.UtcNow);
        order.ApplyProviderUpdate("wompi-test-2", PaymentOrderStatus.Approved, DateTime.UtcNow.AddMinutes(1));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.PaymentOrders.Add(order);
            await db.SaveChangesAsync();
        }

        var (client, _) = factory.CreateAuthenticatedClient();
        using (client)
        {
            var response = await client.GetAsync($"/api/billing/orders/{order.Reference}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<PaymentOrderStatusResponse>();
            Assert.NotNull(body);
            Assert.Equal("Approved", body.Status);
            var change = Assert.Single(body.StatusHistory);
            Assert.Equal("Pending", change.PreviousStatus);
            Assert.Equal("Approved", change.NewStatus);
        }
    }
}
