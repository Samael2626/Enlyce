using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class LeadStageHistoryRepository(EnlyceDbContext context) : ILeadStageHistoryRepository
{
    public async Task<IReadOnlyList<LeadStageHistory>> GetByLeadIdAsync(Guid leadId, CancellationToken ct = default) =>
        await context.LeadStageHistory.AsNoTracking()
            .Where(item => item.LeadId == leadId)
            .OrderBy(item => item.ChangedAt)
            .ThenBy(item => item.Id)
            .ToListAsync(ct);

    public async Task<bool> MoveStageAsync(
        Guid leadId, string newStage, Guid actorId, string reason, CancellationToken ct = default)
    {
        var lead = await context.Leads.SingleOrDefaultAsync(item => item.Id == leadId, ct);
        if (lead is null) return false;

        var previousStage = lead.EtapaPipeline;
        lead.MoverEtapa(newStage);
        if (string.Equals(previousStage, lead.EtapaPipeline, StringComparison.Ordinal))
            return true;

        context.LeadStageHistory.Add(LeadStageHistory.Create(
            lead.Id, previousStage, lead.EtapaPipeline, actorId, reason));
        await context.SaveChangesAsync(ct);
        return true;
    }
}
