using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PublicPropertyRepository(EnlyceDbContext context, IConfiguration? configuration = null) : IPublicPropertyReadRepository
{
    // Cierre temporal hasta completar la validacion y autorizacion comercial.
    private bool PublishingEnabled => configuration?.GetValue<bool>("PublicCatalog:Enabled") == true;

    private IQueryable<PropertyPublication> VisiblePublications() => context.PropertyPublications.AsNoTracking()
        .Where(publication => publication.Status == PublicationStatus.Published &&
            publication.PublicPriceAmount > 0 && publication.PublicPriceCurrency != null &&
            publication.PublicTitle != "" && publication.PublicDescription != "" &&
            publication.Municipality != null && publication.Municipality != "" &&
            publication.Neighborhood != null && publication.Neighborhood != "" &&
            publication.ApproximateLatitude != null && publication.ApproximateLongitude != null &&
            publication.PublishedAt != null &&
            context.PropertyPhotos.Count(photo => photo.PublicationId == publication.Id) >= 3 &&
            context.PropertyPhotos.Count(photo => photo.PublicationId == publication.Id && photo.IsCover) == 1 &&
            context.Asesores.Any(advisor => advisor.Id == publication.AdvisorId && advisor.Activo &&
                advisor.Correo.Value != "asesor.demo@enlyce.test") &&
            !context.Inmuebles.Any(property => property.Id == publication.PropertyId &&
                context.Propietarios.Any(owner => owner.Id == property.PropietarioId &&
                    owner.Email.Value == "catalogo.demo@enlyce.test")));

    public async Task<string?> GetVisibleSlugByPropertyIdAsync(Guid propertyId, CancellationToken ct = default)
    {
        if (!PublishingEnabled) return null;
        var isVisible = await context.Inmuebles.AsNoTracking()
            .AnyAsync(property => property.Id == propertyId && property.Activo &&
                property.Estado == EstadoInmueble.Disponible, ct);
        if (!isVisible)
            return null;

        return await VisiblePublications()
            .Where(publication => publication.PropertyId == propertyId)
            .Select(publication => publication.Slug)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<PublicPropertyPageData> SearchAsync(
        PublicPropertySearchCriteria criteria,
        CancellationToken ct = default)
    {
        if (!PublishingEnabled) return new PublicPropertyPageData(0, []);
        var visible = VisiblePublications();
        var properties = context.Inmuebles.AsNoTracking()
            .Where(property => property.Activo &&
                property.Estado == EstadoInmueble.Disponible &&
                visible.Any(publication => publication.PropertyId == property.Id));

        if (criteria.Operation.HasValue)
            properties = properties.Where(property => property.Modalidad == criteria.Operation.Value);
        if (criteria.PropertyType.HasValue)
            properties = properties.Where(property => property.Tipo == criteria.PropertyType.Value);
        if (criteria.Municipality is not null)
        {
            var municipality = criteria.Municipality.ToLower();
            properties = properties.Where(property => visible.Any(publication =>
                publication.PropertyId == property.Id && publication.Municipality!.ToLower() == municipality));
        }
        if (criteria.Neighborhood is not null)
        {
            var neighborhood = criteria.Neighborhood.ToLower();
            properties = properties.Where(property => visible.Any(publication =>
                publication.PropertyId == property.Id && publication.Neighborhood!.ToLower() == neighborhood));
        }
        if (criteria.MinPrice.HasValue)
            properties = properties.Where(property => visible.Any(publication =>
                publication.PropertyId == property.Id && publication.PublicPriceAmount >= criteria.MinPrice.Value));
        if (criteria.MaxPrice.HasValue)
            properties = properties.Where(property => visible.Any(publication =>
                publication.PropertyId == property.Id && publication.PublicPriceAmount <= criteria.MaxPrice.Value));
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
            PublicPropertySort.PriceAscending => properties.OrderBy(property => visible
                .Where(publication => publication.PropertyId == property.Id).Select(publication => publication.PublicPriceAmount).First())
                .ThenBy(property => property.Id),
            PublicPropertySort.PriceDescending => properties.OrderByDescending(property => visible
                .Where(publication => publication.PropertyId == property.Id).Select(publication => publication.PublicPriceAmount).First())
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
            var publication = publications[property.Id];
            return new PublicPropertyListData(
                property.Id,
                publication.Slug,
                publication.PublicTitle,
                property.Tipo,
                property.Modalidad,
                publication.PublicPriceAmount!.Value,
                publication.PublicPriceCurrency!,
                publication.Municipality!,
                publication.Neighborhood!,
                publication.ApproximateLatitude,
                publication.ApproximateLongitude,
                property.MetrosCuadrados,
                property.Habitaciones,
                property.Banos,
                property.Parqueaderos,
                covers.GetValueOrDefault(publication.Id),
                publication.PublishedAt!.Value);
        }).ToList();

        return new PublicPropertyPageData(total, items);
    }

    public async Task<PublicPropertyDetailData?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (!PublishingEnabled) return null;
        var published = await (
            from publication in VisiblePublications()
            join property in context.Inmuebles.AsNoTracking() on publication.PropertyId equals property.Id
            join advisor in context.Asesores.AsNoTracking() on publication.AdvisorId equals advisor.Id into advisors
            from advisor in advisors.DefaultIfEmpty()
            where publication.Status == PublicationStatus.Published && publication.Slug == slug &&
                  property.Activo && property.Estado == EstadoInmueble.Disponible
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
        {
            return await AddPhotosAsync(published, published.PublicationId!.Value, ct);
        }
        return null;
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

}
