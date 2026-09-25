using Enlyce.Application.Abstractions;
using Enlyce.Domain.Billing;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Billing;

public sealed record CreateCheckoutSessionCommand(
    string CompanyName,
    string TaxId,
    string CustomerEmail,
    int AdditionalAdvisors,
    bool IncludeSetup,
    bool IncludeWhatsApp,
    bool IncludePortal,
    bool IncludeAdvancedReports);

public sealed record CreateCheckoutSessionResponse(
    Guid OrderId,
    PaymentCheckoutData Checkout);

public sealed class CreateCheckoutSessionHandler(
    IPaymentOrderRepository orders,
    IPaymentCheckoutGateway gateway,
    TimeProvider timeProvider)
    : ICommandHandler<CreateCheckoutSessionCommand, CreateCheckoutSessionResponse>
{
    public async Task<CreateCheckoutSessionResponse> HandleAsync(
        CreateCheckoutSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var selection = new SubscriptionSelection(
            command.AdditionalAdvisors,
            command.IncludeSetup,
            command.IncludeWhatsApp,
            command.IncludePortal,
            command.IncludeAdvancedReports);

        var order = PaymentOrder.Create(
            command.CompanyName,
            command.TaxId,
            command.CustomerEmail,
            selection,
            timeProvider.GetUtcNow().UtcDateTime);

        var checkout = gateway.CreateCheckout(order);
        await orders.AddAsync(order, cancellationToken);
        return new CreateCheckoutSessionResponse(order.Id, checkout);
    }
}
