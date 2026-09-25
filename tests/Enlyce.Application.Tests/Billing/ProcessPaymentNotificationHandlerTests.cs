using Enlyce.Application.Billing;
using Enlyce.Domain.Billing;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using NSubstitute;

namespace Enlyce.Application.Tests.Billing;

public sealed class ProcessPaymentNotificationHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidNotification_UpdatesOrder()
    {
        var order = CreateOrder();
        var orders = Substitute.For<IPaymentOrderRepository>();
        orders.GetByReferenceAsync(order.Reference, Arg.Any<CancellationToken>()).Returns(order);
        var handler = new ProcessPaymentNotificationHandler(orders, TimeProvider.System);

        var processed = await handler.HandleAsync(new VerifiedPaymentNotification(
            "wompi-1", order.Reference, order.AmountInCents, "COP", PaymentOrderStatus.Approved));

        Assert.True(processed);
        Assert.Equal(PaymentOrderStatus.Approved, order.Status);
        await orders.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TamperedAmount_RejectsNotification()
    {
        var order = CreateOrder();
        var orders = Substitute.For<IPaymentOrderRepository>();
        orders.GetByReferenceAsync(order.Reference, Arg.Any<CancellationToken>()).Returns(order);
        var handler = new ProcessPaymentNotificationHandler(orders, TimeProvider.System);

        await Assert.ThrowsAsync<DomainError>(() => handler.HandleAsync(new VerifiedPaymentNotification(
            "wompi-1", order.Reference, order.AmountInCents + 1, "COP", PaymentOrderStatus.Approved)));

        await orders.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static PaymentOrder CreateOrder() => PaymentOrder.Create(
        "L&C",
        "901951636-2",
        "billing@lyc.com",
        new SubscriptionSelection(0, false, false, false, false),
        DateTime.UtcNow);
}
