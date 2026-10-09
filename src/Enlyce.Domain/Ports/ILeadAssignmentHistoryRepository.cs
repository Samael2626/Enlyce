using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ILeadAssignmentHistoryRepository
{
    Task<IReadOnlyList<LeadAssignmentHistory>> GetByLeadIdAsync(Guid leadId, CancellationToken ct = default);
    Task<bool> ChangeAssignmentAsync(
        Lead lead,
        Guid newAdvisorId,
        Guid actorId,
        string reason,
        LeadAssignmentSource source,
        CancellationToken ct = default);
}
