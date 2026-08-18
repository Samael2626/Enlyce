using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly EnlyceDbContext _context;

    public AuditLogRepository(EnlyceDbContext context) => _context = context;

    public async Task RegistrarAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> ObtenerPorEntidadAsync(string entidad, Guid entidadId)
    {
        return await _context.AuditLogs
            .Where(a => a.Entidad == entidad && a.EntidadId == entidadId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
