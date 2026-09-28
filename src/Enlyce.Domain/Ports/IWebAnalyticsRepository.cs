using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public sealed record WebAnalyticsCount(WebAnalyticsEventType EventType, int UniqueSessions, int TotalEvents);

public interface IWebAnalyticsRepository
{
    Task AddAsync(WebAnalyticsEvent analyticsEvent, CancellationToken ct = default);
    Task<IReadOnlyList<WebAnalyticsCount>> GetFunnelAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}
