using System.Text.RegularExpressions;
using Sentry;

namespace Enlyce.Api.Observability;

// Envio de errores a Sentry. Solo se activa si existe Sentry:Dsn (secreto del
// entorno); sin DSN la API funciona igual y los errores quedan solo en logs.
// Cada LogError existente y cada excepcion no manejada se convierten en alerta.
public static partial class ErrorMonitoring
{
    public static WebApplicationBuilder AddErrorMonitoring(this WebApplicationBuilder builder)
    {
        var dsn = builder.Configuration["Sentry:Dsn"];
        if (string.IsNullOrWhiteSpace(dsn))
            return builder;

        builder.WebHost.UseSentry(options =>
        {
            options.Dsn = dsn;
            options.Environment = builder.Configuration["Sentry:Environment"] ?? builder.Environment.EnvironmentName;
            // Railway expone el commit desplegado; permite saber que version fallo.
            options.Release = Environment.GetEnvironmentVariable("RAILWAY_GIT_COMMIT_SHA");
            // Ley 1581: sin IP, cookies, cabeceras de autenticacion ni cuerpos de solicitud.
            options.SendDefaultPii = false;
            options.MaxRequestBodySize = Sentry.Extensibility.RequestSize.None;
            options.MinimumEventLevel = LogLevel.Error;
            options.MinimumBreadcrumbLevel = LogLevel.Information;
            options.TracesSampleRate = 0;
            options.SetBeforeSend((sentryEvent, _) => Scrub(sentryEvent));
            options.SetBeforeBreadcrumb((breadcrumb, _) => ScrubBreadcrumb(breadcrumb));
        });

        return builder;
    }

    // Quita datos que pueden identificar personas o abrir sesiones: la query
    // (p. ej. tokens de baja de alertas) y cualquier usuario adjunto.
    public static SentryEvent Scrub(SentryEvent sentryEvent)
    {
        sentryEvent.Request.QueryString = null;
        sentryEvent.Request.Cookies = null;
        sentryEvent.Request.Data = null;
        foreach (var header in sentryEvent.Request.Headers.Keys.ToList())
        {
            if (!SafeHeaders.Contains(header))
                sentryEvent.Request.Headers.Remove(header);
        }

        sentryEvent.User = new SentryUser();
        return sentryEvent;
    }

    // Los logs de ASP.NET ("Request starting GET /ruta?token=...") viajan como
    // migas de pan: se elimina la query de cualquier URL del mensaje.
    public static Breadcrumb ScrubBreadcrumb(Breadcrumb breadcrumb)
    {
        if (breadcrumb.Message is null || !breadcrumb.Message.Contains('?'))
            return breadcrumb;

        return new Breadcrumb(
            QueryPattern().Replace(breadcrumb.Message, string.Empty),
            breadcrumb.Type,
            breadcrumb.Data,
            breadcrumb.Category,
            breadcrumb.Level);
    }

    [GeneratedRegex(@"\?[^\s]*")]
    private static partial Regex QueryPattern();

    private static readonly HashSet<string> SafeHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Accept", "Content-Type", "Content-Length", "User-Agent", "Host"
    };
}
