using Enlyce.Application.Media;
using Enlyce.Domain.Ports;
using NSubstitute;

namespace Enlyce.Application.Tests.Media;

public sealed class CleanupOrphanMediaHandlerTests
{
    private static readonly Guid PublicationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task DryRunReportsOrphansWithoutDeleting()
    {
        var storage = Substitute.For<IOrphanMediaStorage>();
        var references = Substitute.For<IPropertyPhotoReferenceReadRepository>();
        var referencedKey = $"publicaciones/{PublicationId}/1111111111111111-1600.webp";
        var orphanKey = $"publicaciones/{PublicationId}/2222222222222222-800.webp";
        storage.ListObjectsAsync(Arg.Any<CancellationToken>()).Returns([
            new StoredObjectInfo(referencedKey, 100), new StoredObjectInfo(orphanKey, 25)]);
        references.GetReferencedPhotoUrlsAsync(Arg.Any<CancellationToken>()).Returns([
            $"https://media.example/{referencedKey}"]);
        var handler = new CleanupOrphanMediaHandler(storage, references);

        var result = await handler.ExecuteAsync(true);

        Assert.True(result.DryRun);
        Assert.Equal(1, result.ObjectCount);
        Assert.Equal(25, result.TotalBytes);
        Assert.Equal([orphanKey], result.Keys);
        await storage.DidNotReceive().DeleteObjectsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRechecksReferencesAndDeletesOnlyOrphanKeys()
    {
        var storage = Substitute.For<IOrphanMediaStorage>();
        var references = Substitute.For<IPropertyPhotoReferenceReadRepository>();
        var referencedKey = $"publicaciones/{PublicationId}/1111111111111111-1600.webp";
        var orphanKey = $"publicaciones/{PublicationId}/2222222222222222-1600.webp";
        storage.ListObjectsAsync(Arg.Any<CancellationToken>()).Returns([
            new StoredObjectInfo(referencedKey, 100), new StoredObjectInfo(orphanKey, 25)]);
        references.GetReferencedPhotoUrlsAsync(Arg.Any<CancellationToken>()).Returns([
            $"https://media.example/{referencedKey}"]);
        var handler = new CleanupOrphanMediaHandler(storage, references);

        var result = await handler.ExecuteAsync(false);

        Assert.False(result.DryRun);
        Assert.Equal([orphanKey], result.Keys);
        await storage.Received(1).DeleteObjectsAsync(Arg.Is<IReadOnlyCollection<string>>(keys => keys.Count == 1 && keys.Single() == orphanKey), Arg.Any<CancellationToken>());
        await references.Received(2).GetReferencedPhotoUrlsAsync(Arg.Any<CancellationToken>());
    }
}
