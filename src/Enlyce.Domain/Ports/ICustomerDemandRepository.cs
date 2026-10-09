using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ICustomerDemandRepository
{
    Task<CustomerDemand?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerDemand>> GetByContactIdAsync(Guid contactId, CancellationToken ct = default);
    Task AddAsync(CustomerDemand demand, CancellationToken ct = default);
    Task<IReadOnlyList<DemandPropertyLink>> GetPropertyLinksAsync(Guid demandId, CancellationToken ct = default);
    Task<DemandPropertyLink?> GetPropertyLinkAsync(Guid demandId, Guid propertyId, CancellationToken ct = default);
    Task AddPropertyLinkAsync(DemandPropertyLink link, CancellationToken ct = default);
    Task SavePropertyLinkAsync(DemandPropertyLink link, CancellationToken ct = default);
}
