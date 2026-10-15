using Enlyce.Domain.Entities;

namespace Enlyce.Application.Analytics;

public sealed record RealizedDealsReportFilter(
    DateOnly? From,
    DateOnly? Through,
    Guid? AdvisorId,
    RealizedDealOperation? Operation,
    TipoInmueble? PropertyType,
    int Page,
    int PageSize);

public sealed record RealizedDealsReportPage(
    int Total,
    int Page,
    int PageSize,
    IReadOnlyList<RealizedDealsReportTotal> TotalsByCurrency,
    IReadOnlyList<RealizedDealReportItem> Items);

public sealed record RealizedDealsReportTotal(string Currency, int Count, decimal GrossAmount, decimal CommissionAmount);

public sealed record RealizedDealReportItem(
    Guid Id,
    string PropertyName,
    string PropertyType,
    string Operation,
    DateOnly ClosedOn,
    string AdvisorName,
    string ClientName,
    decimal GrossAmount,
    decimal CommissionAmount,
    string Currency);

public interface IRealizedDealsReportReadRepository
{
    Task<RealizedDealsReportPage> GetAsync(RealizedDealsReportFilter filter, CancellationToken ct = default);
}
