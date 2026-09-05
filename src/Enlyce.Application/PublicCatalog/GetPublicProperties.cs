using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;

namespace Enlyce.Application.PublicCatalog;

public sealed record GetPublicPropertiesQuery(
    int Page = 1,
    int PageSize = 12,
    string? Operation = null,
    string? PropertyType = null,
    string? Municipality = null,
    string? Neighborhood = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? MinArea = null,
    int? MaxArea = null,
    int? Bedrooms = null,
    int? Bathrooms = null,
    int? ParkingSpaces = null,
    string? Sort = null);

public sealed class GetPublicPropertiesHandler
    : IQueryHandler<GetPublicPropertiesQuery, PublicPropertyPageResponse>
{
    private const int MaximumPageSize = 100;
    private readonly IPublicPropertyReadRepository _repository;

    public GetPublicPropertiesHandler(IPublicPropertyReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<PublicPropertyPageResponse> HandleAsync(
        GetPublicPropertiesQuery query,
        CancellationToken ct = default)
    {
        var criteria = ValidateAndCreateCriteria(query);
        var result = await _repository.SearchAsync(criteria, ct);

        return new PublicPropertyPageResponse(
            query.Page,
            query.PageSize,
            result.Total,
            result.Items.Select(MapListItem).ToList());
    }

    private static PublicPropertySearchCriteria ValidateAndCreateCriteria(
        GetPublicPropertiesQuery query)
    {
        var errors = new Dictionary<string, string[]>();

        if (query.Page < 1)
            errors["page"] = ["La página debe ser mayor o igual a 1."];

        if (query.PageSize is < 1 or > MaximumPageSize)
            errors["pageSize"] = [$"El tamaño de página debe estar entre 1 y {MaximumPageSize}."];

        var operation = ParseEnum<ModalidadInmueble>(query.Operation, "operation", errors);
        var propertyType = ParseEnum<TipoInmueble>(query.PropertyType, "propertyType", errors);
        var sort = ParseSort(query.Sort, errors);

        ValidateRange(query.MinPrice, query.MaxPrice, "priceRange", "precio", errors);
        ValidateRange(query.MinArea, query.MaxArea, "areaRange", "área", errors);
        ValidateNonNegative(query.MinPrice, "minPrice", errors);
        ValidateNonNegative(query.MaxPrice, "maxPrice", errors);
        ValidateNonNegative(query.MinArea, "minArea", errors);
        ValidateNonNegative(query.MaxArea, "maxArea", errors);
        ValidateNonNegative(query.Bedrooms, "bedrooms", errors);
        ValidateNonNegative(query.Bathrooms, "bathrooms", errors);
        ValidateNonNegative(query.ParkingSpaces, "parkingSpaces", errors);

        if (errors.Count > 0)
            throw new PublicCatalogValidationException(errors);

        return new PublicPropertySearchCriteria
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Operation = operation,
            PropertyType = propertyType,
            Municipality = NormalizeFilter(query.Municipality),
            Neighborhood = NormalizeFilter(query.Neighborhood),
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            MinArea = query.MinArea,
            MaxArea = query.MaxArea,
            MinBedrooms = query.Bedrooms,
            MinBathrooms = query.Bathrooms,
            MinParkingSpaces = query.ParkingSpaces,
            Sort = sort
        };
    }

    private static TEnum? ParseEnum<TEnum>(
        string? value,
        string field,
        IDictionary<string, string[]> errors)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        var validName = Enum.GetNames<TEnum>()
            .Any(name => name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        if (!validName || !Enum.TryParse<TEnum>(trimmed, true, out var parsed))
        {
            errors[field] = [$"Valor inválido. Permitidos: {string.Join(", ", Enum.GetNames<TEnum>())}."];
            return null;
        }

        return parsed;
    }

    private static PublicPropertySort ParseSort(
        string? value,
        IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Equals("publishedAtDesc", StringComparison.OrdinalIgnoreCase))
            return PublicPropertySort.PublishedAtDescending;

        if (value.Equals("priceAsc", StringComparison.OrdinalIgnoreCase))
            return PublicPropertySort.PriceAscending;

        if (value.Equals("priceDesc", StringComparison.OrdinalIgnoreCase))
            return PublicPropertySort.PriceDescending;

        errors["sort"] = ["Orden inválido. Permitidos: priceAsc, priceDesc, publishedAtDesc."];
        return PublicPropertySort.PublishedAtDescending;
    }

    private static void ValidateRange<T>(
        T? minimum,
        T? maximum,
        string field,
        string label,
        IDictionary<string, string[]> errors)
        where T : struct, IComparable<T>
    {
        if (minimum.HasValue && maximum.HasValue &&
            minimum.Value.CompareTo(maximum.Value) > 0)
            errors[field] = [$"El mínimo de {label} no puede superar el máximo."];
    }

    private static void ValidateNonNegative<T>(
        T? value,
        string field,
        IDictionary<string, string[]> errors)
        where T : struct, IComparable<T>
    {
        if (value.HasValue && value.Value.CompareTo(default) < 0)
            errors[field] = ["El valor no puede ser negativo."];
    }

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PublicPropertyListItemResponse MapListItem(PublicPropertyListData item) =>
        new(
            item.Id,
            item.Slug,
            item.PublicTitle,
            item.PropertyType.ToString(),
            item.Operation.ToString(),
            new PublicMoneyResponse(item.PriceAmount, item.PriceCurrency),
            new PublicLocationResponse(
                item.Municipality,
                item.Neighborhood,
                item.ApproximateLatitude,
                item.ApproximateLongitude),
            item.AreaSquareMeters,
            item.Bedrooms,
            item.Bathrooms,
            item.ParkingSpaces,
            item.CoverPhoto is null
                ? null
                : new PublicCoverPhotoResponse(item.CoverPhoto.Url, item.CoverPhoto.AltText),
            item.PublishedAt);

    internal static PublicPhotoResponse MapPhoto(PublicPhotoData photo) =>
        new(photo.Url, photo.AltText, photo.Order, photo.IsCover);
}
