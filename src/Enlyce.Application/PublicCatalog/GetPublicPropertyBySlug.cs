using Enlyce.Application.Abstractions;

namespace Enlyce.Application.PublicCatalog;

public sealed record GetPublicPropertyBySlugQuery(string Slug);

public sealed class GetPublicPropertyBySlugHandler
    : IQueryHandler<GetPublicPropertyBySlugQuery, PublicPropertyDetailResponse?>
{
    private readonly IPublicPropertyReadRepository _repository;

    public GetPublicPropertyBySlugHandler(IPublicPropertyReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<PublicPropertyDetailResponse?> HandleAsync(
        GetPublicPropertyBySlugQuery query,
        CancellationToken ct = default)
    {
        var slug = query.Slug.Trim().ToLowerInvariant();
        var item = await _repository.GetBySlugAsync(slug, ct);
        if (item is null)
            return null;

        return new PublicPropertyDetailResponse(
            item.Id,
            item.Slug,
            item.PublicTitle,
            item.PublicDescription,
            item.PropertyType.ToString(),
            item.Operation.ToString(),
            new PublicMoneyResponse(item.PriceAmount, item.PriceCurrency),
            null,
            new PublicLocationResponse(
                item.Municipality,
                item.Neighborhood,
                item.ApproximateLatitude,
                item.ApproximateLongitude),
            new PublicPropertyFeaturesResponse(
                item.AreaSquareMeters,
                item.Bedrooms,
                item.Bathrooms,
                item.ParkingSpaces,
                null,
                []),
            item.Photos.Select(GetPublicPropertiesHandler.MapPhoto).ToList(),
            new PublicAdvisorResponse(item.AdvisorId, item.AdvisorDisplayName, null),
            item.PublishedAt);
    }
}
