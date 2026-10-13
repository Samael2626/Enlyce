using Enlyce.Domain.Imports;

namespace Enlyce.Domain.Ports;

public interface IExternalListingSource
{
    // Nombre corto del origen, usado como prefijo de ExternalRef (ej. "wasi").
    string Source { get; }

    Task<IReadOnlyList<ExternalListing>> GetListingsAsync(CancellationToken ct = default);

    Task<ExternalPhotoContent> DownloadPhotoAsync(ExternalPhoto photo, CancellationToken ct = default);
}
