using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PropertyPublicationRepository : IPropertyPublicationRepository
{
    private readonly EnlyceDbContext _context;

    public PropertyPublicationRepository(EnlyceDbContext context)
    {
        _context = context;
    }

    public Task<PropertyPublication?> GetPublishedByIdAsync(Guid id) =>
        _context.PropertyPublications
            .AsNoTracking()
            .SingleOrDefaultAsync(publication =>
                publication.Id == id &&
                publication.Status == PublicationStatus.Published);

    public Task<PropertyPublication?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.PropertyPublications
            .Include(publication => publication.Photos)
            .SingleOrDefaultAsync(publication => publication.Id == id, ct);

    public async Task SaveAsync(PropertyPublication publication, CancellationToken ct = default)
    {
        if (_context.Entry(publication).State == EntityState.Detached)
            _context.PropertyPublications.Update(publication);

        await _context.SaveChangesAsync(ct);
    }
}
