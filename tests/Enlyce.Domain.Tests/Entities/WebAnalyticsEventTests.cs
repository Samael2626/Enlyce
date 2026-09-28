using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Tests.Entities;

public sealed class WebAnalyticsEventTests
{
    [Fact]
    public void CreateStoresOnlyAnonymousFunnelData()
    {
        var occurredAt = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

        var analyticsEvent = WebAnalyticsEvent.Create(
            new string('a', 64),
            WebAnalyticsEventType.Favorite,
            "/inmuebles/apartamento-laureles",
            "apartamento-laureles",
            occurredAt);

        Assert.Equal(WebAnalyticsEventType.Favorite, analyticsEvent.EventType);
        Assert.Equal(new string('a', 64), analyticsEvent.SessionHash);
        Assert.Equal(occurredAt, analyticsEvent.OccurredAt);
    }

    [Theory]
    [InlineData("", "/inmuebles")]
    [InlineData("abc", "/inmuebles")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "inmuebles")]
    public void CreateRejectsInvalidIdentifiersOrPaths(string sessionHash, string path)
    {
        Assert.Throws<DomainError>(() => WebAnalyticsEvent.Create(
            sessionHash,
            WebAnalyticsEventType.PageView,
            path,
            null,
            DateTime.UtcNow));
    }
}
