namespace Enlyce.Application.Analytics;

public sealed record WebPropertyVisitsReport(
    DateOnly From,
    DateOnly Through,
    IReadOnlyList<WebPropertyVisitsRow> Properties);

public sealed record WebPropertyVisitsRow(Guid PublicationId, string Title, int Visits);

public interface IWebPropertyVisitsReportReadRepository
{
    Task<WebPropertyVisitsReport> GetAsync(DateOnly from, DateOnly through, CancellationToken ct = default);
}
