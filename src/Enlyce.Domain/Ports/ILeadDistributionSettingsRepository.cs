using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ILeadDistributionSettingsRepository
{
    Task<LeadDistributionRule> GetRuleAsync(CancellationToken ct = default);
    Task SetRuleAsync(LeadDistributionRule rule, CancellationToken ct = default);
}
