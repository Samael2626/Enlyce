using Enlyce.Application.Abstractions;
using Enlyce.Application.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Analytics;

public static class AnalyticsModule
{
    public static void MapAnalytics(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/analytics/events", async (
            [FromBody] RecordWebAnalyticsEventRequest request,
            ICommandHandler<RecordWebAnalyticsEventCommand> handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(new RecordWebAnalyticsEventCommand(
                request.SessionId,
                request.Event,
                request.Path,
                request.PropertySlug), ct);
            return Results.Accepted();
        })
        .AllowAnonymous()
        .RequireRateLimiting("public-analytics")
        .WithMetadata(new RequestSizeLimitAttribute(8 * 1024))
        .WithName("RecordWebAnalyticsEvent")
        .Produces(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/funnel", async (
            DateTime? from,
            DateTime? to,
            IQueryHandler<GetWebAnalyticsFunnelQuery, WebAnalyticsFunnelResponse> handler,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var until = EnsureUtc(to ?? timeProvider.GetUtcNow().UtcDateTime);
            var since = EnsureUtc(from ?? until.AddDays(-30));
            var result = await handler.HandleAsync(new GetWebAnalyticsFunnelQuery(since, until), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("Administrador")
        .WithName("GetWebAnalyticsFunnel")
        .Produces<WebAnalyticsFunnelResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}

public sealed record RecordWebAnalyticsEventRequest(
    string SessionId,
    string Event,
    string Path,
    string? PropertySlug);
