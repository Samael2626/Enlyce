using Enlyce.Application.Imports;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class ImportedListingsReadRepository : IImportedListingsReadRepository
{
    private readonly EnlyceDbContext _context;

    public ImportedListingsReadRepository(EnlyceDbContext context) => _context = context;

    public async Task<ImportedListingsCounts> CountAsync(string source, CancellationToken ct = default)
    {
        var prefix = source + ":";
        var rows = await (
                from property in _context.Inmuebles.AsNoTracking()
                where property.ExternalRef != null && property.ExternalRef.StartsWith(prefix)
                join publication in _context.PropertyPublications.AsNoTracking()
                    on property.Id equals publication.PropertyId into publications
                from publication in publications.DefaultIfEmpty()
                select new
                {
                    HasPublication = publication != null,
                    Status = publication != null ? publication.Status : (PublicationStatus?)null,
                    Photos = publication != null ? publication.Photos.Count : 0
                })
            .ToListAsync(ct);

        return new ImportedListingsCounts(
            rows.Count,
            rows.Count(row => row.HasPublication),
            rows.Count(row => row.Status == PublicationStatus.Published),
            rows.Count(row => row.Status == PublicationStatus.Draft),
            rows.Count(row => !row.HasPublication),
            rows.Sum(row => row.Photos));
    }
}
