using Enlyce.Application.Billing;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using NSubstitute;

namespace Enlyce.Application.Tests.Billing;

public sealed class CreateCheckoutSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_CalculatesPriceAndPersistsBeforeReturningCheckout()
    {
        var orders = Substitute.For<IPaymentOrderRepository>();
        var gateway = Substitute.For<IPaymentCheckoutGateway>();
        PaymentOrder? captured = null;
        orders.AddAsync(Arg.Do<PaymentOrder>(order => captured = order), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        gateway.CreateCheckout(Arg.Any<PaymentOrder>())
            .Returns(call => CreateCheckout(call.Arg<PaymentOrder>()));
        var handler = new CreateCheckoutSessionHandler(orders, gateway, TimeProvider.System);

        var result = await handler.HandleAsync(new CreateCheckoutSessionCommand(
            "L&C", "901951636-2", "billing@lyc.com", 1, true, false, true, false));

        var order = Assert.IsType<PaymentOrder>(captured);
        Assert.Equal(29_300_000, order.AmountInCents);
        Assert.Equal(order.Reference, result.Checkout.Reference);
        await orders.Received(1).AddAsync(order, Arg.Any<CancellationToken>());
    }

    private static PaymentCheckoutData CreateCheckout(PaymentOrder order) => new(
        "https://checkout.wompi.co/p/",
        "pub_test_key",
        order.Currency,
        order.AmountInCents,
        order.Reference,
        "signature",
        "http://localhost:3000/pagos/respuesta",
        order.CustomerEmail);
}
