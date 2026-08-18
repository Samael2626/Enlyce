using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IAuditLogRepository
{
    Task RegistrarAsync(AuditLog log);
    Task<List<AuditLog>> ObtenerPorEntidadAsync(string entidad, Guid entidadId);
}
