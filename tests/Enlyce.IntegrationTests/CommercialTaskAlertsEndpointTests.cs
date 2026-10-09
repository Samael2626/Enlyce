using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class CommercialTaskAlertsEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task ListsTaskWhenReminderIsBeforeCutoffEvenIfDueDateIsAfter()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddDays(3);
            var reminderTask = SeedTask(advisor.Id, now.AddDays(4), now.AddDays(1));
            var completedTask = SeedTask(advisor.Id, now.AddDays(2), now.AddDays(1), CommercialTaskStatus.Completed);
            var cancelledTask = SeedTask(advisor.Id, now.AddDays(2), now.AddDays(1), CommercialTaskStatus.Cancelled);

            var tasks = await client.GetFromJsonAsync<List<TaskAlertDto>>($"/api/tareas/alertas?through={cutoff:O}");

            Assert.NotNull(tasks);
            Assert.Contains(tasks, task => task.Id == reminderTask);
            Assert.DoesNotContain(tasks, task => task.Id == completedTask);
            Assert.DoesNotContain(tasks, task => task.Id == cancelledTask);
        }
    }

    [Fact]
    public async Task AdvisorSeesOwnAlertsAndAdministratorSeesAll()
    {
        var (advisorClient, advisor) = factory.CreateAuthenticatedClient("Asesor");
        var (otherClient, otherAdvisor) = factory.CreateAuthenticatedClient("Asesor");
        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        using (advisorClient)
        using (otherClient)
        using (adminClient)
        {
            var ownTask = SeedTask(advisor.Id, DateTime.UtcNow.AddDays(1), null);
            var otherTask = SeedTask(otherAdvisor.Id, DateTime.UtcNow.AddDays(1), null);
            var cutoff = DateTime.UtcNow.AddDays(3).ToString("O");

            var advisorTasks = await advisorClient.GetFromJsonAsync<List<TaskAlertDto>>($"/api/tareas/alertas?through={cutoff}");
            var adminTasks = await adminClient.GetFromJsonAsync<List<TaskAlertDto>>($"/api/tareas/alertas?through={cutoff}");

            Assert.NotNull(advisorTasks);
            Assert.Contains(advisorTasks, task => task.Id == ownTask);
            Assert.DoesNotContain(advisorTasks, task => task.Id == otherTask);
            Assert.NotNull(adminTasks);
            Assert.Contains(adminTasks, task => task.Id == ownTask);
            Assert.Contains(adminTasks, task => task.Id == otherTask);
        }
    }

    private Guid SeedTask(Guid advisorId, DateTime dueAt, DateTime? reminderAt,
        CommercialTaskStatus status = CommercialTaskStatus.Pending)
    {
        var contact = Contact.Create("Alert Contact", Email.Create($"{Guid.NewGuid():N}@test.com"));
        var task = CommercialTask.Create(contact.Id, null, advisorId, CommercialTaskType.Call,
            "Alert test task", null, dueAt, reminderAt, CommercialTaskPriority.Normal);
        if (status == CommercialTaskStatus.Completed)
            task.Complete();
        else if (status == CommercialTaskStatus.Cancelled)
            task.Cancel();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        db.Contacts.Add(contact);
        db.CommercialTasks.Add(task);
        db.SaveChanges();
        return task.Id;
    }

    private sealed record TaskAlertDto(Guid Id);
}
