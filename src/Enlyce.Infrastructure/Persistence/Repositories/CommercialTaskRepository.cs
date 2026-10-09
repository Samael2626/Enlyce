using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class CommercialTaskRepository(EnlyceDbContext context) : ICommercialTaskRepository
{
    public Task<CommercialTask?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.CommercialTasks.SingleOrDefaultAsync(task => task.Id == id, ct);

    public async Task<IReadOnlyList<CommercialTask>> GetForAdvisorAsync(
        Guid? advisorId, DateTime? from, DateTime? to, Guid? contactId, CancellationToken ct = default)
    {
        var query = context.CommercialTasks.AsNoTracking().AsQueryable();
        if (advisorId.HasValue)
            query = query.Where(task => task.AdvisorId == advisorId.Value);
        if (from.HasValue)
            query = query.Where(task => task.DueAt >= from.Value);
        if (to.HasValue)
            query = query.Where(task => task.DueAt < to.Value);
        if (contactId.HasValue)
            query = query.Where(task => task.ContactId == contactId.Value);

        return await query.OrderBy(task => task.DueAt).ThenBy(task => task.Priority)
            .ToListAsync(ct);
    }

    public async Task<CommercialTaskSearchPage> SearchAsync(
        Guid? advisorId, string? queryText, CommercialTaskStatus? status,
        CommercialTaskPriority? priority, DateTime? from, DateTime? to, Guid? contactId,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.CommercialTasks.AsNoTracking().AsQueryable();
        if (advisorId.HasValue)
            query = query.Where(task => task.AdvisorId == advisorId.Value);
        if (from.HasValue)
            query = query.Where(task => task.DueAt >= from.Value);
        if (to.HasValue)
            query = query.Where(task => task.DueAt < to.Value);
        if (contactId.HasValue)
            query = query.Where(task => task.ContactId == contactId.Value);
        if (status.HasValue)
            query = query.Where(task => task.Status == status.Value);
        if (priority.HasValue)
            query = query.Where(task => task.Priority == priority.Value);
        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var search = queryText.Trim().ToLower();
            query = query.Where(task => task.Title.ToLower().Contains(search) ||
                context.Contacts.Any(contact => contact.Id == task.ContactId &&
                    (contact.Name.ToLower().Contains(search) || contact.Email.ToLower().Contains(search))));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(task => task.DueAt).ThenBy(task => task.Priority)
            .ThenBy(task => task.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new CommercialTaskSearchPage(total, items);
    }

    public async Task<IReadOnlyList<CommercialTask>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, Guid? advisorId, CancellationToken ct = default)
    {
        var query = context.CommercialTasks.Where(task => ids.Contains(task.Id));
        if (advisorId.HasValue)
            query = query.Where(task => task.AdvisorId == advisorId.Value);
        return await query.ToListAsync(ct);
    }

    public async Task SaveManyAsync(
        IReadOnlyCollection<CommercialTask> tasks,
        IReadOnlyCollection<CommercialTaskEvent> events, CancellationToken ct = default)
    {
        context.CommercialTasks.UpdateRange(tasks);
        context.CommercialTaskEvents.AddRange(events);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CommercialTask>> GetPendingAlertsAsync(
        Guid? advisorId, DateTime through, CancellationToken ct = default)
    {
        var query = context.CommercialTasks.AsNoTracking()
            .Where(task => task.Status == CommercialTaskStatus.Pending &&
                (task.DueAt <= through || (task.ReminderAt.HasValue && task.ReminderAt.Value <= through)));
        if (advisorId.HasValue)
            query = query.Where(task => task.AdvisorId == advisorId.Value);

        return await query.OrderBy(task => task.DueAt).ThenBy(task => task.Priority).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CommercialTask>> GetByContactIdAsync(Guid contactId, CancellationToken ct = default) =>
        await context.CommercialTasks.AsNoTracking().Where(task => task.ContactId == contactId)
            .OrderByDescending(task => task.CreatedAt).ToListAsync(ct);

    public async Task<IReadOnlyList<CommercialTaskEvent>> GetEventsAsync(Guid taskId, CancellationToken ct = default) =>
        await context.CommercialTaskEvents.AsNoTracking()
            .Where(taskEvent => taskEvent.TaskId == taskId)
            .OrderBy(taskEvent => taskEvent.OccurredAt)
            .ToListAsync(ct);

    public async Task AddAsync(CommercialTask task, CommercialTaskEvent taskEvent, CancellationToken ct = default)
    {
        context.CommercialTasks.Add(task);
        context.CommercialTaskEvents.Add(taskEvent);
        await context.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CommercialTask task, CommercialTaskEvent taskEvent, CancellationToken ct = default)
    {
        context.CommercialTasks.Update(task);
        context.CommercialTaskEvents.Add(taskEvent);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddEventAsync(CommercialTaskEvent taskEvent, CancellationToken ct = default)
    {
        context.CommercialTaskEvents.Add(taskEvent);
        await context.SaveChangesAsync(ct);
    }
}
