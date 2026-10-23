using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public sealed class RealizedDealTests
{
    private static readonly DateTime RecordedAt = new(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);

    private static RealizedDeal CreateSampleDeal() =>
        RealizedDeal.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            RealizedDealOperation.Sale,
            150_000_000m,
            4_500_000m,
            "COP",
            new DateOnly(2026, 10, 19),
            Guid.NewGuid(),
            RecordedAt);

    [Fact]
    public void CanEditAt_WithinThreeDays_ReturnsTrue()
    {
        var deal = CreateSampleDeal();

        Assert.True(deal.CanEditAt(RecordedAt));
        Assert.True(deal.CanEditAt(RecordedAt.AddDays(2).AddHours(23)));
    }

    [Fact]
    public void CanEditAt_AfterThreeDays_ReturnsFalse()
    {
        var deal = CreateSampleDeal();

        Assert.False(deal.CanEditAt(RecordedAt.AddDays(3)));
        Assert.False(deal.CanEditAt(RecordedAt.AddDays(4)));
    }

    [Fact]
    public void UpdateFinancials_WithinWindow_UpdatesProperties()
    {
        var deal = CreateSampleDeal();
        var updateTime = RecordedAt.AddDays(1);
        var newClosedOn = new DateOnly(2026, 10, 20);

        deal.UpdateFinancials(200_000_000.555m, 6_000_000.111m, "cop", newClosedOn, updateTime);

        Assert.Equal(200_000_000.56m, deal.GrossAmount);
        Assert.Equal(6_000_000.11m, deal.CommissionAmount);
        Assert.Equal("COP", deal.Currency);
        Assert.Equal(newClosedOn, deal.ClosedOn);
    }

    [Fact]
    public void UpdateFinancials_WhenExpired_ThrowsDomainError()
    {
        var deal = CreateSampleDeal();
        var expiredTime = RecordedAt.AddDays(3).AddMinutes(1);

        var ex = Assert.Throws<DomainError>(() =>
            deal.UpdateFinancials(180_000_000m, 5_000_000m, "COP", new DateOnly(2026, 10, 20), expiredTime));

        Assert.Contains("tres dias", ex.Message);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-100, 100)]
    [InlineData(100, -10)]
    public void UpdateFinancials_InvalidAmounts_ThrowsDomainError(decimal gross, decimal commission)
    {
        var deal = CreateSampleDeal();

        Assert.Throws<DomainError>(() =>
            deal.UpdateFinancials(gross, commission, "COP", new DateOnly(2026, 10, 20), RecordedAt.AddHours(1)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("CO")]
    [InlineData("CO1")]
    [InlineData("PESOS")]
    public void UpdateFinancials_InvalidCurrency_ThrowsDomainError(string currency)
    {
        var deal = CreateSampleDeal();

        Assert.Throws<DomainError>(() =>
            deal.UpdateFinancials(100_000m, 3_000m, currency, new DateOnly(2026, 10, 20), RecordedAt.AddHours(1)));
    }
}
