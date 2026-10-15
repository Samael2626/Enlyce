using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IRealizedDealRepository
{
    Task AddAsync(RealizedDeal deal, CancellationToken ct = default);
}
