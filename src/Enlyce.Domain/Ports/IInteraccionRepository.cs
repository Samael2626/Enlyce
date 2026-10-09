using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IInteraccionRepository
{
    Task<List<Interaccion>> ObtenerPorLeadAsync(Guid leadId);
    Task<List<Interaccion>> ObtenerHastaPrimerContactoAsync(
        IReadOnlyCollection<Guid> leadIds,
        CancellationToken ct = default);
    Task<int> ContarPorLeadAsync(Guid leadId);
    Task AgregarAsync(Interaccion interaccion);
}
