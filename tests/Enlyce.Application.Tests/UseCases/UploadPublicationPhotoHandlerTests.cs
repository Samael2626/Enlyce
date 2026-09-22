using Enlyce.Application.UseCases.UploadPublicationPhoto;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using NSubstitute;

namespace Enlyce.Application.Tests.UseCases;

public sealed class UploadPublicationPhotoHandlerTests
{
    private readonly IPropertyPublicationRepository _publications =
        Substitute.For<IPropertyPublicationRepository>();
    private readonly IMediaStorage _storage = Substitute.For<IMediaStorage>();
    private readonly UploadPublicationPhotoHandler _handler;

    public UploadPublicationPhotoHandlerTests()
    {
        _handler = new UploadPublicationPhotoHandler(_publications, _storage);

        _storage.StoreAsync(Arg.Any<MediaUpload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new StoredMedia(
                "https://cdn.enlyce.test/media/a-1600.webp",
                1600,
                1067,
                [new MediaVariant("https://cdn.enlyce.test/media/a-1600.webp", 1600)])));
    }

    private static PropertyPublication Publication() =>
        PropertyPublication.Create(Guid.NewGuid(), Guid.NewGuid(), "casa-laureles", "Casa", "Descripcion");

    private static UploadPublicationPhotoCommand Command(Guid publicationId, bool isCover = false) =>
        new(publicationId, "fachada.png", "image/png", 4, new MemoryStream([1, 2, 3, 4]), "Fachada", isCover);

    [Fact]
    public async Task HandleAsync_FirstPhotoBecomesCoverAtOrderZero()
    {
        var publication = Publication();
        _publications.GetByIdAsync(publication.Id, Arg.Any<CancellationToken>()).Returns(publication);

        var result = await _handler.HandleAsync(Command(publication.Id));

        Assert.Equal(0, result.Order);
        Assert.True(result.IsCover);
        Assert.Equal("https://cdn.enlyce.test/media/a-1600.webp", result.Url);
        await _publications.Received(1).SaveAsync(publication, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AssignsNextOrderAndKeepsExistingCover()
    {
        var publication = Publication();
        publication.AddPhoto("https://cdn.enlyce.test/media/portada.webp", "Portada", 0, true);
        _publications.GetByIdAsync(publication.Id, Arg.Any<CancellationToken>()).Returns(publication);

        var result = await _handler.HandleAsync(Command(publication.Id));

        Assert.Equal(1, result.Order);
        Assert.False(result.IsCover);
    }

    [Fact]
    public async Task HandleAsync_MissingPublication_FailsLoud()
    {
        var publicationId = Guid.NewGuid();
        _publications.GetByIdAsync(publicationId, Arg.Any<CancellationToken>())
            .Returns((PropertyPublication?)null);

        await Assert.ThrowsAsync<DomainError>(() => _handler.HandleAsync(Command(publicationId)));
        await _storage.DidNotReceive().StoreAsync(Arg.Any<MediaUpload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_RejectedByDomain_RemovesOrphanFiles()
    {
        var publication = Publication();
        // Segunda portada: el dominio rechaza y el archivo ya esta en disco.
        publication.AddPhoto("https://cdn.enlyce.test/media/portada.webp", "Portada", 0, true);
        _publications.GetByIdAsync(publication.Id, Arg.Any<CancellationToken>()).Returns(publication);

        await Assert.ThrowsAsync<DomainError>(() =>
            _handler.HandleAsync(Command(publication.Id, isCover: true)));

        await _storage.Received(1).RemoveAsync(
            publication.Id, "https://cdn.enlyce.test/media/a-1600.webp", Arg.Any<CancellationToken>());
        await _publications.DidNotReceive().SaveAsync(Arg.Any<PropertyPublication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CleanupAlsoFails_KeepsBothErrors()
    {
        var publication = Publication();
        publication.AddPhoto("https://cdn.enlyce.test/media/portada.webp", "Portada", 0, true);
        _publications.GetByIdAsync(publication.Id, Arg.Any<CancellationToken>()).Returns(publication);

        _storage.RemoveAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new IOException("disco ocupado"));

        var error = await Assert.ThrowsAsync<AggregateException>(() =>
            _handler.HandleAsync(Command(publication.Id, isCover: true)));

        Assert.Contains(error.InnerExceptions, inner => inner is DomainError);
        Assert.Contains(error.InnerExceptions, inner => inner is IOException);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedContentType_RejectsBeforeStoring()
    {
        var publication = Publication();
        _publications.GetByIdAsync(publication.Id, Arg.Any<CancellationToken>()).Returns(publication);

        var command = new UploadPublicationPhotoCommand(
            publication.Id, "documento.pdf", "application/pdf", 4,
            new MemoryStream([1, 2, 3, 4]), "Documento", false);

        await Assert.ThrowsAsync<DomainError>(() => _handler.HandleAsync(command));
        await _storage.DidNotReceive().StoreAsync(Arg.Any<MediaUpload>(), Arg.Any<CancellationToken>());
    }
}
