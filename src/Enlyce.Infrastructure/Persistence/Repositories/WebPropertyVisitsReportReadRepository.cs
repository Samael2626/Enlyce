using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class WebPropertyVisitsReportReadRepository(EnlyceDbContext context)
    : IWebPropertyVisitsReportReadRepository
{
    public async Task<WebPropertyVisitsReport> GetAsync(DateOnly from, DateOnly through, CancellationToken ct = default)
    {
        var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = through.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var publications = await context.PropertyPublications.AsNoTracking()
            .Where(item => item.Status != PublicationStatus.Draft)
            .OrderBy(item => item.PublicTitle)
            .Select(item => new { item.Id, item.Slug, item.PublicTitle })
            .ToArrayAsync(ct);

        var visitsBySlug = await context.WebAnalyticsEvents.AsNoTracking()
            .Where(item => item.EventType == WebAnalyticsEventType.PageView &&
                           item.PropertySlug != null && item.OccurredAt >= start && item.OccurredAt < end)
            .GroupBy(item => item.PropertySlug!)
            .Select(group => new PageVisitsCount(group.Key, group.Select(item => item.SessionHash).Distinct().Count()))
            .ToDictionaryAsync(item => item.Slug, item => item.Count, ct);

        var rows = publications.Select(publication => new WebPropertyVisitsRow(
            publication.Id,
            publication.PublicTitle,
            visitsBySlug.GetValueOrDefault(publication.Slug))).ToArray();

        return new WebPropertyVisitsReport(from, through, rows);
    }

    private sealed record PageVisitsCount(string Slug, int Count);
}
