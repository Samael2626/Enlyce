using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IPropertyPublicationRepository
{
    Task<PropertyPublication?> GetPublishedByIdAsync(Guid id);

    // Rastreada y en cualquier estado: la carga de fotos ocurre sobre borradores.
    Task<PropertyPublication?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsForPropertyAsync(Guid propertyId, CancellationToken ct = default);

    Task<bool> SlugExistsAsync(string slug, Guid? excludingId = null, CancellationToken ct = default);

    Task SaveAsync(PropertyPublication publication, CancellationToken ct = default);
}
