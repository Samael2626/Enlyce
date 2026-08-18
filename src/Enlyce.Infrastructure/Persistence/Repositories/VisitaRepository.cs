using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class VisitaRepository : IVisitaRepository
{
    private readonly EnlyceDbContext _context;

    public VisitaRepository(EnlyceDbContext context) => _context = context;

    public async Task<List<Visita>> ObtenerPorLeadAsync(Guid leadId)
    {
        return await _context.Visitas
            .Where(v => v.LeadId == leadId)
            .OrderByDescending(v => v.FechaProgramada)
            .ToListAsync();
    }

    public async Task<List<Visita>> ObtenerPorAsesorAsync(Guid asesorId, DateTime desde)
    {
        return await _context.Visitas
            .Where(v => v.AsesorId == asesorId && v.FechaProgramada >= desde)
            .OrderBy(v => v.FechaProgramada)
            .ToListAsync();
    }

    public async Task<Visita?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Visitas.FindAsync(id);
    }

    public async Task AgregarAsync(Visita visita)
    {
        await _context.Visitas.AddAsync(visita);
        await _context.SaveChangesAsync();
    }
}
