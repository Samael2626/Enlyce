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
}
