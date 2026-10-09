using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public sealed class LeadStageHistoryTests
{
    [Fact]
    public void CreateStoresTrimmedReasonAndUtcTimestamp()
    {
        var changedAt = DateTime.UtcNow;
        var history = LeadStageHistory.Create(Guid.NewGuid(), "Lead nuevo", "Contactado",
            Guid.NewGuid(), "  Primera llamada  ", changedAt);

        Assert.Equal("Lead nuevo", history.PreviousStage);
        Assert.Equal("Contactado", history.NewStage);
        Assert.Equal("Primera llamada", history.Reason);
        Assert.Equal(changedAt, history.ChangedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateRejectsBlankReason(string reason)
    {
        Assert.Throws<DomainError>(() => LeadStageHistory.Create(
            Guid.NewGuid(), "Lead nuevo", "Contactado", Guid.NewGuid(), reason));
    }

    [Fact]
    public void CreateRejectsIdenticalStages()
    {
        Assert.Throws<DomainError>(() => LeadStageHistory.Create(
            Guid.NewGuid(), "Contactado", "Contactado", Guid.NewGuid(), "Correccion"));
    }
}
