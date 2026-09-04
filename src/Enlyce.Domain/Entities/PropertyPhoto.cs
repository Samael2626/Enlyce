using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class PropertyPhoto
{
    public Guid Id { get; private set; }
    public Guid PublicationId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string AltText { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public bool IsCover { get; private set; }

    private PropertyPhoto() { }

    private PropertyPhoto(
        Guid id,
        Guid publicationId,
        string url,
        string altText,
        int order,
        bool isCover)
    {
        Id = id;
        PublicationId = publicationId;
        Url = url;
        AltText = altText;
        Order = order;
        IsCover = isCover;
    }

    internal static PropertyPhoto Create(
        Guid publicationId,
        string url,
        string altText,
        int order,
        bool isCover)
    {
        Validate(publicationId, url, altText, order);

        return new PropertyPhoto(
            Guid.NewGuid(),
            publicationId,
            url.Trim(),
            altText.Trim(),
            order,
            isCover);
    }

    public static PropertyPhoto Reconstitute(
        Guid id,
        Guid publicationId,
        string url,
        string altText,
        int order,
        bool isCover) =>
        new(id, publicationId, url, altText, order, isCover);

    private static void Validate(Guid publicationId, string url, string altText, int order)
    {
        if (publicationId == Guid.Empty)
            throw new DomainError("La publicacion de la foto es obligatoria.");

        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new DomainError("La URL de la foto debe ser HTTP o HTTPS.");

        if (string.IsNullOrWhiteSpace(altText))
            throw new DomainError("El texto alternativo de la foto es obligatorio.");

        if (order < 0)
            throw new DomainError("El orden de la foto no puede ser negativo.");
    }
}
