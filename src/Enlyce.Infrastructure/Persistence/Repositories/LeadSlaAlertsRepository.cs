using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class LeadSlaAlertsRepository(EnlyceDbContext context) : ILeadSlaAlertsRepository
{
    public async Task<IReadOnlyList<LeadSlaAlertCandidate>> GetCandidatesAsync(
        Guid? advisorId, CancellationToken ct = default)
    {
        var query = context.Leads.AsNoTracking().Where(lead => lead.Activo &&
            lead.EtapaPipeline != EtapasPipeline.CerradoGanado &&
            lead.EtapaPipeline != EtapasPipeline.CerradoPerdido);
        if (advisorId.HasValue)
            query = query.Where(lead => lead.AsesorAsignadoId == advisorId.Value);

        var candidates = await query.Select(lead => new LeadSlaAlertCandidate(
                lead.Id,
                lead.Nombre,
                lead.Email.Value,
                lead.TipoOperacion,
                lead.Fuente,
                lead.EtapaPipeline,
                lead.FechaPrimerContacto,
                lead.FechaUltimaInteraccion,
                lead.FechaAsignacion))
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return candidates;

        var leadIds = candidates.Select(candidate => candidate.LeadId).ToArray();
        var firstAssignments = await context.LeadAssignmentHistory.AsNoTracking()
            .Where(history => leadIds.Contains(history.LeadId))
            .GroupBy(history => history.LeadId)
            .Select(group => new { LeadId = group.Key, FirstAssignedAt = group.Min(item => item.ChangedAt) })
            .ToDictionaryAsync(item => item.LeadId, item => item.FirstAssignedAt, ct);

        return candidates.Select(candidate => firstAssignments.TryGetValue(candidate.LeadId, out var firstAssignedAt)
                ? candidate with { FechaPrimeraAsignacion = firstAssignedAt }
                : candidate)
            .ToArray();
    }
}
