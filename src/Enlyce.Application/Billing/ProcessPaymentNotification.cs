using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Billing;

public sealed class ProcessPaymentNotificationHandler(
    IPaymentOrderRepository orders,
    TimeProvider timeProvider)
{
    public async Task<bool> HandleAsync(
        VerifiedPaymentNotification notification,
        CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByReferenceAsync(notification.Reference, cancellationToken);
        if (order is null)
            return false;

        if (order.AmountInCents != notification.AmountInCents ||
            !string.Equals(order.Currency, notification.Currency, StringComparison.Ordinal))
            throw new DomainError("El monto o la moneda del evento no coincide con la orden.");

        order.ApplyProviderUpdate(
            notification.ProviderTransactionId,
            notification.Status,
            timeProvider.GetUtcNow().UtcDateTime);
        await orders.SaveChangesAsync(cancellationToken);
        return true;
    }
}
