namespace Enlyce.Application.UseCases.UploadPublicationPhoto;

public record UploadPublicationPhotoResponse(
    Guid PublicationId,
    string Url,
    string AltText,
    int Order,
    bool IsCover,
    int Width,
    int Height,
    IReadOnlyList<PhotoVariantResponse> Variants);

public record PhotoVariantResponse(string Url, int Width);
