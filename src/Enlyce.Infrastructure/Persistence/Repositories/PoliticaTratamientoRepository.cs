using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class PoliticaTratamientoRepository : IPoliticaTratamientoRepository
{
    private readonly EnlyceDbContext _context;

    public PoliticaTratamientoRepository(EnlyceDbContext context) => _context = context;

    public async Task<PoliticaTratamiento?> ObtenerActivaAsync()
    {
        return await _context.PoliticasTratamiento
            .FirstOrDefaultAsync(p => p.Activa);
    }

    public async Task<PoliticaTratamiento?> ObtenerPorVersionAsync(string version)
    {
        return await _context.PoliticasTratamiento
            .FirstOrDefaultAsync(p => p.Version == version);
    }

    public async Task AgregarAsync(PoliticaTratamiento politica)
    {
        await _context.PoliticasTratamiento.AddAsync(politica);
        await _context.SaveChangesAsync();
    }
}
