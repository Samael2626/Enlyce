using Enlyce.Application.Analytics;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class SentEmailReportReadRepository(EnlyceDbContext context) : ISentEmailReportReadRepository
{
    public async Task<SentEmailReportPage> GetAsync(SentEmailReportFilter filter, CancellationToken ct = default)
    {
        var query = context.EmailDeliveryLogs.AsNoTracking();
        if (filter.From is { } from)
        {
            var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(item => item.AttemptedAt >= start);
        }
        if (filter.Through is { } through)
        {
            var end = through.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(item => item.AttemptedAt < end);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(item => item.AttemptedAt)
            .ThenByDescending(item => item.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(item => new SentEmailReportRow(
                item.Id,
                item.AttemptedAt,
                item.RecipientAddress,
                item.Subject,
                item.Description,
                item.SenderName,
                item.Status,
                item.FailureCode))
            .ToArrayAsync(ct);

        return new SentEmailReportPage(total, filter.Page, filter.PageSize, items);
    }
}
