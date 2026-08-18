using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class PropietarioRepository : IPropietarioRepository
{
    private readonly EnlyceDbContext _context;

    public PropietarioRepository(EnlyceDbContext context) => _context = context;

    public async Task<Propietario?> GetByIdAsync(Guid id)
    {
        return await _context.Propietarios.FindAsync(id);
    }

    public async Task<Propietario?> GetByEmailAsync(Domain.ValueObjects.Email email)
    {
        return await _context.Propietarios
            .FirstOrDefaultAsync(p => p.Email.Value == email.Value);
    }

    public async Task<IReadOnlyList<Propietario>> GetAllAsync()
    {
        return await _context.Propietarios
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Propietario> SaveAsync(Propietario propietario)
    {
        var existing = await _context.Propietarios.FindAsync(propietario.Id);

        if (existing is null)
            _context.Propietarios.Add(propietario);
        else
            _context.Entry(existing).CurrentValues.SetValues(propietario);

        await _context.SaveChangesAsync();
        return propietario;
    }

    public async Task<bool> ExistsByEmailAsync(Domain.ValueObjects.Email email)
    {
        return await _context.Propietarios.AnyAsync(p => p.Email.Value == email.Value);
    }
}
