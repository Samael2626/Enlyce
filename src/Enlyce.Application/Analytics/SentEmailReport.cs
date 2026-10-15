using Enlyce.Domain.Entities;

namespace Enlyce.Application.Analytics;

public sealed record SentEmailReportFilter(DateOnly? From, DateOnly? Through, int Page = 1, int PageSize = 10);

public sealed record SentEmailReportRow(
    Guid Id,
    DateTime AttemptedAt,
    string RecipientAddress,
    string Subject,
    string Description,
    string SenderName,
    EmailDeliveryStatus Status,
    string? FailureCode);

public sealed record SentEmailReportPage(
    int Total,
    int Page,
    int PageSize,
    IReadOnlyList<SentEmailReportRow> Items);

public interface ISentEmailReportReadRepository
{
    Task<SentEmailReportPage> GetAsync(SentEmailReportFilter filter, CancellationToken ct = default);
}
