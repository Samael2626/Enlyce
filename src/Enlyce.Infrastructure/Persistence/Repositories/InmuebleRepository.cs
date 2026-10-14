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

    public async Task<IReadOnlyList<Inmueble>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        ids.Count == 0 ? [] : await _context.Inmuebles.AsNoTracking().Where(i => ids.Contains(i.Id)).ToListAsync(ct);

    public async Task<IReadOnlyList<Inmueble>> GetByExternalRefsAsync(IReadOnlyCollection<string> externalRefs)
    {
        if (externalRefs.Count == 0)
            return [];

        return await _context.Inmuebles
            .Where(i => i.ExternalRef != null && externalRefs.Contains(i.ExternalRef))
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
