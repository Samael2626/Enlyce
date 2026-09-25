using Enlyce.Domain.Billing;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Billing;

public sealed class SaasPricingTests
{
    [Fact]
    public void CalculateAmountInCents_BasePlan_ReturnsEightyNineThousandPesos()
    {
        var amount = SaasPricing.CalculateAmountInCents(new SubscriptionSelection(0, false, false, false, false));

        Assert.Equal(8_900_000, amount);
    }

    [Fact]
    public void CalculateAmountInCents_AllOptions_ReturnsServerPrice()
    {
        var amount = SaasPricing.CalculateAmountInCents(new SubscriptionSelection(1, true, true, true, true));

        Assert.Equal(36_300_000, amount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void CalculateAmountInCents_InvalidAdvisorCount_RejectsSelection(int count)
    {
        Assert.Throws<DomainError>(() =>
            SaasPricing.CalculateAmountInCents(new SubscriptionSelection(count, false, false, false, false)));
    }
}
