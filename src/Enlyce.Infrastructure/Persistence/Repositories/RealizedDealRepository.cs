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

    public Task<RealizedDeal?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.RealizedDeals.FindAsync([id], ct).AsTask();

    public Task SaveAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
