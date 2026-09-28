using System.Security.Cryptography;
using System.Text;
using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Analytics;

public sealed record RecordWebAnalyticsEventCommand(
    string SessionId,
    string Event,
    string Path,
    string? PropertySlug);

public sealed class RecordWebAnalyticsEventHandler(
    IWebAnalyticsRepository repository,
    TimeProvider timeProvider) : ICommandHandler<RecordWebAnalyticsEventCommand>
{
    public async Task HandleAsync(
        RecordWebAnalyticsEventCommand command,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(command.SessionId, out var sessionId))
            throw new DomainError("La sesion de analitica no es valida.");

        var eventType = command.Event.Trim().ToLowerInvariant() switch
        {
            "page_view" => WebAnalyticsEventType.PageView,
            "search" => WebAnalyticsEventType.Search,
            "favorite" => WebAnalyticsEventType.Favorite,
            "form_started" => WebAnalyticsEventType.FormStarted,
            "conversion" => WebAnalyticsEventType.Conversion,
            _ => throw new DomainError("El evento de analitica no es valido.")
        };

        var sessionHash = Convert.ToHexStringLower(
            SHA256.HashData(Encoding.UTF8.GetBytes(sessionId.ToString("D"))));
        var analyticsEvent = WebAnalyticsEvent.Create(
            sessionHash,
            eventType,
            command.Path,
            command.PropertySlug,
            timeProvider.GetUtcNow().UtcDateTime);

        await repository.AddAsync(analyticsEvent, ct);
    }
}

public sealed record GetWebAnalyticsFunnelQuery(DateTime From, DateTime To);

public sealed record WebAnalyticsStepResponse(
    string Event,
    int UniqueSessions,
    int TotalEvents,
    decimal RateFromVisits);

public sealed record WebAnalyticsFunnelResponse(
    DateTime From,
    DateTime To,
    IReadOnlyList<WebAnalyticsStepResponse> Steps);

public sealed class GetWebAnalyticsFunnelHandler(IWebAnalyticsRepository repository)
    : IQueryHandler<GetWebAnalyticsFunnelQuery, WebAnalyticsFunnelResponse>
{
    private static readonly WebAnalyticsEventType[] OrderedSteps =
    [
        WebAnalyticsEventType.PageView,
        WebAnalyticsEventType.Search,
        WebAnalyticsEventType.Favorite,
        WebAnalyticsEventType.FormStarted,
        WebAnalyticsEventType.Conversion
    ];

    public async Task<WebAnalyticsFunnelResponse> HandleAsync(
        GetWebAnalyticsFunnelQuery query,
        CancellationToken ct = default)
    {
        if (query.From.Kind != DateTimeKind.Utc || query.To.Kind != DateTimeKind.Utc)
            throw new DomainError("El rango de analitica debe estar expresado en UTC.");
        if (query.From >= query.To || query.To - query.From > TimeSpan.FromDays(366))
            throw new DomainError("El rango de analitica no es valido.");

        var counts = await repository.GetFunnelAsync(query.From, query.To, ct);
        var byType = counts.ToDictionary(item => item.EventType);
        var visits = byType.GetValueOrDefault(WebAnalyticsEventType.PageView)?.UniqueSessions ?? 0;

        var steps = OrderedSteps.Select(eventType =>
        {
            var count = byType.GetValueOrDefault(eventType);
            var uniqueSessions = count?.UniqueSessions ?? 0;
            var rate = visits == 0 ? 0 : Math.Round(uniqueSessions * 100m / visits, 2);
            return new WebAnalyticsStepResponse(
                ToPublicName(eventType),
                uniqueSessions,
                count?.TotalEvents ?? 0,
                rate);
        }).ToArray();

        return new WebAnalyticsFunnelResponse(query.From, query.To, steps);
    }

    private static string ToPublicName(WebAnalyticsEventType eventType) => eventType switch
    {
        WebAnalyticsEventType.PageView => "page_view",
        WebAnalyticsEventType.Search => "search",
        WebAnalyticsEventType.Favorite => "favorite",
        WebAnalyticsEventType.FormStarted => "form_started",
        WebAnalyticsEventType.Conversion => "conversion",
        _ => throw new ArgumentOutOfRangeException(nameof(eventType))
    };
}
