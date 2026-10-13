using System.Text.RegularExpressions;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Media;

public interface IPropertyPhotoReferenceReadRepository
{
    Task<IReadOnlyList<string>> GetReferencedPhotoUrlsAsync(CancellationToken ct = default);
}

public sealed record OrphanMediaCleanupResult(bool DryRun, int ObjectCount, long TotalBytes, IReadOnlyList<string> Keys);

public interface ICleanupOrphanMediaHandler
{
    Task<OrphanMediaCleanupResult> ExecuteAsync(bool dryRun, CancellationToken ct = default);
}

public sealed partial class CleanupOrphanMediaHandler(
    IOrphanMediaStorage storage,
    IPropertyPhotoReferenceReadRepository references) : ICleanupOrphanMediaHandler
{
    [GeneratedRegex(@"(?:^|/)publicaciones/([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})/([0-9a-f]{16})-\d+\.webp$", RegexOptions.IgnoreCase)]
    private static partial Regex MediaKeyPattern();

    public async Task<OrphanMediaCleanupResult> ExecuteAsync(bool dryRun, CancellationToken ct = default)
    {
        var referencedUrls = await references.GetReferencedPhotoUrlsAsync(ct);
        var referenced = referencedUrls.Select(GetIdentity).Where(x => x is not null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var objects = await storage.ListObjectsAsync(ct);
        var orphaned = objects.Where(x => GetIdentity(x.Key) is { } identity && !referenced.Contains(identity)).ToArray();
        var keys = orphaned.Select(x => x.Key).ToArray();
        var bytes = orphaned.Sum(x => x.Size);

        if (!dryRun && keys.Length > 0)
        {
            // Relee referencias justo antes del borrado para reducir riesgo ante cambios recientes.
            var latestReferenced = (await references.GetReferencedPhotoUrlsAsync(ct))
                .Select(GetIdentity).Where(x => x is not null).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var safeKeys = orphaned.Where(x => GetIdentity(x.Key) is { } identity && !latestReferenced.Contains(identity))
                .Select(x => x.Key).ToArray();
            await storage.DeleteObjectsAsync(safeKeys, ct);
            keys = safeKeys;
            bytes = orphaned.Where(x => safeKeys.Contains(x.Key, StringComparer.Ordinal)).Sum(x => x.Size);
        }

        return new OrphanMediaCleanupResult(dryRun, keys.Length, bytes, keys);
    }

    private static string? GetIdentity(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var candidate = Uri.TryCreate(path, UriKind.Absolute, out var uri) ? uri.AbsolutePath : path;
        var match = MediaKeyPattern().Match(candidate);
        return match.Success ? $"{match.Groups[1].Value}/{match.Groups[2].Value}" : null;
    }
}
