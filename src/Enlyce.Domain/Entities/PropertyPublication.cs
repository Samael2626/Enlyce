using System.Globalization;
using System.Text;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;

namespace Enlyce.Domain.Entities;

public enum PublicationStatus
{
    Draft = 0,
    Published = 1,
    Paused = 2,
    Withdrawn = 3
}

public sealed class PropertyPublication
{
    private const int MinimumPhotos = 3;

    // Tres decimales son unos 111 m: cubren la manzana sin salirse del barrio.
    // Con cuatro (unos 11 m) la coordenada senala el portal del inmueble, y con
    // dos (1,1 km) en Medellin se cruza de comuna y el pin apunta al barrio
    // equivocado, que es enganar en la direccion contraria.
    private const int PublicCoordinateDecimals = 3;
    private readonly List<PropertyPhoto> _photos = [];

    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid AdvisorId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string PublicTitle { get; private set; } = string.Empty;
    public string PublicDescription { get; private set; } = string.Empty;
    public PublicationStatus Status { get; private set; }
    public decimal? PublicPriceAmount { get; private set; }
    public string? PublicPriceCurrency { get; private set; }
    public string? Municipality { get; private set; }
    public string? Neighborhood { get; private set; }
    public decimal? ApproximateLatitude { get; private set; }
    public decimal? ApproximateLongitude { get; private set; }
    public bool ExactAddressVisible { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public IReadOnlyCollection<PropertyPhoto> Photos => _photos;

    private PropertyPublication() { }

    private PropertyPublication(
        Guid id,
        Guid propertyId,
        Guid advisorId,
        string slug,
        string publicTitle,
        string publicDescription,
        PublicationStatus status,
        decimal? publicPriceAmount,
        string? publicPriceCurrency,
        string? municipality,
        string? neighborhood,
        decimal? approximateLatitude,
        decimal? approximateLongitude,
        bool exactAddressVisible,
        DateTime createdAt,
        DateTime? publishedAt,
        IEnumerable<PropertyPhoto> photos)
    {
        Id = id;
        PropertyId = propertyId;
        AdvisorId = advisorId;
        Slug = slug;
        PublicTitle = publicTitle;
        PublicDescription = publicDescription;
        Status = status;
        PublicPriceAmount = publicPriceAmount;
        PublicPriceCurrency = publicPriceCurrency;
        Municipality = municipality;
        Neighborhood = neighborhood;
        ApproximateLatitude = approximateLatitude;
        ApproximateLongitude = approximateLongitude;
        ExactAddressVisible = exactAddressVisible;
        CreatedAt = createdAt;
        PublishedAt = publishedAt;
        _photos.AddRange(photos);
    }

    public static PropertyPublication Create(
        Guid propertyId,
        Guid advisorId,
        string slug,
        string publicTitle,
        string? publicDescription)
    {
        if (propertyId == Guid.Empty)
            throw new DomainError("El inmueble es obligatorio.");

        if (advisorId == Guid.Empty)
            throw new DomainError("El asesor responsable es obligatorio.");

        if (string.IsNullOrWhiteSpace(publicTitle))
            throw new DomainError("El titulo publico es obligatorio.");

        var trimmedTitle = publicTitle.Trim();
        if (trimmedTitle.Length > 200)
            throw new DomainError("El titulo publico no puede superar 200 caracteres.");

        var trimmedDescription = publicDescription?.Trim() ?? string.Empty;
        if (trimmedDescription.Length > 4_000)
            throw new DomainError("La descripcion publica no puede superar 4000 caracteres.");

        return new PropertyPublication(
            Guid.NewGuid(),
            propertyId,
            advisorId,
            NormalizeSlug(slug),
            trimmedTitle,
            trimmedDescription,
            PublicationStatus.Draft,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            DateTime.UtcNow,
            null,
            []);
    }

    public static PropertyPublication Reconstitute(
        Guid id,
        Guid propertyId,
        Guid advisorId,
        string slug,
        string publicTitle,
        string publicDescription,
        PublicationStatus status,
        decimal? publicPriceAmount,
        string? publicPriceCurrency,
        string? municipality,
        string? neighborhood,
        decimal? approximateLatitude,
        decimal? approximateLongitude,
        bool exactAddressVisible,
        DateTime createdAt,
        DateTime? publishedAt,
        IEnumerable<PropertyPhoto> photos) =>
        new(
            id,
            propertyId,
            advisorId,
            slug,
            publicTitle,
            publicDescription,
            status,
            publicPriceAmount,
            publicPriceCurrency,
            municipality,
            neighborhood,
            // Red de seguridad para filas guardadas antes de que el redondeo
            // existiera. No sustituye a la migracion que limpia la tabla: solo
            // evita que una fila vieja se reexponga con precision de portal.
            RoundPublicCoordinate(approximateLatitude),
            RoundPublicCoordinate(approximateLongitude),
            exactAddressVisible,
            createdAt,
            publishedAt,
            photos);

    public void SetPublicPrice(decimal amount, string currency = "COP")
    {
        EnsureNotWithdrawn();

        if (amount <= 0)
            throw new DomainError("El precio publico debe ser mayor a 0.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new DomainError("La moneda publica debe tener tres caracteres.");

        PublicPriceAmount = amount;
        PublicPriceCurrency = currency.Trim().ToUpperInvariant();
    }

    public void SetPublicLocation(
        string municipality,
        string neighborhood,
        decimal approximateLatitude,
        decimal approximateLongitude)
    {
        EnsureNotWithdrawn();

        if (string.IsNullOrWhiteSpace(municipality))
            throw new DomainError("El municipio publico es obligatorio.");

        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new DomainError("El barrio o sector publico es obligatorio.");

        if (approximateLatitude is < -90 or > 90)
            throw new DomainError("La latitud aproximada no es valida.");

        if (approximateLongitude is < -180 or > 180)
            throw new DomainError("La longitud aproximada no es valida.");

        Municipality = municipality.Trim();
        Neighborhood = neighborhood.Trim();
        // El redondeo vive aqui para que el invariante valga sin importar quien
        // llame: CRM, seeder o un importador futuro. Si dependiera de que cada
        // caller recuerde redondear, la fuga volveria en el primer descuido.
        ApproximateLatitude = RoundPublicCoordinate(approximateLatitude);
        ApproximateLongitude = RoundPublicCoordinate(approximateLongitude);
    }

    private static decimal RoundPublicCoordinate(decimal value) =>
        Math.Round(value, PublicCoordinateDecimals, MidpointRounding.AwayFromZero);

    private static decimal? RoundPublicCoordinate(decimal? value) =>
        value.HasValue ? RoundPublicCoordinate(value.Value) : null;

    public void AddPhoto(string url, string altText, int order, bool isCover = false)
    {
        EnsureNotWithdrawn();
        MediaConstraints.ValidatePhotoCount(_photos.Count);

        if (_photos.Any(photo => photo.Order == order))
            throw new DomainError("El orden de la foto ya existe en la publicacion.");

        if (isCover && _photos.Any(photo => photo.IsCover))
            throw new DomainError("La publicacion ya tiene una foto de portada.");

        _photos.Add(PropertyPhoto.Create(Id, url, altText, order, isCover));
    }

    public void Publish()
    {
        if (Status is not PublicationStatus.Draft and not PublicationStatus.Paused)
            throw new DomainError($"No se puede publicar desde estado {Status}.");

        ValidatePublishingRequirements();

        Status = PublicationStatus.Published;
        PublishedAt ??= DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != PublicationStatus.Published)
            throw new DomainError($"No se puede pausar desde estado {Status}.");

        Status = PublicationStatus.Paused;
    }

    public void Withdraw()
    {
        if (Status is not PublicationStatus.Published and not PublicationStatus.Paused)
            throw new DomainError($"No se puede retirar desde estado {Status}.");

        Status = PublicationStatus.Withdrawn;
    }

    private void ValidatePublishingRequirements()
    {
        if (PublicPriceAmount is null or <= 0 || string.IsNullOrWhiteSpace(PublicPriceCurrency))
            throw new DomainError("La publicacion requiere un precio publico positivo.");

        if (string.IsNullOrWhiteSpace(Municipality) ||
            string.IsNullOrWhiteSpace(Neighborhood) ||
            ApproximateLatitude is null ||
            ApproximateLongitude is null)
            throw new DomainError("La publicacion requiere una ubicacion publica completa.");

        if (_photos.Count < MinimumPhotos)
            throw new DomainError($"La publicacion requiere al menos {MinimumPhotos} fotos.");

        if (_photos.Count(photo => photo.IsCover) != 1)
            throw new DomainError("La publicacion requiere exactamente una foto de portada.");
    }

    private void EnsureNotWithdrawn()
    {
        if (Status == PublicationStatus.Withdrawn)
            throw new DomainError("Una publicacion retirada no puede modificarse.");
    }

    private static string NormalizeSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainError("El slug es obligatorio.");

        var normalized = slug.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var pendingSeparator = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (pendingSeparator && builder.Length > 0)
                    builder.Append('-');

                builder.Append(character);
                pendingSeparator = false;
            }
            else
            {
                pendingSeparator = true;
            }
        }

        if (builder.Length == 0)
            throw new DomainError("El slug no contiene caracteres validos.");

        if (builder.Length > 200)
            throw new DomainError("El slug no puede superar 200 caracteres.");

        return builder.ToString();
    }
}
