using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Media;

// Reglas de los medios de una publicacion. Viven en Domain porque definen que
// es publicable, no como se guarda el archivo.
public static class MediaConstraints
{
    public const long MaxSizeBytes = 8L * 1024 * 1024;
    public const int MinWidth = 800;
    public const int MinHeight = 600;
    public const int MaxDimension = 8_000;
    public const int MaxPhotosPerPublication = 20;

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public static IReadOnlyCollection<string> SupportedContentTypes => AllowedContentTypes;

    // Lo verificable antes de decodificar: tipo declarado y peso.
    public static void ValidateDescriptor(string contentType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType.Trim()))
            throw new DomainError(
                $"El tipo de imagen no esta permitido. Use {string.Join(", ", AllowedContentTypes)}.");

        if (sizeBytes <= 0)
            throw new DomainError("El archivo de imagen esta vacio.");

        if (sizeBytes > MaxSizeBytes)
            throw new DomainError($"La imagen supera el maximo de {MaxSizeBytes / (1024 * 1024)} MB.");
    }

    // Solo comprobable tras decodificar; la llama el adaptador de almacenamiento.
    public static void ValidateDimensions(int width, int height)
    {
        if (width < MinWidth || height < MinHeight)
            throw new DomainError($"La imagen debe medir al menos {MinWidth}x{MinHeight} pixeles.");

        if (width > MaxDimension || height > MaxDimension)
            throw new DomainError($"La imagen no puede superar {MaxDimension} pixeles por lado.");
    }

    public static void ValidatePhotoCount(int currentCount)
    {
        if (currentCount >= MaxPhotosPerPublication)
            throw new DomainError(
                $"La publicacion no admite mas de {MaxPhotosPerPublication} fotos.");
    }
}
