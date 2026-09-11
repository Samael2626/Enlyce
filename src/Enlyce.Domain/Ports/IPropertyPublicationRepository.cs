using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IPropertyPublicationRepository
{
    Task<PropertyPublication?> GetPublishedByIdAsync(Guid id);
}
