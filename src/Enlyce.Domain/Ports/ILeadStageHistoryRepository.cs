using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ILeadStageHistoryRepository
{
    Task<IReadOnlyList<LeadStageHistory>> GetByLeadIdAsync(Guid leadId, CancellationToken ct = default);
    Task<bool> MoveStageAsync(Guid leadId, string newStage, Guid actorId, string reason, CancellationToken ct = default);
}
