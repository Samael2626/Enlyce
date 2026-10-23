using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IRealizedDealRepository
{
    Task AddAsync(RealizedDeal deal, CancellationToken ct = default);
    Task<RealizedDeal?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
