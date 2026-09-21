namespace Enlyce.Application.UseCases.UploadPublicationPhoto;

public record UploadPublicationPhotoCommand(
    Guid PublicationId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    string AltText,
    bool IsCover);
