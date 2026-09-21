using Enlyce.Domain.Media;

namespace Enlyce.Domain.Ports;

public interface IMediaStorage
{
    Task<StoredMedia> StoreAsync(MediaUpload upload, CancellationToken ct = default);

    Task RemoveAsync(Guid publicationId, string url, CancellationToken ct = default);
}
