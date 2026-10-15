using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class InteraccionRepository : IInteraccionRepository
{
    private readonly EnlyceDbContext _context;

    public InteraccionRepository(EnlyceDbContext context) => _context = context;

    public async Task<InteractionSearchPage> SearchAsync(Guid? advisorId, string? type, string? queryText,
        DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default)
    {
        var query =
            from interaction in _context.Interacciones.AsNoTracking()
            join lead in _context.Leads.AsNoTracking() on interaction.LeadId equals lead.Id
            join contact in _context.Contacts.AsNoTracking() on lead.ContactId equals contact.Id into contacts
            from contact in contacts.DefaultIfEmpty()
            select new { interaction, lead, contact };

        if (advisorId.HasValue)
            query = query.Where(item => item.interaction.AsesorId == advisorId.Value);
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(item => item.interaction.Tipo == type.Trim());
        if (from.HasValue)
            query = query.Where(item => item.interaction.Fecha >= from.Value);
        if (to.HasValue)
            query = query.Where(item => item.interaction.Fecha < to.Value);
        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var search = queryText.Trim().ToLower();
            query = query.Where(item =>
                (item.contact != null && item.contact.Name.ToLower().Contains(search)) ||
                item.lead.Nombre.ToLower().Contains(search));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(item => item.interaction.Fecha)
            .ThenBy(item => item.interaction.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new InteractionSearchItem(
                item.interaction.Id, item.interaction.LeadId, item.lead.ContactId,
                item.interaction.AsesorId, item.interaction.Tipo, item.interaction.Resumen,
                item.interaction.Fecha, item.contact == null ? item.lead.Nombre : item.contact.Name))
            .ToListAsync(ct);
        return new InteractionSearchPage(total, items);
    }

    public async Task<List<Interaccion>> ObtenerPorLeadAsync(Guid leadId)
    {
        return await _context.Interacciones
            .Where(i => i.LeadId == leadId)
            .OrderByDescending(i => i.Fecha)
            .ToListAsync();
    }

    public Task<List<Interaccion>> ObtenerHastaPrimerContactoAsync(
        IReadOnlyCollection<Guid> leadIds,
        CancellationToken ct = default) =>
        (from interaction in _context.Interacciones.AsNoTracking()
         join lead in _context.Leads.AsNoTracking() on interaction.LeadId equals lead.Id
         where leadIds.Contains(lead.Id) &&
               lead.FechaPrimerContacto.HasValue &&
               interaction.Fecha <= lead.FechaPrimerContacto.Value
         select interaction).ToListAsync(ct);

    public async Task<int> ContarPorLeadAsync(Guid leadId)
    {
        return await _context.Interacciones
            .CountAsync(i => i.LeadId == leadId);
    }

    public async Task AgregarAsync(Interaccion interaccion)
    {
        await _context.Interacciones.AddAsync(interaccion);
        await _context.SaveChangesAsync();
    }
}
