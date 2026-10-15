namespace Enlyce.Application.Analytics;

public sealed record OwnerPerformanceReport(
    DateOnly From,
    DateOnly Through,
    IReadOnlyList<OwnerPerformanceRow> Owners);

public sealed record OwnerPerformanceRow(
    Guid OwnerId,
    string OwnerName,
    int PageVisits,
    int Calls,
    int Emails,
    int Events,
    int Visits,
    int ReceivedContacts);

public interface IOwnerPerformanceReportReadRepository
{
    Task<OwnerPerformanceReport> GetAsync(DateOnly from, DateOnly through, CancellationToken ct = default);
}
