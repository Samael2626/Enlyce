using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ILeadSlaRuleRepository
{
    Task<IReadOnlyList<LeadSlaRule>> GetAllAsync(CancellationToken ct = default);
    Task<LeadSlaRule> UpsertAsync(LeadSlaRule rule, CancellationToken ct = default);
}
