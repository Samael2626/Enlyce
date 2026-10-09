using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class TaskListBulkEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task ListFiltersSearchByContactAndTitleAndPaginatesWithinAdvisorScope()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        var (otherClient, otherAdvisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        using (otherClient)
        {
            var now = DateTime.UtcNow;
            SeedTask(advisor.Id, "Visita Laureles", "Cliente Laureles", now.AddDays(1),
                CommercialTaskPriority.High);
            var secondMatch = SeedTask(advisor.Id, "Seguimiento", "Cliente Laureles", now.AddDays(2),
                CommercialTaskPriority.High);
            SeedTask(advisor.Id, "Visita Centro", "Cliente Centro", now.AddDays(2),
                CommercialTaskPriority.Normal);
            SeedTask(otherAdvisor.Id, "Visita Laureles", "Cliente externo", now.AddDays(2),
                CommercialTaskPriority.High);

            var from = Uri.EscapeDataString(now.ToString("O"));
            var to = Uri.EscapeDataString(now.AddDays(3).ToString("O"));
            using var httpResponse = await client.GetAsync(
                $"/api/tareas?query=laureles&priority=High&status=Pending&from={from}&to={to}&page=2&pageSize=1");
            Assert.True(httpResponse.IsSuccessStatusCode,
                await httpResponse.Content.ReadAsStringAsync());
            var response = await httpResponse.Content.ReadFromJsonAsync<TaskSearchResponse>();

            Assert.NotNull(response);
            Assert.Equal(2, response.Total);
            var item = Assert.Single(response.Items);
            Assert.Equal(secondMatch, item.Id);
            Assert.Equal(advisor.Id, item.AdvisorId);
        }
    }

    [Fact]
    public async Task BulkActionsReturnPerIdResultsAndAuditActorWithoutCrossAdvisorMutation()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        var (otherClient, otherAdvisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        using (otherClient)
        {
            var completeId = SeedTask(advisor.Id, "Completar", "Contacto A", DateTime.UtcNow.AddDays(1),
                CommercialTaskPriority.Normal);
            var cancelId = SeedTask(advisor.Id, "Cancelar", "Contacto B", DateTime.UtcNow.AddDays(1),
                CommercialTaskPriority.Normal);
            var hiddenId = SeedTask(otherAdvisor.Id, "Privada", "Contacto C", DateTime.UtcNow.AddDays(1),
                CommercialTaskPriority.Normal);
            var missingId = Guid.NewGuid();

            var completed = await client.PostAsJsonAsync("/api/tareas/bulk/completar", new
            {
                ids = new[] { completeId, hiddenId, missingId }
            });
            Assert.Equal(HttpStatusCode.OK, completed.StatusCode);
            var completeResponse = await completed.Content.ReadFromJsonAsync<BulkTaskResponse>();
            Assert.NotNull(completeResponse);
            Assert.Collection(completeResponse.Results,
                result => Assert.Equal((completeId, "Completed"), (result.Id, result.Result)),
                result => Assert.Equal((hiddenId, "NotFound"), (result.Id, result.Result)),
                result => Assert.Equal((missingId, "NotFound"), (result.Id, result.Result)));

            var cancelled = await client.PostAsJsonAsync("/api/tareas/bulk/cancelar", new
            {
                ids = new[] { cancelId }
            });
            Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);
            var cancelResponse = await cancelled.Content.ReadFromJsonAsync<BulkTaskResponse>();
            Assert.NotNull(cancelResponse);
            Assert.Equal("Cancelled", Assert.Single(cancelResponse.Results).Result);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            Assert.Equal(CommercialTaskStatus.Completed,
                (await db.CommercialTasks.SingleAsync(task => task.Id == completeId)).Status);
            Assert.Equal(CommercialTaskStatus.Cancelled,
                (await db.CommercialTasks.SingleAsync(task => task.Id == cancelId)).Status);
            Assert.Equal(CommercialTaskStatus.Pending,
                (await db.CommercialTasks.SingleAsync(task => task.Id == hiddenId)).Status);

            var events = await db.CommercialTaskEvents
                .Where(taskEvent => taskEvent.TaskId == completeId || taskEvent.TaskId == cancelId)
                .ToListAsync();
            Assert.Contains(events, item => item.TaskId == completeId && item.ActorId == advisor.Id &&
                item.Action == "Completed" && item.Comment == "Accion masiva");
            Assert.Contains(events, item => item.TaskId == cancelId && item.ActorId == advisor.Id &&
                item.Action == "Cancelled" && item.Comment == "Accion masiva");
        }
    }

    [Fact]
    public async Task BulkActionRejectsMoreThanOneHundredIds()
    {
        var (client, _) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.PostAsJsonAsync("/api/tareas/bulk/completar", new
            {
                ids = Enumerable.Range(0, 101).Select(_ => Guid.NewGuid()).ToArray()
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    private Guid SeedTask(Guid advisorId, string title, string contactName, DateTime dueAt,
        CommercialTaskPriority priority)
    {
        var contact = Contact.Create(contactName, Email.Create($"{Guid.NewGuid():N}@test.com"));
        var task = CommercialTask.Create(contact.Id, null, advisorId, CommercialTaskType.Call,
            title, null, dueAt, null, priority);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        db.Contacts.Add(contact);
        db.CommercialTasks.Add(task);
        db.CommercialTaskEvents.Add(CommercialTaskEvent.Create(task.Id, advisorId, "Created"));
        db.SaveChanges();
        return task.Id;
    }

    private sealed record TaskSearchResponse(int Total, TaskItem[] Items);
    private sealed record TaskItem(Guid Id, Guid AdvisorId, string Title, string Priority, string Status);
    private sealed record BulkTaskResponse(BulkTaskResult[] Results);
    private sealed record BulkTaskResult(Guid Id, string Result);
}
