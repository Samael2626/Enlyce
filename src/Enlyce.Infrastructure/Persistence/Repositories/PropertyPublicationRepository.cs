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

    public Task<bool> ExistsForPropertyAsync(Guid propertyId, CancellationToken ct = default) =>
        _context.PropertyPublications.AnyAsync(publication => publication.PropertyId == propertyId, ct);

    public Task<bool> SlugExistsAsync(
        string slug, Guid? excludingId = null, CancellationToken ct = default) =>
        _context.PropertyPublications.AnyAsync(publication =>
            publication.Slug == slug &&
            (!excludingId.HasValue || publication.Id != excludingId.Value), ct);

    public async Task SaveAsync(PropertyPublication publication, CancellationToken ct = default)
    {
        if (_context.Entry(publication).State == EntityState.Detached)
        {
            var exists = await _context.PropertyPublications
                .AnyAsync(item => item.Id == publication.Id, ct);
            if (exists)
                _context.PropertyPublications.Update(publication);
            else
                _context.PropertyPublications.Add(publication);
        }

        await _context.SaveChangesAsync(ct);
    }
}
