using System.Security.Cryptography;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Enlyce.Infrastructure.Media;

// Adaptador inicial: escribe variantes WebP en disco y las sirve por static
// files. Cambiar a S3 u otro proveedor solo implica otra implementacion del
// puerto; Domain y Application no se enteran.
public sealed class LocalMediaStorage : IMediaStorage
{
    private readonly MediaStorageOptions _options;

    public LocalMediaStorage(IOptions<MediaStorageOptions> options)
    {
        _options = options.Value;

        if (_options.VariantWidths.Length == 0)
            throw new InvalidOperationException("MediaStorage:VariantWidths no puede estar vacio.");
    }

    public async Task<StoredMedia> StoreAsync(MediaUpload upload, CancellationToken ct = default)
    {
        // Se materializa en memoria para poder hashear y decodificar sin depender
        // de que el Stream de origen admita seek.
        using var buffer = new MemoryStream();
        await upload.Content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        if (buffer.Length != upload.SizeBytes)
            MediaConstraints.ValidateDescriptor(upload.ContentType, buffer.Length);

        using var image = await LoadAsync(buffer, ct);
        MediaConstraints.ValidateDimensions(image.Width, image.Height);

        buffer.Position = 0;
        var fingerprint = Convert.ToHexString(SHA256.HashData(buffer.ToArray()))[..16].ToLowerInvariant();

        var folder = Path.Combine(_options.RootPath, "publicaciones", upload.PublicationId.ToString());
        Directory.CreateDirectory(folder);

        var widths = _options.VariantWidths
            .Where(width => width > 0)
            .Distinct()
            .OrderByDescending(width => width)
            .ToArray();

        // Sin FileFormat explicito ImageSharp puede salir sin perdida y los
        // archivos se van a megabytes, que es justo lo que se quiere evitar.
        var encoder = new WebpEncoder
        {
            Quality = _options.Quality,
            FileFormat = WebpFileFormatType.Lossy
        };
        var variants = new List<MediaVariant>(widths.Length);

        foreach (var width in widths)
        {
            // Max preserva la relacion de aspecto y nunca agranda la original.
            var target = Math.Min(width, image.Width);
            using var variant = image.Clone(context => context.Resize(new ResizeOptions
            {
                Size = new Size(target, 0),
                Mode = ResizeMode.Max
            }));

            var fileName = $"{fingerprint}-{target}.webp";
            var path = Path.Combine(folder, fileName);

            await using (var output = File.Create(path))
                await variant.SaveAsWebpAsync(output, encoder, ct);

            variants.Add(new MediaVariant(BuildUrl(upload.PublicationId, fileName), target));
        }

        var canonical = variants[0];
        var scale = (double)canonical.Width / image.Width;

        return new StoredMedia(
            canonical.Url,
            canonical.Width,
            (int)Math.Round(image.Height * scale),
            variants);
    }

    public Task RemoveAsync(Guid publicationId, string url, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(new Uri(url, UriKind.RelativeOrAbsolute).LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
            return Task.CompletedTask;

        var folder = Path.Combine(_options.RootPath, "publicaciones", publicationId.ToString());
        var prefix = fileName[..fileName.LastIndexOf('-')];

        // Se borran todas las variantes que comparten huella, no solo la URL dada.
        if (Directory.Exists(folder))
            foreach (var path in Directory.EnumerateFiles(folder, $"{prefix}-*.webp"))
                File.Delete(path);

        return Task.CompletedTask;
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

    private string BuildUrl(Guid publicationId, string fileName) =>
        string.Join('/',
            _options.PublicBaseUrl.TrimEnd('/'),
            _options.RequestPath.Trim('/'),
            "publicaciones",
            publicationId,
            fileName);
}
