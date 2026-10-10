namespace Enlyce.Domain.Entities;

public sealed class PaymentOrderStatusChange
{
    public Guid Id { get; private set; }
    public Guid PaymentOrderId { get; private set; }
    public PaymentOrderStatus PreviousStatus { get; private set; }
    public PaymentOrderStatus NewStatus { get; private set; }
    public string ProviderTransactionId { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    private PaymentOrderStatusChange() { }

    internal static PaymentOrderStatusChange Create(
        Guid paymentOrderId,
        PaymentOrderStatus previousStatus,
        PaymentOrderStatus newStatus,
        string providerTransactionId,
        DateTime occurredAt) => new()
    {
        Id = Guid.NewGuid(),
        PaymentOrderId = paymentOrderId,
        PreviousStatus = previousStatus,
        NewStatus = newStatus,
        ProviderTransactionId = providerTransactionId,
        OccurredAt = occurredAt
    };
}
