using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public sealed class CommercialTaskTests
{
    [Fact]
    public void Create_StoresPendingTaskAndNormalizedText()
    {
        var task = CommercialTask.Create(Guid.NewGuid(), null, Guid.NewGuid(),
            CommercialTaskType.Call, "  Confirmar visita  ", "  Llamar al cliente  ",
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(1), CommercialTaskPriority.High);

        Assert.Equal("Confirmar visita", task.Title);
        Assert.Equal("Llamar al cliente", task.Description);
        Assert.Equal(CommercialTaskStatus.Pending, task.Status);
    }

    [Fact]
    public void Reschedule_RejectsReminderAfterDueDate()
    {
        var task = CommercialTask.Create(Guid.NewGuid(), null, Guid.NewGuid(),
            CommercialTaskType.FollowUp, "Seguimiento", null,
            DateTime.UtcNow.AddHours(2), null, CommercialTaskPriority.Normal);

        Assert.Throws<DomainError>(() => task.Reschedule(
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(3)));
    }

    [Fact]
    public void Complete_PreventsFurtherRescheduling()
    {
        var task = CommercialTask.Create(Guid.NewGuid(), null, Guid.NewGuid(),
            CommercialTaskType.Call, "Llamar", null,
            DateTime.UtcNow.AddHours(2), null, CommercialTaskPriority.Normal);
        task.Complete();

        Assert.Throws<DomainError>(() => task.Reschedule(DateTime.UtcNow.AddHours(3), null));
    }
}
