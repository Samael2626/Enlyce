using Enlyce.Domain.Billing;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public sealed class PaymentOrderTests
{
    private static readonly DateTime Now = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_CalculatesAmountAndCreatesUnpredictableReference()
    {
        var order = CreateOrder(new SubscriptionSelection(1, true, false, true, false));

        Assert.Equal(29_300_000, order.AmountInCents);
        Assert.StartsWith("ENL-", order.Reference);
        Assert.Equal(36, order.Reference.Length);
        Assert.Equal(PaymentOrderStatus.Pending, order.Status);
        Assert.Equal("billing@lyc.com", order.CustomerEmail);
    }

    [Fact]
    public void ApplyProviderUpdate_DuplicateEvent_IsIdempotent()
    {
        var order = CreateOrder(default);

        order.ApplyProviderUpdate("wompi-1", PaymentOrderStatus.Approved, Now.AddMinutes(1));
        order.ApplyProviderUpdate("wompi-1", PaymentOrderStatus.Approved, Now.AddMinutes(2));

        Assert.Equal(PaymentOrderStatus.Approved, order.Status);
        Assert.Equal(Now.AddMinutes(2), order.UpdatedAt);
    }

    [Fact]
    public void ApplyProviderUpdate_ApprovedToDeclined_RejectsRegression()
    {
        var order = CreateOrder(default);
        order.ApplyProviderUpdate("wompi-1", PaymentOrderStatus.Approved, Now.AddMinutes(1));

        Assert.Throws<DomainError>(() =>
            order.ApplyProviderUpdate("wompi-1", PaymentOrderStatus.Declined, Now.AddMinutes(2)));
    }

    [Fact]
    public void ApplyProviderUpdate_DifferentTransaction_RejectsReplacement()
    {
        var order = CreateOrder(default);
        order.ApplyProviderUpdate("wompi-1", PaymentOrderStatus.Pending, Now.AddMinutes(1));

        Assert.Throws<DomainError>(() =>
            order.ApplyProviderUpdate("wompi-2", PaymentOrderStatus.Approved, Now.AddMinutes(2)));
    }

    private static PaymentOrder CreateOrder(SubscriptionSelection selection) =>
        PaymentOrder.Create("L&C", "901951636-2", "BILLING@LYC.COM", selection, Now);
}
