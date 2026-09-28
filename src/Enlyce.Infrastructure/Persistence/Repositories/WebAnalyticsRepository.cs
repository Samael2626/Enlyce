using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class WebAnalyticsRepository(EnlyceDbContext context) : IWebAnalyticsRepository
{
    public async Task AddAsync(WebAnalyticsEvent analyticsEvent, CancellationToken ct = default)
    {
        context.WebAnalyticsEvents.Add(analyticsEvent);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<WebAnalyticsCount>> GetFunnelAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default) =>
        await context.WebAnalyticsEvents
            .AsNoTracking()
            .Where(analyticsEvent => analyticsEvent.OccurredAt >= from && analyticsEvent.OccurredAt < to)
            .GroupBy(analyticsEvent => analyticsEvent.EventType)
            .Select(group => new WebAnalyticsCount(
                group.Key,
                group.Select(analyticsEvent => analyticsEvent.SessionHash).Distinct().Count(),
                group.Count()))
            .ToListAsync(ct);
}
