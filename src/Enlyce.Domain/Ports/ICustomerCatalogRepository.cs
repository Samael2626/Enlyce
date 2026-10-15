using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface ICustomerCatalogRepository
{
    Task<IReadOnlyList<CustomerCatalogItem>> GetAllAsync(CancellationToken ct = default);
    Task<bool> AddAsync(CustomerCatalogItem item, CancellationToken ct = default);
}
