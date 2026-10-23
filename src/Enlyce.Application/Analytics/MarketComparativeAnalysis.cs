using Enlyce.Domain.Entities;

namespace Enlyce.Application.Analytics;

public sealed record MarketAnalysisFilter(
    string? City = null,
    string? Neighborhood = null,
    TipoInmueble? PropertyType = null,
    ModalidadInmueble? Operation = null,
    int? TargetAreaM2 = null,
    int? Bedrooms = null,
    int? Bathrooms = null);

public sealed record MarketInventoryMetrics(
    int SampleSize,
    decimal AveragePrice,
    decimal MedianPrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal AveragePricePerM2,
    decimal MedianPricePerM2,
    decimal MinPricePerM2,
    decimal MaxPricePerM2,
    double AverageDaysOnMarket);

public sealed record MarketClosedDealsMetrics(
    int SampleSize,
    decimal AverageGrossAmount,
    decimal MedianGrossAmount,
    decimal AveragePricePerM2,
    decimal MedianPricePerM2,
    double AverageDaysToClose,
    decimal? AverageDiscountPercentage);

public sealed record MarketValuationEstimate(
    int TargetAreaM2,
    decimal ReferencePricePerM2,
    decimal LowEstimate,
    decimal TargetEstimate,
    decimal HighEstimate,
    string ConfidenceLevel,
    string Explanation);

public sealed record MarketComparableActiveProperty(
    Guid Id,
    string Title,
    string PropertyType,
    string Operation,
    string City,
    string? Neighborhood,
    int AreaM2,
    int Bedrooms,
    int Bathrooms,
    decimal AskingPrice,
    decimal PricePerM2,
    int DaysOnMarket,
    string Currency);

public sealed record MarketComparableClosedDeal(
    Guid DealId,
    Guid PropertyId,
    string Title,
    string PropertyType,
    string Operation,
    string City,
    string? Neighborhood,
    int AreaM2,
    int Bedrooms,
    int Bathrooms,
    decimal ClosedPrice,
    decimal PricePerM2,
    int DaysToClose,
    DateOnly ClosedOn,
    string Currency);

public sealed record MarketComparativeAnalysisResult(
    MarketAnalysisFilter Filter,
    MarketInventoryMetrics ActiveMetrics,
    MarketClosedDealsMetrics ClosedMetrics,
    MarketValuationEstimate? ValuationEstimate,
    IReadOnlyList<MarketComparableActiveProperty> ActiveComparables,
    IReadOnlyList<MarketComparableClosedDeal> ClosedComparables);

public interface IMarketAnalysisReadRepository
{
    Task<MarketComparativeAnalysisResult> AnalyzeAsync(MarketAnalysisFilter filter, CancellationToken ct = default);
}
