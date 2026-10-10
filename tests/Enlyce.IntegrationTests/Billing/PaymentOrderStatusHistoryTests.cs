using Enlyce.Domain.Billing;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Enlyce.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.IntegrationTests.Billing;

public sealed class PaymentOrderStatusHistoryTests
{
    [Fact]
    public async Task StatusChanges_PersistWithOrderAndIgnoreWebhookReplay()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EnlyceDbContext>()
            .UseSqlite(connection)
            .Options;
        await using (var db = new EnlyceDbContext(options))
            await db.Database.EnsureCreatedAsync();

        var order = PaymentOrder.Create(
            "L&C", "901951636-2", "billing@lyc.com",
            new SubscriptionSelection(0, false, false, false, false),
            DateTime.UtcNow);

        await using (var db = new EnlyceDbContext(options))
        {
            var repository = new PaymentOrderRepository(db);
            await repository.AddAsync(order);
            order.ApplyProviderUpdate("wompi-test-1", PaymentOrderStatus.Approved, DateTime.UtcNow);
            await repository.SaveChangesAsync();
            order.ApplyProviderUpdate("wompi-test-1", PaymentOrderStatus.Approved, DateTime.UtcNow.AddMinutes(1));
            await repository.SaveChangesAsync();
        }

        await using (var db = new EnlyceDbContext(options))
        {
            var stored = await db.PaymentOrders
                .Include(item => item.StatusHistory)
                .SingleAsync(item => item.Reference == order.Reference);

            Assert.Equal(PaymentOrderStatus.Approved, stored.Status);
            var change = Assert.Single(stored.StatusHistory);
            Assert.Equal(PaymentOrderStatus.Pending, change.PreviousStatus);
            Assert.Equal(PaymentOrderStatus.Approved, change.NewStatus);
            Assert.Equal("wompi-test-1", change.ProviderTransactionId);
        }
    }
}
