using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PublicPropertyRepository : IPublicPropertyReadRepository
{
    private readonly EnlyceDbContext _context;

    public PublicPropertyRepository(EnlyceDbContext context)
    {
        _context = context;
    }

    public async Task<PublicPropertyPageData> SearchAsync(
        PublicPropertySearchCriteria criteria,
        CancellationToken ct = default)
    {
        var query =
            from publication in _context.PropertyPublications.AsNoTracking()
            join property in _context.Inmuebles.AsNoTracking()
                on publication.PropertyId equals property.Id
            where publication.Status == PublicationStatus.Published
            select new { Publication = publication, Property = property };

        if (criteria.Operation.HasValue)
            query = query.Where(item => item.Property.Modalidad == criteria.Operation.Value);

        if (criteria.PropertyType.HasValue)
            query = query.Where(item => item.Property.Tipo == criteria.PropertyType.Value);

        if (criteria.Municipality is not null)
        {
            var municipality = criteria.Municipality.ToLower();
            query = query.Where(item =>
                item.Publication.Municipality != null &&
                item.Publication.Municipality.ToLower() == municipality);
        }

        if (criteria.Neighborhood is not null)
        {
            var neighborhood = criteria.Neighborhood.ToLower();
            query = query.Where(item =>
                item.Publication.Neighborhood != null &&
                item.Publication.Neighborhood.ToLower() == neighborhood);
        }

        if (criteria.MinPrice.HasValue)
            query = query.Where(item => item.Publication.PublicPriceAmount >= criteria.MinPrice.Value);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(item => item.Publication.PublicPriceAmount <= criteria.MaxPrice.Value);

        if (criteria.MinArea.HasValue)
            query = query.Where(item => item.Property.MetrosCuadrados >= criteria.MinArea.Value);

        if (criteria.MaxArea.HasValue)
            query = query.Where(item => item.Property.MetrosCuadrados <= criteria.MaxArea.Value);

        if (criteria.MinBedrooms.HasValue)
            query = query.Where(item => item.Property.Habitaciones >= criteria.MinBedrooms.Value);

        if (criteria.MinBathrooms.HasValue)
            query = query.Where(item => item.Property.Banos >= criteria.MinBathrooms.Value);

        if (criteria.MinParkingSpaces.HasValue)
            query = query.Where(item => item.Property.Parqueaderos >= criteria.MinParkingSpaces.Value);

        var total = await query.CountAsync(ct);
        query = criteria.Sort switch
        {
            PublicPropertySort.PriceAscending => query
                .OrderBy(item => (double)(item.Publication.PublicPriceAmount ?? 0))
                .ThenBy(item => item.Publication.Id),
            PublicPropertySort.PriceDescending => query
                .OrderByDescending(item => (double)(item.Publication.PublicPriceAmount ?? 0))
                .ThenBy(item => item.Publication.Id),
            _ => query
                .OrderByDescending(item => item.Publication.PublishedAt)
                .ThenBy(item => item.Publication.Id)
        };

        var items = await query
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(item => new PublicPropertyListData(
                item.Publication.Id,
                item.Publication.Slug,
                item.Publication.PublicTitle,
                item.Property.Tipo,
                item.Property.Modalidad,
                item.Publication.PublicPriceAmount!.Value,
                item.Publication.PublicPriceCurrency!,
                item.Publication.Municipality!,
                item.Publication.Neighborhood!,
                item.Publication.ApproximateLatitude!.Value,
                item.Publication.ApproximateLongitude!.Value,
                item.Property.MetrosCuadrados,
                item.Property.Habitaciones,
                item.Property.Banos,
                item.Property.Parqueaderos,
                null,
                item.Publication.PublishedAt!.Value))
            .ToListAsync(ct);

        if (items.Count == 0)
            return new PublicPropertyPageData(total, items);

        var publicationIds = items.Select(item => item.Id).ToArray();
        var coverRows = await _context.PropertyPhotos
            .AsNoTracking()
            .Where(photo => publicationIds.Contains(photo.PublicationId) && photo.IsCover)
            .Select(photo => new
            {
                photo.PublicationId,
                Photo = new PublicPhotoData(photo.Url, photo.AltText, photo.Order, photo.IsCover)
            })
            .ToListAsync(ct);
        var covers = coverRows.ToDictionary(row => row.PublicationId, row => row.Photo);
        var mappedItems = items
            .Select(item => item with
            {
                CoverPhoto = covers.GetValueOrDefault(item.Id)
            })
            .ToList();

        return new PublicPropertyPageData(total, mappedItems);
    }

    public async Task<PublicPropertyDetailData?> GetBySlugAsync(
        string slug,
        CancellationToken ct = default)
    {
        var detail = await (
            from publication in _context.PropertyPublications.AsNoTracking()
            join property in _context.Inmuebles.AsNoTracking()
                on publication.PropertyId equals property.Id
            join advisor in _context.Asesores.AsNoTracking()
                on publication.AdvisorId equals advisor.Id
            where publication.Status == PublicationStatus.Published &&
                  publication.Slug == slug
            select new PublicPropertyDetailData(
                publication.Id,
                publication.Slug,
                publication.PublicTitle,
                publication.PublicDescription,
                property.Tipo,
                property.Modalidad,
                publication.PublicPriceAmount!.Value,
                publication.PublicPriceCurrency!,
                publication.Municipality!,
                publication.Neighborhood!,
                publication.ApproximateLatitude!.Value,
                publication.ApproximateLongitude!.Value,
                property.MetrosCuadrados,
                property.Habitaciones,
                property.Banos,
                property.Parqueaderos,
                Array.Empty<PublicPhotoData>(),
                advisor.Id,
                advisor.Nombre,
                publication.PublishedAt!.Value))
            .SingleOrDefaultAsync(ct);

        if (detail is null)
            return null;

        var photos = await _context.PropertyPhotos
            .AsNoTracking()
            .Where(photo => photo.PublicationId == detail.Id)
            .OrderBy(photo => photo.Order)
            .Select(photo => new PublicPhotoData(
                photo.Url,
                photo.AltText,
                photo.Order,
                photo.IsCover))
            .ToListAsync(ct);

        return detail with { Photos = photos };
    }
}
