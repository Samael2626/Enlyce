using System.Security.Cryptography;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using ImageMagick;

namespace Enlyce.Infrastructure.Media;

public sealed record ProcessedVariant(int Width, int Height, byte[] Content);

public sealed record ProcessedImage(
    string Fingerprint,
    int OriginalWidth,
    int OriginalHeight,
    IReadOnlyList<ProcessedVariant> Variants);

public sealed class ImageVariantProcessor
{
    static ImageVariantProcessor()
    {
        ResourceLimits.Width = MediaConstraints.MaxDimension;
        ResourceLimits.Height = MediaConstraints.MaxDimension;
        ResourceLimits.ListLength = 16;
        ResourceLimits.Memory = 256UL * 1024 * 1024;
        ResourceLimits.Disk = 0;
        ResourceLimits.Thread = 2;
    }

    public async Task<ProcessedImage> ProcessAsync(
        MediaUpload upload,
        IReadOnlyCollection<int> variantWidths,
        int quality,
        CancellationToken ct = default)
    {
        var widths = variantWidths.Where(width => width > 0).Distinct().OrderByDescending(width => width).ToArray();
        if (widths.Length == 0)
            throw new InvalidOperationException("MediaStorage:VariantWidths no puede estar vacio.");

        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int count;
        while ((count = await upload.Content.ReadAsync(chunk, ct)) != 0)
        {
            if (buffer.Length + count > MediaConstraints.MaxSizeBytes)
                MediaConstraints.ValidateDescriptor(upload.ContentType, buffer.Length + count);
            buffer.Write(chunk, 0, count);
        }

        MediaConstraints.ValidateDescriptor(upload.ContentType, buffer.Length);
        var bytes = buffer.ToArray();
        if (!HasDeclaredFormat(bytes, upload.ContentType))
            throw new DomainError("El contenido de la imagen no coincide con su tipo declarado.");

        MagickImage image;
        try
        {
            image = new MagickImage(bytes, new MagickReadSettings { FrameIndex = 0, FrameCount = 1 });
        }
        catch (MagickException)
        {
            throw new DomainError("La imagen esta corrupta o no se puede leer.");
        }

        try
        {
            using (image)
            {
                if (!MatchesFormat(image.Format, upload.ContentType))
                    throw new DomainError("El contenido de la imagen no coincide con su tipo declarado.");
                MediaConstraints.ValidateDimensions((int)image.Width, (int)image.Height);
                var fingerprint = Convert.ToHexString(SHA256.HashData(bytes))[..16].ToLowerInvariant();
                image.AutoOrient();

                var variants = new List<ProcessedVariant>(widths.Length);
                foreach (var width in widths)
                {
                    ct.ThrowIfCancellationRequested();
                    using var variant = new MagickImage(image);
                    var target = Math.Min(width, (int)image.Width);
                    if (target < image.Width)
                        variant.Resize((uint)target, 0);
                    variant.Quality = (uint)Math.Clamp(quality, 1, 100);
                    variant.Settings.SetDefine(MagickFormat.WebP, "lossless", false);
                    variants.Add(new ProcessedVariant((int)variant.Width, (int)variant.Height,
                        variant.ToByteArray(MagickFormat.WebP)));
                }

                return new ProcessedImage(fingerprint, (int)image.Width, (int)image.Height, variants);
            }
        }
        catch (MagickException)
        {
            throw new DomainError("La imagen esta corrupta o no se puede procesar.");
        }
    }

    private static bool MatchesFormat(MagickFormat format, string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => format is MagickFormat.Jpeg or MagickFormat.Jpg,
            "image/png" => format == MagickFormat.Png,
            "image/webp" => format == MagickFormat.WebP,
            _ => false
        };

    private static bool HasDeclaredFormat(ReadOnlySpan<byte> data, string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => data.Length >= 3 && data[0] == 0xff && data[1] == 0xd8 && data[2] == 0xff,
            "image/png" => data.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "image/webp" => data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) &&
                            data.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
}
