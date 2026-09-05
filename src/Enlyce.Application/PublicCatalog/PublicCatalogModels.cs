using Enlyce.Domain.Entities;

namespace Enlyce.Application.PublicCatalog;

public enum PublicPropertySort
{
    PublishedAtDescending = 0,
    PriceAscending = 1,
    PriceDescending = 2
}

public sealed record PublicPropertySearchCriteria
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public ModalidadInmueble? Operation { get; init; }
    public TipoInmueble? PropertyType { get; init; }
    public string? Municipality { get; init; }
    public string? Neighborhood { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinArea { get; init; }
    public int? MaxArea { get; init; }
    public int? MinBedrooms { get; init; }
    public int? MinBathrooms { get; init; }
    public int? MinParkingSpaces { get; init; }
    public PublicPropertySort Sort { get; init; } = PublicPropertySort.PublishedAtDescending;
}

public sealed record PublicPhotoData(
    string Url,
    string AltText,
    int Order,
    bool IsCover);

public sealed record PublicPropertyListData(
    Guid Id,
    string Slug,
    string PublicTitle,
    TipoInmueble PropertyType,
    ModalidadInmueble Operation,
    decimal PriceAmount,
    string PriceCurrency,
    string Municipality,
    string Neighborhood,
    decimal ApproximateLatitude,
    decimal ApproximateLongitude,
    int AreaSquareMeters,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpaces,
    PublicPhotoData? CoverPhoto,
    DateTime PublishedAt);

public sealed record PublicPropertyDetailData(
    Guid Id,
    string Slug,
    string PublicTitle,
    string PublicDescription,
    TipoInmueble PropertyType,
    ModalidadInmueble Operation,
    decimal PriceAmount,
    string PriceCurrency,
    string Municipality,
    string Neighborhood,
    decimal ApproximateLatitude,
    decimal ApproximateLongitude,
    int AreaSquareMeters,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpaces,
    IReadOnlyList<PublicPhotoData> Photos,
    Guid AdvisorId,
    string AdvisorDisplayName,
    DateTime PublishedAt);

public sealed record PublicPropertyPageData(
    int Total,
    IReadOnlyList<PublicPropertyListData> Items);

public sealed record PublicMoneyResponse(decimal Amount, string Currency);

public sealed record PublicLocationResponse(
    string Municipality,
    string Neighborhood,
    decimal ApproximateLatitude,
    decimal ApproximateLongitude);

public sealed record PublicPhotoResponse(
    string Url,
    string AltText,
    int Order,
    bool IsCover);

public sealed record PublicCoverPhotoResponse(
    string Url,
    string AltText);

public sealed record PublicAdvisorResponse(
    Guid Id,
    string DisplayName,
    string? PublicPhone);

public sealed record PublicPropertyFeaturesResponse(
    int AreaSquareMeters,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpaces,
    int? Stratum,
    IReadOnlyList<string> Amenities);

public sealed record PublicPropertyListItemResponse(
    Guid Id,
    string Slug,
    string PublicTitle,
    string PropertyType,
    string Operation,
    PublicMoneyResponse Price,
    PublicLocationResponse Location,
    int AreaSquareMeters,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpaces,
    PublicCoverPhotoResponse? CoverPhoto,
    DateTime PublishedAt);

public sealed record PublicPropertyPageResponse(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<PublicPropertyListItemResponse> Items);

public sealed record PublicPropertyDetailResponse(
    Guid Id,
    string Slug,
    string PublicTitle,
    string PublicDescription,
    string PropertyType,
    string Operation,
    PublicMoneyResponse Price,
    PublicMoneyResponse? AdministrationFee,
    PublicLocationResponse Location,
    PublicPropertyFeaturesResponse Features,
    IReadOnlyList<PublicPhotoResponse> Photos,
    PublicAdvisorResponse Advisor,
    DateTime PublishedAt);
