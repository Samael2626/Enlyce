using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class InteraccionRepository : IInteraccionRepository
{
    private readonly EnlyceDbContext _context;

    public InteraccionRepository(EnlyceDbContext context) => _context = context;

    public async Task<List<Interaccion>> ObtenerPorLeadAsync(Guid leadId)
    {
        return await _context.Interacciones
            .Where(i => i.LeadId == leadId)
            .OrderByDescending(i => i.Fecha)
            .ToListAsync();
    }

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
