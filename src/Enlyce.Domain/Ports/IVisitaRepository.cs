using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IVisitaRepository
{
    Task<List<Visita>> ObtenerPorLeadAsync(Guid leadId);
    Task<List<Visita>> ObtenerPorAsesorAsync(Guid asesorId, DateTime desde);
    Task<Visita?> ObtenerPorIdAsync(Guid id);
    Task AgregarAsync(Visita visita);
}
