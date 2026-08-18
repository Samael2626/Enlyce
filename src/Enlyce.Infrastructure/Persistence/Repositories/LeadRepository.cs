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

    public async Task<IReadOnlyList<Lead>> GetAllAsync()
    {
        return await _context.Leads
            .Where(l => l.Activo)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Lead>> GetByAsesorIdAsync(Guid asesorId)
    {
        return await _context.Leads
            .Where(l => l.AsesorAsignadoId == asesorId && l.Activo)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

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
