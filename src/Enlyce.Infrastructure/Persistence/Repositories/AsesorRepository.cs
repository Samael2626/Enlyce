using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class AsesorRepository : IAsesorRepository
{
    private readonly EnlyceDbContext _context;

    public AsesorRepository(EnlyceDbContext context)
    {
        _context = context;
    }

    public async Task<Asesor?> ObtenerPorCorreoAsync(string correo)
    {
        return await _context.Asesores
            .FirstOrDefaultAsync(a => a.Correo.Value == correo);
    }

    public async Task<Asesor?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Asesores.FindAsync(id);
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        return await _context.Asesores.AnyAsync(a => a.Correo.Value == correo);
    }

    public async Task AgregarAsync(Asesor asesor)
    {
        await _context.Asesores.AddAsync(asesor);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Asesor>> ObtenerTodosAsync()
    {
        return await _context.Asesores.ToListAsync();
    }
}
