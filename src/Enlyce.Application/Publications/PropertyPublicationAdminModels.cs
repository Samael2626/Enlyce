namespace Enlyce.Application.Publications;

public sealed record PublicationAdminPhotoData(
    string Url,
    string AltText,
    int Order,
    bool IsCover);

public sealed record PropertyPublicationAdminData(
    Guid Id,
    Guid PropertyId,
    string PropertyName,
    string PropertyType,
    string Operation,
    Guid AdvisorId,
    string AdvisorName,
    string Slug,
    string PublicTitle,
    string PublicDescription,
    string Status,
    decimal? PriceAmount,
    string? PriceCurrency,
    string? Municipality,
    string? Neighborhood,
    decimal? ApproximateLatitude,
    decimal? ApproximateLongitude,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    IReadOnlyList<PublicationAdminPhotoData> Photos);

public sealed record PublicationPropertyOption(
    Guid Id,
    string Name,
    string PropertyType,
    string Operation,
    string Municipality,
    string? Neighborhood,
    decimal PriceAmount,
    string PriceCurrency);

public sealed record PublicationAdvisorOption(Guid Id, string Name);

public sealed record PropertyPublicationOptions(
    IReadOnlyList<PublicationPropertyOption> Properties,
    IReadOnlyList<PublicationAdvisorOption> Advisors);

public sealed record PropertyPublicationMutationResponse(Guid Id, string Status);
