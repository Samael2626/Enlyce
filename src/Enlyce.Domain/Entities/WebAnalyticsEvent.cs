using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public enum WebAnalyticsEventType
{
    PageView = 1,
    Search = 2,
    Favorite = 3,
    FormStarted = 4,
    Conversion = 5
}

public sealed class WebAnalyticsEvent
{
    public Guid Id { get; private set; }
    public string SessionHash { get; private set; } = string.Empty;
    public WebAnalyticsEventType EventType { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public string? PropertySlug { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private WebAnalyticsEvent() { }

    public static WebAnalyticsEvent Create(
        string sessionHash,
        WebAnalyticsEventType eventType,
        string path,
        string? propertySlug,
        DateTime occurredAt)
    {
        if (sessionHash.Length != 64 || sessionHash.Any(character => !Uri.IsHexDigit(character)))
            throw new DomainError("La sesion de analitica no es valida.");
        if (!Enum.IsDefined(eventType))
            throw new DomainError("El evento de analitica no es valido.");
        if (string.IsNullOrWhiteSpace(path) || !path.Trim().StartsWith('/'))
            throw new DomainError("La ruta de analitica no es valida.");

        var normalizedPath = path.Trim();
        if (normalizedPath.Length > 200)
            throw new DomainError("La ruta de analitica no puede superar 200 caracteres.");

        var normalizedSlug = string.IsNullOrWhiteSpace(propertySlug) ? null : propertySlug.Trim();
        if (normalizedSlug?.Length > 160)
            throw new DomainError("El inmueble de analitica no es valido.");

        return new WebAnalyticsEvent
        {
            Id = Guid.NewGuid(),
            SessionHash = sessionHash.ToLowerInvariant(),
            EventType = eventType,
            Path = normalizedPath,
            PropertySlug = normalizedSlug,
            OccurredAt = occurredAt
        };
    }
}
