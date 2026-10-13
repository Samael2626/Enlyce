using System.Security.Cryptography;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Enlyce.Infrastructure.Media;

// Variante ya codificada en WebP y lista para persistir en cualquier backend.
public sealed record ProcessedVariant(int Width, int Height, byte[] Content);

// Resultado del procesado: huella estable del original mas las variantes.
public sealed record ProcessedImage(
    string Fingerprint,
    int OriginalWidth,
    int OriginalHeight,
    IReadOnlyList<ProcessedVariant> Variants);

// Procesado de imagen compartido por los adaptadores de IMediaStorage.
// Decodifica, valida, calcula la huella y genera las variantes WebP en memoria;
// cada adaptador decide donde escribir los bytes (disco, S3, etc.).
public sealed class ImageVariantProcessor
{
    public async Task<ProcessedImage> ProcessAsync(
        MediaUpload upload,
        IReadOnlyCollection<int> variantWidths,
        int quality,
        CancellationToken ct = default)
    {
        var widths = variantWidths
            .Where(width => width > 0)
            .Distinct()
            .OrderByDescending(width => width)
            .ToArray();

        if (widths.Length == 0)
            throw new InvalidOperationException("MediaStorage:VariantWidths no puede estar vacio.");

        // Se materializa en memoria para poder hashear y decodificar sin depender
        // de que el Stream de origen admita seek.
        using var buffer = new MemoryStream();
        await upload.Content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        if (buffer.Length != upload.SizeBytes)
            MediaConstraints.ValidateDescriptor(upload.ContentType, buffer.Length);

        using var image = await LoadAsync(buffer, ct);
        MediaConstraints.ValidateDimensions(image.Width, image.Height);

        var fingerprint = Convert.ToHexString(SHA256.HashData(buffer.ToArray()))[..16].ToLowerInvariant();

        // Sin FileFormat explicito ImageSharp puede salir sin perdida y los
        // archivos se van a megabytes, que es justo lo que se quiere evitar.
        var encoder = new WebpEncoder
        {
            Quality = quality,
            FileFormat = WebpFileFormatType.Lossy
        };

        var variants = new List<ProcessedVariant>(widths.Length);
        foreach (var width in widths)
        {
            // Max preserva la relacion de aspecto y nunca agranda la original.
            var target = Math.Min(width, image.Width);
            using var variant = image.Clone(context => context.Resize(new ResizeOptions
            {
                Size = new Size(target, 0),
                Mode = ResizeMode.Max
            }));

            using var output = new MemoryStream();
            await variant.SaveAsWebpAsync(output, encoder, ct);
            variants.Add(new ProcessedVariant(variant.Width, variant.Height, output.ToArray()));
        }

        return new ProcessedImage(fingerprint, image.Width, image.Height, variants);
    }

    private static async Task<Image> LoadAsync(Stream content, CancellationToken ct)
    {
        try
        {
            return await Image.LoadAsync(new DecoderOptions { MaxFrames = 1 }, content, ct);
        }
        catch (UnknownImageFormatException)
        {
            throw new DomainError("El archivo no es una imagen valida.");
        }
        catch (InvalidImageContentException)
        {
            throw new DomainError("La imagen esta corrupta o no se puede leer.");
        }
    }
}
