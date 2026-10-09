using Enlyce.Application.Publications;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PropertyPublicationAdminReadRepository
    : IPropertyPublicationAdminReadRepository
{
    private readonly EnlyceDbContext _context;

    public PropertyPublicationAdminReadRepository(EnlyceDbContext context) => _context = context;

    public async Task<IReadOnlyList<PropertyPublicationAdminData>> GetAllAsync(
        Guid? advisorId, CancellationToken ct = default)
    {
        var query =
            from publication in _context.PropertyPublications.AsNoTracking()
            join property in _context.Inmuebles.AsNoTracking()
                on publication.PropertyId equals property.Id
            join advisor in _context.Asesores.AsNoTracking()
                on publication.AdvisorId equals advisor.Id
            where !advisorId.HasValue || publication.AdvisorId == advisorId.Value
            orderby publication.CreatedAt descending
            select new { publication, property, advisor };

        var rows = await query.ToListAsync(ct);
        var publicationIds = rows.Select(row => row.publication.Id).ToArray();
        var photos = await _context.PropertyPhotos.AsNoTracking()
            .Where(photo => publicationIds.Contains(photo.PublicationId))
            .OrderBy(photo => photo.Order)
            .ToListAsync(ct);
        var photosByPublication = photos
            .GroupBy(photo => photo.PublicationId)
            .ToDictionary(group => group.Key, group => group.Select(photo =>
                new PublicationAdminPhotoData(photo.Url, photo.AltText, photo.Order, photo.IsCover))
                .ToList() as IReadOnlyList<PublicationAdminPhotoData>);

        return rows.Select(row => new PropertyPublicationAdminData(
            row.publication.Id,
            row.publication.PropertyId,
            row.property.Nombre,
            row.property.Tipo.ToString(),
            row.property.Modalidad.ToString(),
            row.publication.AdvisorId,
            row.advisor.Nombre,
            row.publication.Slug,
            row.publication.PublicTitle,
            row.publication.PublicDescription,
            row.publication.Status.ToString(),
            row.publication.PublicPriceAmount,
            row.publication.PublicPriceCurrency,
            row.publication.Municipality,
            row.publication.Neighborhood,
            row.publication.ApproximateLatitude,
            row.publication.ApproximateLongitude,
            row.publication.CreatedAt,
            row.publication.PublishedAt,
            photosByPublication.GetValueOrDefault(row.publication.Id, [])))
            .ToList();
    }

    public async Task<PropertyPublicationOptions> GetOptionsAsync(
        Guid? advisorId, CancellationToken ct = default)
    {
        var publishedPropertyIds = _context.PropertyPublications.Select(publication => publication.PropertyId);
        var propertyRows = await _context.Inmuebles.AsNoTracking()
            .Where(property => property.Activo && !publishedPropertyIds.Contains(property.Id))
            .OrderBy(property => property.Nombre)
            .ToListAsync(ct);
        var properties = propertyRows.Select(property => new PublicationPropertyOption(
                property.Id,
                property.Nombre,
                property.Tipo.ToString(),
                property.Modalidad.ToString(),
                property.Direccion.Ciudad,
                property.Direccion.Barrio,
                property.Precio.Monto,
                property.Precio.Moneda))
            .ToList();

        var advisorsQuery = _context.Asesores.AsNoTracking()
            .Where(advisor => advisor.Activo && advisor.Rol == "Asesor");
        if (advisorId.HasValue)
            advisorsQuery = advisorsQuery.Where(advisor => advisor.Id == advisorId.Value);

        var advisors = await advisorsQuery
            .OrderBy(advisor => advisor.Nombre)
            .Select(advisor => new PublicationAdvisorOption(advisor.Id, advisor.Nombre))
            .ToListAsync(ct);

        return new(properties, advisors);
    }
}
