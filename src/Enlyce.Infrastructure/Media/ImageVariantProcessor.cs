using System.Security.Cryptography;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using SkiaSharp;

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

        // Se leen las dimensiones del encabezado antes de decodificar los pixeles:
        // una imagen enorme se rechaza sin reservar su memoria.
        buffer.Position = 0;
        using var codec = OpenCodec(buffer);
        MediaConstraints.ValidateDimensions(codec.Info.Width, codec.Info.Height);
        using var image = Decode(codec);

        var fingerprint = Convert.ToHexString(SHA256.HashData(buffer.ToArray()))[..16].ToLowerInvariant();

        // Skia codifica WebP con perdida cuando la calidad es menor que 100.
        var webpQuality = Math.Clamp(quality, 1, 99);
        var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);

        var variants = new List<ProcessedVariant>(widths.Length);
        foreach (var width in widths)
        {
            ct.ThrowIfCancellationRequested();

            // Preserva la relacion de aspecto y nunca agranda la original.
            var targetWidth = Math.Min(width, image.Width);
            var targetHeight = Math.Max(1, (int)Math.Round((double)image.Height * targetWidth / image.Width));
            using var variant = targetWidth == image.Width
                ? image.Copy()
                : image.Resize(new SKImageInfo(targetWidth, targetHeight, image.ColorType, image.AlphaType), sampling);
            if (variant is null)
                throw new DomainError("La imagen esta corrupta o no se puede leer.");

            using var data = variant.Encode(SKEncodedImageFormat.Webp, webpQuality);
            variants.Add(new ProcessedVariant(variant.Width, variant.Height, data.ToArray()));
        }

        return new ProcessedImage(fingerprint, image.Width, image.Height, variants);
    }

    private static SKCodec OpenCodec(Stream content)
    {
        var codec = SKCodec.Create(new SKManagedStream(content, disposeManagedStream: false));
        return codec ?? throw new DomainError("El archivo no es una imagen valida.");
    }

    // Decodifica solo el primer cuadro (GIF/WebP animados incluidos).
    private static SKBitmap Decode(SKCodec codec)
    {
        var info = codec.Info.WithColorType(SKColorType.Rgba8888).WithAlphaType(SKAlphaType.Premul);
        var bitmap = new SKBitmap(info);
        var result = codec.GetPixels(info, bitmap.GetPixels());
        if (result == SKCodecResult.Success)
            return bitmap;

        bitmap.Dispose();
        throw new DomainError("La imagen esta corrupta o no se puede leer.");
    }
}
