namespace Enlyce.Application.Analytics;

public sealed record AdvisorPerformanceReport(
    DateOnly From,
    DateOnly Through,
    DateOnly PreviousFrom,
    DateOnly PreviousThrough,
    IReadOnlyList<AdvisorPerformanceRow> Advisors);

public sealed record AdvisorPerformanceRow(
    Guid AdvisorId,
    string AdvisorName,
    int Interactions,
    int PreviousInteractions,
    int Visits,
    int PreviousVisits,
    int Sales,
    int PreviousSales,
    int Rentals,
    int PreviousRentals,
    int NewClients,
    int PreviousNewClients,
    int NewProperties,
    int PreviousNewProperties);

public interface IAdvisorPerformanceReportReadRepository
{
    Task<AdvisorPerformanceReport> GetAsync(DateOnly from, DateOnly through, CancellationToken ct = default);
    Task<AdvisorActivityDetailReport?> GetDetailAsync(Guid advisorId, DateOnly from, DateOnly through, CancellationToken ct = default);
}

public sealed record AdvisorActivityDetailReport(
    Guid AdvisorId,
    DateOnly From,
    DateOnly Through,
    IReadOnlyList<AdvisorInteractionDetail> Interactions,
    IReadOnlyList<AdvisorVisitDetail> Visits,
    IReadOnlyList<AdvisorDealDetail> Deals);

public sealed record AdvisorInteractionDetail(DateTime Date, string Type, string? Summary, string ClientName);
public sealed record AdvisorVisitDetail(DateTime ScheduledAt, DateTime? CompletedAt, string Status, string? Feedback, string ClientName, string PropertyName);
public sealed record AdvisorDealDetail(DateOnly ClosedOn, string Operation, string PropertyName, string ClientName, decimal GrossAmount, decimal CommissionAmount, string Currency);
