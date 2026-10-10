using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class LeadSlaRuleRepository(EnlyceDbContext context) : ILeadSlaRuleRepository
{
    public async Task<IReadOnlyList<LeadSlaRule>> GetAllAsync(CancellationToken ct = default) =>
        await context.LeadSlaRules.AsNoTracking()
            .OrderBy(rule => rule.SourceKey)
            .ThenBy(rule => rule.OperationType)
            .ToListAsync(ct);

    public async Task<LeadSlaRule> UpsertAsync(LeadSlaRule rule, CancellationToken ct = default)
    {
        var existing = await context.LeadSlaRules.SingleOrDefaultAsync(item =>
            item.SourceKey == rule.SourceKey && item.OperationType == rule.OperationType, ct);
        if (existing is null)
        {
            context.LeadSlaRules.Add(rule);
            existing = rule;
        }
        else
        {
            existing.Update(rule.FirstResponseMinutes, rule.InactivityDays, rule.Enabled);
        }

        await context.SaveChangesAsync(ct);
        return existing;
    }
}
