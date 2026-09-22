using Enlyce.Application.Abstractions;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.UseCases.UploadPublicationPhoto;

public class UploadPublicationPhotoHandler
    : ICommandHandler<UploadPublicationPhotoCommand, UploadPublicationPhotoResponse>
{
    private readonly IPropertyPublicationRepository _publications;
    private readonly IMediaStorage _storage;

    public UploadPublicationPhotoHandler(
        IPropertyPublicationRepository publications, IMediaStorage storage)
    {
        _publications = publications;
        _storage = storage;
    }

    public async Task<UploadPublicationPhotoResponse> HandleAsync(
        UploadPublicationPhotoCommand command, CancellationToken ct = default)
    {
        var publication = await _publications.GetByIdAsync(command.PublicationId, ct)
            ?? throw new DomainError($"La publicacion {command.PublicationId} no existe.");

        // El orden lo decide el dominio, no el cliente: evita huecos y colisiones.
        var order = publication.Photos.Count == 0 ? 0 : publication.Photos.Max(photo => photo.Order) + 1;
        var isCover = command.IsCover || publication.Photos.Count == 0;

        var upload = MediaUpload.Create(
            command.PublicationId,
            command.FileName,
            command.ContentType,
            command.SizeBytes,
            command.Content);

        var stored = await _storage.StoreAsync(upload, ct);

        try
        {
            publication.AddPhoto(stored.Url, command.AltText, order, isCover);
            await _publications.SaveAsync(publication, ct);
        }
        catch (Exception rejection)
        {
            // Sin esto el disco acumula variantes huerfanas ante cualquier rechazo.
            try
            {
                await _storage.RemoveAsync(command.PublicationId, stored.Url, ct);
            }
            catch (Exception cleanupFailure)
            {
                // Los dos fallos importan: el rechazo y las variantes que quedaron
                // en disco. Tragarse cualquiera de ellos deja basura invisible.
                throw new AggregateException(
                    $"La foto fue rechazada y ademas no se pudo limpiar {stored.Url}.",
                    rejection,
                    cleanupFailure);
            }

            throw;
        }

        return new UploadPublicationPhotoResponse(
            publication.Id,
            stored.Url,
            command.AltText,
            order,
            isCover,
            stored.Width,
            stored.Height,
            stored.Variants.Select(variant => new PhotoVariantResponse(variant.Url, variant.Width)).ToList());
    }
}
