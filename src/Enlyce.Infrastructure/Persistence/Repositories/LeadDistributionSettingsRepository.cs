using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class LeadDistributionSettingsRepository(EnlyceDbContext context)
    : ILeadDistributionSettingsRepository
{
    public async Task<LeadDistributionRule> GetRuleAsync(CancellationToken ct = default)
    {
        var value = await context.LeadDistributionSettings
            .AsNoTracking()
            .Where(settings => settings.Id == 1)
            .Select(settings => settings.Rule)
            .SingleOrDefaultAsync(ct);
        return value is null || !Enum.TryParse<LeadDistributionRule>(value, out var rule)
            ? LeadDistributionRule.LeastOpenLeads
            : rule;
    }

    public async Task SetRuleAsync(LeadDistributionRule rule, CancellationToken ct = default)
    {
        var settings = await context.LeadDistributionSettings.FindAsync([1], ct);
        if (settings is null)
        {
            settings = new LeadDistributionSettings { Id = 1, Rule = rule.ToString() };
            context.LeadDistributionSettings.Add(settings);
        }
        else
        {
            settings.Rule = rule.ToString();
        }

        await context.SaveChangesAsync(ct);
    }
}
