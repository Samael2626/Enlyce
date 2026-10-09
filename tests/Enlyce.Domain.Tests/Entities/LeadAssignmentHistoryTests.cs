using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public sealed class LeadAssignmentHistoryTests
{
    [Fact]
    public void Create_StoresTrimmedReasonAndUtcTimestamp()
    {
        var changedAt = DateTime.UtcNow;
        var item = LeadAssignmentHistory.Create(Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
            "  Cobertura de zona  ", LeadAssignmentSource.ManualAssignment, changedAt);

        Assert.Equal("Cobertura de zona", item.Reason);
        Assert.Equal(changedAt, item.ChangedAt);
        Assert.Equal(LeadAssignmentSource.ManualAssignment, item.Source);
    }

    [Fact]
    public void Create_RejectsManualChangeWithoutActor()
    {
        Assert.Throws<DomainError>(() => LeadAssignmentHistory.Create(
            Guid.NewGuid(), null, Guid.NewGuid(), null, "Asignacion manual",
            LeadAssignmentSource.ManualAssignment));
    }

    [Fact]
    public void Create_RejectsReassignmentWithoutPreviousAdvisor()
    {
        Assert.Throws<DomainError>(() => LeadAssignmentHistory.Create(
            Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), "Cambio",
            LeadAssignmentSource.ManualReassignment));
    }
}
