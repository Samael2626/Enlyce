using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class CustomerCatalogRepository(EnlyceDbContext context) : ICustomerCatalogRepository
{
    public async Task<IReadOnlyList<CustomerCatalogItem>> GetAllAsync(CancellationToken ct = default) =>
        await context.CustomerCatalogItems.AsNoTracking()
            .OrderBy(item => item.Catalog).ThenBy(item => item.Name).ToListAsync(ct);

    public async Task<bool> AddAsync(CustomerCatalogItem item, CancellationToken ct = default)
    {
        if (await context.CustomerCatalogItems.AnyAsync(existing =>
            existing.Catalog == item.Catalog && existing.NormalizedName == item.NormalizedName, ct))
            return false;

        context.CustomerCatalogItems.Add(item);
        try
        {
            await context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.Entry(item).State = EntityState.Detached;
            return false;
        }
    }
}
