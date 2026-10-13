using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using Microsoft.Extensions.Options;

namespace Enlyce.Infrastructure.Media;

// Adaptador S3-compatible (Cloudflare R2, AWS S3). Sube las variantes WebP al
// bucket y construye las URL publicas. El procesado de imagen es el mismo que
// usa el adaptador local; aqui solo cambia el destino de los bytes.
public sealed partial class S3MediaStorage : IMediaStorage, IOrphanMediaStorage
{
    [GeneratedRegex(@"^([0-9a-f]{16})-\d+\.webp$", RegexOptions.IgnoreCase)]
    private static partial Regex VariantFileName();

    private const string ContentType = "image/webp";

    private readonly IAmazonS3 _client;
    private readonly MediaStorageOptions _options;
    private readonly ImageVariantProcessor _processor;

    public S3MediaStorage(IAmazonS3 client, IOptions<MediaStorageOptions> options, ImageVariantProcessor processor)
    {
        _client = client;
        _options = options.Value;
        _processor = processor;

        if (string.IsNullOrWhiteSpace(_options.S3.Bucket))
            throw new InvalidOperationException("MediaStorage:S3:Bucket es obligatorio con Provider=s3.");
        if (string.IsNullOrWhiteSpace(_options.S3.PublicBaseUrl))
            throw new InvalidOperationException("MediaStorage:S3:PublicBaseUrl es obligatorio con Provider=s3.");
    }

    public async Task<StoredMedia> StoreAsync(MediaUpload upload, CancellationToken ct = default)
    {
        var processed = await _processor.ProcessAsync(upload, _options.VariantWidths, _options.Quality, ct);

        var variants = new List<MediaVariant>(processed.Variants.Count);
        foreach (var variant in processed.Variants)
        {
            var fileName = $"{processed.Fingerprint}-{variant.Width}.webp";
            var key = BuildKey(upload.PublicationId, fileName);

            using var body = new MemoryStream(variant.Content, writable: false);
            await _client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _options.S3.Bucket,
                Key = key,
                InputStream = body,
                ContentType = ContentType,
                DisablePayloadSigning = true
            }, ct);

            variants.Add(new MediaVariant(BuildUrl(key), variant.Width));
        }

        var canonical = processed.Variants[0];
        return new StoredMedia(variants[0].Url, canonical.Width, canonical.Height, variants);
    }

    public async Task RemoveAsync(Guid publicationId, string url, CancellationToken ct = default)
    {
        // Igual que el adaptador local: ignora URL que no sigan el patron, para no
        // reventar durante el rollback de una carga.
        var fingerprint = ExtractFingerprint(url);
        if (fingerprint is null)
            return;

        // Borra todas las variantes que comparten huella bajo la carpeta de la publicacion.
        var prefix = BuildKey(publicationId, $"{fingerprint}-");
        var listed = await _client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _options.S3.Bucket,
            Prefix = prefix
        }, ct);

        if (listed.S3Objects is null || listed.S3Objects.Count == 0)
            return;

        await _client.DeleteObjectsAsync(new DeleteObjectsRequest
        {
            BucketName = _options.S3.Bucket,
            Objects = listed.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
        }, ct);
    }

    public async Task<IReadOnlyList<StoredObjectInfo>> ListObjectsAsync(CancellationToken ct = default)
    {
        var result = new List<StoredObjectInfo>();
        string? token = null;
        do
        {
            var page = await _client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _options.S3.Bucket,
                Prefix = string.IsNullOrWhiteSpace(_options.S3.KeyPrefix) ? "publicaciones/" : $"{_options.S3.KeyPrefix.Trim('/')}/publicaciones/",
                ContinuationToken = token
            }, ct);
            result.AddRange((page.S3Objects ?? []).Select(item => new StoredObjectInfo(item.Key, item.Size ?? 0)));
            token = page.IsTruncated == true ? page.NextContinuationToken : null;
        } while (token is not null);
        return result;
    }

    public async Task DeleteObjectsAsync(IReadOnlyCollection<string> keys, CancellationToken ct = default)
    {
        foreach (var batch in keys.Chunk(1000))
        {
            if (batch.Length == 0) continue;
            await _client.DeleteObjectsAsync(new DeleteObjectsRequest
            {
                BucketName = _options.S3.Bucket,
                Objects = batch.Select(key => new KeyVersion { Key = key }).ToList()
            }, ct);
        }
    }

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

    private string BuildKey(Guid publicationId, string fileName)
    {
        var prefix = _options.S3.KeyPrefix.Trim('/');
        var segments = string.IsNullOrEmpty(prefix)
            ? new[] { "publicaciones", publicationId.ToString(), fileName }
            : new[] { prefix, "publicaciones", publicationId.ToString(), fileName };
        return string.Join('/', segments);
    }

    private string BuildUrl(string key) =>
        string.Join('/', _options.S3.PublicBaseUrl.TrimEnd('/'), key);
}
