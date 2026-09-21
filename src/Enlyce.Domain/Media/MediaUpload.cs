using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Media;

// Archivo entrante ya validado en lo declarativo. El contenido es un Stream de
// solo lectura que el adaptador consume una vez.
public sealed class MediaUpload
{
    public Guid PublicationId { get; }
    public string FileName { get; }
    public string ContentType { get; }
    public long SizeBytes { get; }
    public Stream Content { get; }

    private MediaUpload(
        Guid publicationId, string fileName, string contentType, long sizeBytes, Stream content)
    {
        PublicationId = publicationId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        Content = content;
    }

    public static MediaUpload Create(
        Guid publicationId, string fileName, string contentType, long sizeBytes, Stream content)
    {
        if (publicationId == Guid.Empty)
            throw new DomainError("La publicacion del medio es obligatoria.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainError("El nombre del archivo es obligatorio.");

        ArgumentNullException.ThrowIfNull(content);
        MediaConstraints.ValidateDescriptor(contentType, sizeBytes);

        return new MediaUpload(
            publicationId, fileName.Trim(), contentType.Trim().ToLowerInvariant(), sizeBytes, content);
    }
}
