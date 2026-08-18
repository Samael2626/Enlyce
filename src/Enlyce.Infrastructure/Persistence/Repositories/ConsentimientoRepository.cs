using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class ConsentimientoRepository : IConsentimientoRepository
{
    private readonly EnlyceDbContext _context;

    public ConsentimientoRepository(EnlyceDbContext context) => _context = context;

    public async Task<Consentimiento?> ObtenerPorLeadAsync(Guid leadId)
    {
        return await _context.Consentimientos
            .FirstOrDefaultAsync(c => c.LeadId == leadId);
    }

    public async Task<bool> TieneConsentimientoAsync(Guid leadId)
    {
        return await _context.Consentimientos.AnyAsync(c => c.LeadId == leadId);
    }

    public async Task AgregarAsync(Consentimiento consentimiento)
    {
        await _context.Consentimientos.AddAsync(consentimiento);
        await _context.SaveChangesAsync();
    }
}
