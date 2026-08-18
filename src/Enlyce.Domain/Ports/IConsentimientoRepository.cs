using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IConsentimientoRepository
{
    Task<Consentimiento?> ObtenerPorLeadAsync(Guid leadId);
    Task<bool> TieneConsentimientoAsync(Guid leadId);
    Task AgregarAsync(Consentimiento consentimiento);
}
