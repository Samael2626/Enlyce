using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class CustomerDemandRepository(EnlyceDbContext context) : ICustomerDemandRepository
{
    public Task<CustomerDemand?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.CustomerDemands.SingleOrDefaultAsync(demand => demand.Id == id && demand.Active, ct);

    public async Task<IReadOnlyList<CustomerDemand>> GetByContactIdAsync(Guid contactId, CancellationToken ct = default) =>
        await context.CustomerDemands.AsNoTracking().Where(demand => demand.ContactId == contactId && demand.Active)
            .OrderByDescending(demand => demand.UpdatedAt).ToListAsync(ct);

    public async Task AddAsync(CustomerDemand demand, CancellationToken ct = default)
    {
        context.CustomerDemands.Add(demand);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DemandPropertyLink>> GetPropertyLinksAsync(Guid demandId, CancellationToken ct = default) =>
        await context.DemandPropertyLinks.AsNoTracking().Where(link => link.DemandId == demandId)
            .OrderByDescending(link => link.UpdatedAt).ToListAsync(ct);

    public Task<DemandPropertyLink?> GetPropertyLinkAsync(Guid demandId, Guid propertyId, CancellationToken ct = default) =>
        context.DemandPropertyLinks.SingleOrDefaultAsync(link => link.DemandId == demandId && link.PropertyId == propertyId, ct);

    public async Task AddPropertyLinkAsync(DemandPropertyLink link, CancellationToken ct = default)
    {
        context.DemandPropertyLinks.Add(link);
        await context.SaveChangesAsync(ct);
    }

    public async Task SavePropertyLinkAsync(DemandPropertyLink link, CancellationToken ct = default)
    {
        context.DemandPropertyLinks.Update(link);
        await context.SaveChangesAsync(ct);
    }
}
