using Enlyce.Api.Observability;
using Sentry;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class ErrorMonitoringTests
{
    [Fact]
    public void Scrub_RemovesQueryCookiesBodyUserAndSensitiveHeaders()
    {
        var sentryEvent = new SentryEvent
        {
            User = new SentryUser { Email = "persona@example.com", IpAddress = "203.0.113.7" },
        };
        sentryEvent.Request.QueryString = "?id=1&token=secreto";
        sentryEvent.Request.Cookies = "enlyce_auth=abc";
        sentryEvent.Request.Data = new { correo = "persona@example.com" };
        sentryEvent.Request.Headers["Authorization"] = "Bearer abc";
        sentryEvent.Request.Headers["X-Forwarded-For"] = "203.0.113.7";
        sentryEvent.Request.Headers["User-Agent"] = "Prueba";

        var scrubbed = ErrorMonitoring.Scrub(sentryEvent);

        Assert.Null(scrubbed.Request.QueryString);
        Assert.Null(scrubbed.Request.Cookies);
        Assert.Null(scrubbed.Request.Data);
        Assert.Null(scrubbed.User.Email);
        Assert.Null(scrubbed.User.IpAddress);
        Assert.Equal(["User-Agent"], scrubbed.Request.Headers.Keys);
    }

    [Fact]
    public void ScrubBreadcrumb_RemovesQueryFromRequestLog()
    {
        var breadcrumb = new Breadcrumb(
            "Request starting HTTP/1.1 GET http://api/alertas/baja?id=1&token=secreto - - -",
            "default", category: "Microsoft.AspNetCore.Hosting.Diagnostics");

        var scrubbed = ErrorMonitoring.ScrubBreadcrumb(breadcrumb);

        Assert.Equal("Request starting HTTP/1.1 GET http://api/alertas/baja - - -", scrubbed.Message);
        Assert.Equal(breadcrumb.Category, scrubbed.Category);
    }
}
