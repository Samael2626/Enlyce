using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IPropertyPublicationRepository
{
    Task<PropertyPublication?> GetPublishedByIdAsync(Guid id);

    // Rastreada y en cualquier estado: la carga de fotos ocurre sobre borradores.
    Task<PropertyPublication?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task SaveAsync(PropertyPublication publication, CancellationToken ct = default);
}
