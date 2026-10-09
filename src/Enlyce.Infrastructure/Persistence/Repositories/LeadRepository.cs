using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly EnlyceDbContext _context;

    public LeadRepository(EnlyceDbContext context) => _context = context;

    public async Task<Lead?> GetByIdAsync(Guid id)
    {
        return await _context.Leads.FindAsync(id);
    }

    public async Task<Lead?> GetByEmailAsync(Domain.ValueObjects.Email email)
    {
        return await _context.Leads
            .FirstOrDefaultAsync(l => l.Email.Value == email.Value);
    }

    public Task<Lead?> GetByOpportunityKeyAsync(string opportunityKey, CancellationToken ct = default) =>
        _context.Leads.FirstOrDefaultAsync(
            lead => lead.OpportunityKey == opportunityKey && lead.Activo, ct);

    public async Task<LeadCreationResult> CreateOrGetExistingAsync(
        Lead lead,
        CancellationToken ct = default,
        LeadAssignmentSource assignmentSource = LeadAssignmentSource.AutomaticLoadBalance)
    {
        _context.Leads.Add(lead);
        LeadAssignmentHistory? assignmentHistory = null;
        if (lead.AsesorAsignadoId is Guid advisorId)
        {
            var source = lead.PublicationId.HasValue ? LeadAssignmentSource.Publication : assignmentSource;
            var reason = lead.PublicationId.HasValue
                ? "Asignada al asesor responsable de la publicacion"
                : source == LeadAssignmentSource.AutomaticRoundRobin
                    ? "Asignada automaticamente por turnos rotativos"
                    : "Asignada automaticamente por menor carga abierta";
            assignmentHistory = LeadAssignmentHistory.Create(
                lead.Id, null, advisorId, null, reason, source, lead.FechaAsignacion);
            _context.LeadAssignmentHistory.Add(assignmentHistory);
        }

        try
        {
            await _context.SaveChangesAsync(ct);
            return new LeadCreationResult(lead, true);
        }
        catch (DbUpdateException)
        {
            _context.Entry(lead).State = EntityState.Detached;
            if (assignmentHistory is not null)
                _context.Entry(assignmentHistory).State = EntityState.Detached;
            var existing = await GetByOpportunityKeyAsync(lead.OpportunityKey, ct);
            if (existing is null)
                throw;

            return new LeadCreationResult(existing, false);
        }
    }

    public async Task<Lead?> ConsumeOwnerInquiryTokenAsync(
        string tokenHash,
        DateTime consumedAt,
        CancellationToken ct = default)
    {
        var leadId = await _context.Leads
            .Where(lead =>
                lead.OwnerInquiryTokenHash == tokenHash &&
                lead.OwnerInquiryTokenExpiresAt > consumedAt &&
                lead.OwnerInquiryTokenConsumedAt == null)
            .Select(lead => lead.Id)
            .SingleOrDefaultAsync(ct);

        if (leadId == Guid.Empty)
            return null;

        var affected = await _context.Leads
            .Where(lead =>
                lead.Id == leadId &&
                lead.OwnerInquiryTokenHash == tokenHash &&
                lead.OwnerInquiryTokenConsumedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    lead => lead.OwnerInquiryTokenConsumedAt,
                    consumedAt),
                ct);

        if (affected != 1)
            return null;

        return await _context.Leads.SingleAsync(lead => lead.Id == leadId, ct);
    }

    public async Task<IReadOnlyList<Lead>> GetAllAsync()
    {
        return await _context.Leads
            .Where(l => l.Activo)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public async Task<LeadSearchPage> SearchPipelineAsync(
        Guid? advisorId,
        string? stage,
        string? search,
        string? operation,
        Guid? assignedAdvisorId,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        IQueryable<Lead> query = _context.Leads.AsNoTracking().Where(lead => lead.Activo);
        if (advisorId is Guid scopedAdvisorId)
            query = query.Where(lead => lead.AsesorAsignadoId == scopedAdvisorId);
        if (!string.IsNullOrWhiteSpace(stage))
            query = query.Where(lead => lead.EtapaPipeline == stage);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(lead =>
                lead.Nombre.ToLower().Contains(term) ||
                lead.Email.Value.ToLower().Contains(term) ||
                (lead.Telefono != null && lead.Telefono.Value.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(operation))
        {
            var normalizedOperation = operation.Trim().ToLower();
            query = query.Where(lead => lead.TipoOperacion.ToLower() == normalizedOperation);
        }
        if (assignedAdvisorId is Guid selectedAdvisorId)
            query = query.Where(lead => lead.AsesorAsignadoId == selectedAdvisorId);
        if (createdFrom is DateOnly from)
        {
            var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(lead => lead.FechaCreacion >= start);
        }
        if (createdTo is DateOnly to && to < DateOnly.MaxValue)
        {
            var endExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(lead => lead.FechaCreacion < endExclusive);
        }

        var total = await query.CountAsync(ct);
        var stageCounts = await query
            .GroupBy(lead => lead.EtapaPipeline)
            .Select(group => new { Stage = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Stage, item => item.Count, ct);
        var offset = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var items = await query
            .OrderByDescending(lead => lead.FechaCreacion)
            .ThenBy(lead => lead.Id)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(ct);

        return new LeadSearchPage(items, total, stageCounts);
    }

    public async Task<IReadOnlyList<Lead>> GetByAsesorIdAsync(Guid asesorId)
    {
        return await _context.Leads
            .Where(l => l.AsesorAsignadoId == asesorId && l.Activo)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Lead>> GetByContactIdAsync(Guid contactId, CancellationToken ct = default) =>
        await _context.Leads.Where(lead => lead.ContactId == contactId && lead.Activo)
            .OrderByDescending(lead => lead.FechaCreacion)
            .ToListAsync(ct);

    public async Task<Lead> SaveAsync(Lead lead)
    {
        var tracked = await _context.Leads.FindAsync(lead.Id);

        if (tracked is null)
        {
            _context.Leads.Add(lead);
        }
        else if (!ReferenceEquals(tracked, lead))
        {
            _context.Entry(tracked).State = EntityState.Detached;
            _context.Attach(lead);
            _context.Entry(lead).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();
        return lead;
    }

    public async Task<bool> ExistsByEmailAsync(Domain.ValueObjects.Email email)
    {
        return await _context.Leads.AnyAsync(l => l.Email.Value == email.Value);
    }

    public async Task<List<Lead>> ObtenerPorEtapaAsync(string etapa)
    {
        return await _context.Leads
            .Where(l => l.EtapaPipeline == etapa && l.Activo)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Lead>> ObtenerSinAsignarAsync()
    {
        return await _context.Leads
            .Where(l => l.AsesorAsignadoId == null && l.Activo)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public async Task<int> ContarPorAsesorAsync(Guid asesorId)
    {
        return await _context.Leads
            .CountAsync(l => l.AsesorAsignadoId == asesorId && l.Activo);
    }

    public async Task<int> ContarPorEtapaAsync(string etapa)
    {
        return await _context.Leads
            .CountAsync(l => l.EtapaPipeline == etapa && l.Activo);
    }
}
