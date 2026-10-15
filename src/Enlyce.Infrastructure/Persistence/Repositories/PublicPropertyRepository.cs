using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PublicPropertyRepository(EnlyceDbContext context) : IPublicPropertyReadRepository
{
    public async Task<PublicPropertyPageData> SearchAsync(
        PublicPropertySearchCriteria criteria,
        CancellationToken ct = default)
    {
        var properties = context.Inmuebles.AsNoTracking().Where(property => property.Activo);

        if (criteria.Operation.HasValue)
            properties = properties.Where(property => property.Modalidad == criteria.Operation.Value);
        if (criteria.PropertyType.HasValue)
            properties = properties.Where(property => property.Tipo == criteria.PropertyType.Value);
        if (criteria.Municipality is not null)
        {
            var municipality = criteria.Municipality.ToLower();
            properties = properties.Where(property => property.Direccion.Ciudad.ToLower() == municipality);
        }
        if (criteria.Neighborhood is not null)
        {
            var neighborhood = criteria.Neighborhood.ToLower();
            properties = properties.Where(property =>
                (property.Direccion.Barrio ?? property.Direccion.Ciudad).ToLower() == neighborhood);
        }
        if (criteria.MinPrice.HasValue)
            properties = properties.Where(property => property.Precio.Monto >= criteria.MinPrice.Value);
        if (criteria.MaxPrice.HasValue)
            properties = properties.Where(property => property.Precio.Monto <= criteria.MaxPrice.Value);
        if (criteria.MinArea.HasValue)
            properties = properties.Where(property => property.MetrosCuadrados >= criteria.MinArea.Value);
        if (criteria.MaxArea.HasValue)
            properties = properties.Where(property => property.MetrosCuadrados <= criteria.MaxArea.Value);
        if (criteria.MinBedrooms.HasValue)
            properties = properties.Where(property => property.Habitaciones >= criteria.MinBedrooms.Value);
        if (criteria.MinBathrooms.HasValue)
            properties = properties.Where(property => property.Banos >= criteria.MinBathrooms.Value);
        if (criteria.MinParkingSpaces.HasValue)
            properties = properties.Where(property => property.Parqueaderos >= criteria.MinParkingSpaces.Value);

        var total = await properties.CountAsync(ct);
        var ordered = criteria.Sort switch
        {
            PublicPropertySort.PriceAscending => properties.OrderBy(property => property.Precio.Monto)
                .ThenBy(property => property.Id),
            PublicPropertySort.PriceDescending => properties.OrderByDescending(property => property.Precio.Monto)
                .ThenBy(property => property.Id),
            _ => properties.OrderByDescending(property => property.FechaCreacion).ThenBy(property => property.Id)
        };
        var page = await ordered.Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize).ToListAsync(ct);
        if (page.Count == 0)
            return new PublicPropertyPageData(total, []);

        var ids = page.Select(property => property.Id).ToArray();
        var publications = await context.PropertyPublications.AsNoTracking()
            .Where(publication => ids.Contains(publication.PropertyId) && publication.Status == PublicationStatus.Published)
            .ToDictionaryAsync(publication => publication.PropertyId, ct);
        var publicationIds = publications.Values.Select(publication => publication.Id).ToArray();
        var covers = await context.PropertyPhotos.AsNoTracking()
            .Where(photo => publicationIds.Contains(photo.PublicationId) && photo.IsCover)
            .ToDictionaryAsync(photo => photo.PublicationId,
                photo => new PublicPhotoData(photo.Url, photo.AltText, photo.Order, photo.IsCover), ct);

        var items = page.Select(property =>
        {
            publications.TryGetValue(property.Id, out var publication);
            return new PublicPropertyListData(
                property.Id,
                publication?.Slug ?? BuildCrmSlug(property.Id),
                publication?.PublicTitle ?? property.Nombre,
                property.Tipo,
                property.Modalidad,
                publication?.PublicPriceAmount ?? property.Precio.Monto,
                publication?.PublicPriceCurrency ?? property.Precio.Moneda,
                publication?.Municipality ?? property.Direccion.Ciudad,
                publication?.Neighborhood ?? property.Direccion.Barrio ?? property.Direccion.Ciudad,
                publication?.ApproximateLatitude,
                publication?.ApproximateLongitude,
                property.MetrosCuadrados,
                property.Habitaciones,
                property.Banos,
                property.Parqueaderos,
                publication is not null ? covers.GetValueOrDefault(publication.Id) : null,
                publication?.PublishedAt ?? property.FechaCreacion);
        }).ToList();

        return new PublicPropertyPageData(total, items);
    }

    public async Task<PublicPropertyDetailData?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var published = await (
            from publication in context.PropertyPublications.AsNoTracking()
            join property in context.Inmuebles.AsNoTracking() on publication.PropertyId equals property.Id
            join advisor in context.Asesores.AsNoTracking() on publication.AdvisorId equals advisor.Id into advisors
            from advisor in advisors.DefaultIfEmpty()
            where publication.Status == PublicationStatus.Published && publication.Slug == slug && property.Activo
            select new PublicPropertyDetailData(
                property.Id, publication.Slug, publication.PublicTitle, publication.PublicDescription,
                property.Tipo, property.Modalidad, publication.PublicPriceAmount!.Value,
                publication.PublicPriceCurrency!, publication.Municipality!, publication.Neighborhood!,
                publication.ApproximateLatitude, publication.ApproximateLongitude,
                property.MetrosCuadrados, property.Habitaciones, property.Banos, property.Parqueaderos,
                Array.Empty<PublicPhotoData>(), advisor == null ? null : advisor.Id,
                advisor == null ? null : advisor.Nombre, publication.Id, publication.PublishedAt!.Value))
            .SingleOrDefaultAsync(ct);

        if (published is not null)
            return await AddPhotosAsync(published, published.PublicationId!.Value, ct);

        if (!TryParseCrmSlug(slug, out var propertyId))
            return null;

        var propertyRecord = await context.Inmuebles.AsNoTracking()
            .SingleOrDefaultAsync(property => property.Id == propertyId && property.Activo, ct);
        if (propertyRecord is null)
            return null;

        var detail = new PublicPropertyDetailData(
            propertyRecord.Id, slug, propertyRecord.Nombre, propertyRecord.Descripcion,
            propertyRecord.Tipo, propertyRecord.Modalidad, propertyRecord.Precio.Monto,
            propertyRecord.Precio.Moneda, propertyRecord.Direccion.Ciudad,
            propertyRecord.Direccion.Barrio ?? propertyRecord.Direccion.Ciudad,
            null, null, propertyRecord.MetrosCuadrados, propertyRecord.Habitaciones,
            propertyRecord.Banos, propertyRecord.Parqueaderos, [], null, null, null,
            propertyRecord.FechaCreacion);
        return detail;
    }

    private async Task<PublicPropertyDetailData> AddPhotosAsync(
        PublicPropertyDetailData detail, Guid publicationId, CancellationToken ct)
    {
        var photos = await context.PropertyPhotos.AsNoTracking()
            .Where(photo => photo.PublicationId == publicationId)
            .OrderBy(photo => photo.Order)
            .Select(photo => new PublicPhotoData(photo.Url, photo.AltText, photo.Order, photo.IsCover))
            .ToListAsync(ct);
        return detail with { Photos = photos };
    }

    private static string BuildCrmSlug(Guid id) => $"crm-{id:N}";

    private static bool TryParseCrmSlug(string slug, out Guid id)
    {
        id = Guid.Empty;
        return slug.StartsWith("crm-", StringComparison.Ordinal) &&
               Guid.TryParseExact(slug.AsSpan(4), "N", out id);
    }
}
