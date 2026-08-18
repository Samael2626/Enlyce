using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class InmuebleRepository : IInmuebleRepository
{
    private readonly EnlyceDbContext _context;

    public InmuebleRepository(EnlyceDbContext context) => _context = context;

    public async Task<Inmueble?> GetByIdAsync(Guid id)
    {
        return await _context.Inmuebles.FindAsync(id);
    }

    public async Task<IReadOnlyList<Inmueble>> GetAllAsync()
    {
        return await _context.Inmuebles
            .Where(i => i.Activo)
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Inmueble>> GetByPropietarioIdAsync(Guid propietarioId)
    {
        return await _context.Inmuebles
            .Where(i => i.PropietarioId == propietarioId && i.Activo)
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Inmueble> SaveAsync(Inmueble inmueble)
    {
        var existing = await _context.Inmuebles.FindAsync(inmueble.Id);

        if (existing is null)
            _context.Inmuebles.Add(inmueble);
        else
            _context.Entry(existing).CurrentValues.SetValues(inmueble);

        await _context.SaveChangesAsync();
        return inmueble;
    }
}
