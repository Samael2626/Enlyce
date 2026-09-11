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
}
