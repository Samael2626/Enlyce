using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.UploadPublicationPhoto;
using Enlyce.Domain.Media;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.PublicationMedia;

public static class PublicationMediaModule
{
    public static void MapPublicationMedia(this IEndpointRouteBuilder app)
    {
        // Carga de medios: solo personal del CRM. El catalogo publico es de lectura.
        var group = app.MapGroup("/api/publicaciones/{publicationId:guid}/fotos")
            .WithTags("PublicationMedia")
            .RequireAuthorization();

        group.MapPost("/", async (
            Guid publicationId,
            IFormFile archivo,
            [FromForm] string textoAlternativo,
            [FromForm] bool esPortada,
            ICommandHandler<UploadPublicationPhotoCommand, UploadPublicationPhotoResponse> handler,
            CancellationToken ct) =>
        {
            await using var content = archivo.OpenReadStream();

            var result = await handler.HandleAsync(new UploadPublicationPhotoCommand(
                publicationId,
                archivo.FileName,
                archivo.ContentType,
                archivo.Length,
                content,
                textoAlternativo,
                esPortada), ct);

            return Results.Created(result.Url, result);
        })
        .DisableAntiforgery()
        .WithName("UploadPublicationPhoto")
        .Produces<UploadPublicationPhotoResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/restricciones", () => Results.Ok(new MediaConstraintsResponse(
            MediaConstraints.SupportedContentTypes.ToArray(),
            MediaConstraints.MaxSizeBytes,
            MediaConstraints.MinWidth,
            MediaConstraints.MinHeight,
            MediaConstraints.MaxDimension,
            MediaConstraints.MaxPhotosPerPublication)))
        .WithName("GetMediaConstraints")
        .Produces<MediaConstraintsResponse>();
    }
}

public record MediaConstraintsResponse(
    string[] TiposPermitidos,
    long MaxBytes,
    int AnchoMinimo,
    int AltoMinimo,
    int DimensionMaxima,
    int MaxFotosPorPublicacion);
