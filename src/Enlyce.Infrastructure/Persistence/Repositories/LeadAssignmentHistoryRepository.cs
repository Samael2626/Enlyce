using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class LeadAssignmentHistoryRepository(EnlyceDbContext context) : ILeadAssignmentHistoryRepository
{
    public async Task<IReadOnlyList<LeadAssignmentHistory>> GetByLeadIdAsync(
        Guid leadId, CancellationToken ct = default) =>
        await context.LeadAssignmentHistory.AsNoTracking()
            .Where(item => item.LeadId == leadId)
            .OrderBy(item => item.ChangedAt)
            .ThenBy(item => item.Id)
            .ToListAsync(ct);

    public async Task<bool> ChangeAssignmentAsync(
        Lead lead,
        Guid newAdvisorId,
        Guid actorId,
        string reason,
        LeadAssignmentSource source,
        CancellationToken ct = default)
    {
        var previousAdvisorId = lead.AsesorAsignadoId;
        if (previousAdvisorId == newAdvisorId)
            return true;

        var history = LeadAssignmentHistory.Create(
            lead.Id, previousAdvisorId, newAdvisorId, actorId, reason, source);
        lead.AsignarAsesor(newAdvisorId);
        context.LeadAssignmentHistory.Add(history);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
