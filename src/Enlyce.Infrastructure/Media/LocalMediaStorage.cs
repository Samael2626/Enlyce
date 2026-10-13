using System.Text.RegularExpressions;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using Microsoft.Extensions.Options;

namespace Enlyce.Infrastructure.Media;

// Adaptador local: escribe las variantes WebP en disco y las sirve por static
// files. El procesado de imagen lo hace ImageVariantProcessor, compartido con
// el adaptador S3; aqui solo cambia donde se escriben los bytes.
public sealed partial class LocalMediaStorage : IMediaStorage
{
    [GeneratedRegex(@"^([0-9a-f]{16})-\d+\.webp$", RegexOptions.IgnoreCase)]
    private static partial Regex VariantFileName();

    private readonly MediaStorageOptions _options;
    private readonly ImageVariantProcessor _processor;

    public LocalMediaStorage(IOptions<MediaStorageOptions> options, ImageVariantProcessor processor)
    {
        _options = options.Value;
        _processor = processor;

        if (_options.VariantWidths.Length == 0)
            throw new InvalidOperationException("MediaStorage:VariantWidths no puede estar vacio.");
    }

    public async Task<StoredMedia> StoreAsync(MediaUpload upload, CancellationToken ct = default)
    {
        var processed = await _processor.ProcessAsync(upload, _options.VariantWidths, _options.Quality, ct);

        var folder = Path.Combine(_options.RootPath, "publicaciones", upload.PublicationId.ToString());
        Directory.CreateDirectory(folder);

        var variants = new List<MediaVariant>(processed.Variants.Count);
        foreach (var variant in processed.Variants)
        {
            var fileName = $"{processed.Fingerprint}-{variant.Width}.webp";
            var path = Path.Combine(folder, fileName);

            await using (var output = File.Create(path))
                await output.WriteAsync(variant.Content, ct);

            variants.Add(new MediaVariant(BuildUrl(upload.PublicationId, fileName), variant.Width));
        }

        var canonical = processed.Variants[0];
        return new StoredMedia(variants[0].Url, canonical.Width, canonical.Height, variants);
    }

    public Task RemoveAsync(Guid publicationId, string url, CancellationToken ct = default)
    {
        // Las URL heredadas o externas no siguen el patron huella-ancho.webp.
        // Se ignoran en vez de reventar: esto corre en el rollback de la carga
        // y una excepcion aqui enterraria el error que motivo la limpieza.
        var fingerprint = ExtractFingerprint(url);
        if (fingerprint is null)
            return Task.CompletedTask;

        var folder = Path.Combine(_options.RootPath, "publicaciones", publicationId.ToString());
        if (!Directory.Exists(folder))
            return Task.CompletedTask;

        // Se borran todas las variantes que comparten huella, no solo la URL dada.
        foreach (var path in Directory.EnumerateFiles(folder, $"{fingerprint}-*.webp"))
            File.Delete(path);

        return Task.CompletedTask;
    }

    // Acepta solo el nombre que genera StoreAsync, para que un valor arbitrario
    // no se convierta en un patron de busqueda con comodines.
    private static string? ExtractFingerprint(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            return null;

        var fileName = Path.GetFileName(uri.IsAbsoluteUri ? uri.LocalPath : url);
        var match = VariantFileName().Match(fileName);
        return match.Success ? match.Groups[1].Value : null;
    }

    private string BuildUrl(Guid publicationId, string fileName) =>
        string.Join('/',
            _options.PublicBaseUrl.TrimEnd('/'),
            _options.RequestPath.Trim('/'),
            "publicaciones",
            publicationId,
            fileName);
}
