using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class RealizedDealRepository(EnlyceDbContext context) : IRealizedDealRepository
{
    public async Task AddAsync(RealizedDeal deal, CancellationToken ct = default)
    {
        context.RealizedDeals.Add(deal);
        await context.SaveChangesAsync(ct);
    }
}
