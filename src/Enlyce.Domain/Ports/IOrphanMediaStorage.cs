namespace Enlyce.Domain.Ports;

public sealed record StoredObjectInfo(string Key, long Size);

public interface IOrphanMediaStorage
{
    Task<IReadOnlyList<StoredObjectInfo>> ListObjectsAsync(CancellationToken ct = default);
    Task DeleteObjectsAsync(IReadOnlyCollection<string> keys, CancellationToken ct = default);
}
