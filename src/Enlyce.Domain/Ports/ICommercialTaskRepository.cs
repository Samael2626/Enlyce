using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ICommercialTaskRepository
{
    Task<CommercialTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CommercialTask>> GetForAdvisorAsync(Guid? advisorId, DateTime? from, DateTime? to, Guid? contactId, CancellationToken ct = default);
    Task<IReadOnlyList<CommercialTask>> GetPendingAlertsAsync(Guid? advisorId, DateTime through, CancellationToken ct = default);
    Task<IReadOnlyList<CommercialTask>> GetByContactIdAsync(Guid contactId, CancellationToken ct = default);
    Task<IReadOnlyList<CommercialTaskEvent>> GetEventsAsync(Guid taskId, CancellationToken ct = default);
    Task AddAsync(CommercialTask task, CommercialTaskEvent taskEvent, CancellationToken ct = default);
    Task SaveAsync(CommercialTask task, CommercialTaskEvent taskEvent, CancellationToken ct = default);
    Task AddEventAsync(CommercialTaskEvent taskEvent, CancellationToken ct = default);
}
