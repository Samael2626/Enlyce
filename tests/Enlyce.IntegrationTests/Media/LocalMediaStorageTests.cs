using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using Enlyce.Infrastructure.Media;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Enlyce.IntegrationTests.Media;

public sealed class LocalMediaStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"enlyce-media-{Guid.NewGuid():N}");

    private LocalMediaStorage CreateStorage() =>
        new(Options.Create(new MediaStorageOptions
        {
            RootPath = _root,
            RequestPath = "/media",
            PublicBaseUrl = "https://cdn.enlyce.test/",
            VariantWidths = [1600, 800, 400],
            Quality = 80
        }));

    private static MemoryStream Png(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height, new Rgba32(180, 120, 90));
        var buffer = new MemoryStream();
        image.Save(buffer, new PngEncoder());
        buffer.Position = 0;
        return buffer;
    }

    private static MediaUpload Upload(Guid publicationId, Stream content) =>
        MediaUpload.Create(publicationId, "fachada.png", "image/png", content.Length, content);

    [Fact]
    public async Task StoreAsync_WritesEveryVariantAsWebp()
    {
        var publicationId = Guid.NewGuid();
        using var source = Png(2400, 1600);

        var stored = await CreateStorage().StoreAsync(Upload(publicationId, source));

        Assert.Equal(3, stored.Variants.Count);
        Assert.Equal([1600, 800, 400], stored.Variants.Select(variant => variant.Width));

        var folder = Path.Combine(_root, "publicaciones", publicationId.ToString());
        Assert.Equal(3, Directory.GetFiles(folder, "*.webp").Length);

        foreach (var variant in stored.Variants)
        {
            var path = Path.Combine(folder, Path.GetFileName(new Uri(variant.Url).LocalPath));
            using var image = await Image.LoadAsync(path);
            Assert.Equal(variant.Width, image.Width);
        }
    }

    [Fact]
    public async Task StoreAsync_CanonicalUrlIsLargestVariantAndKeepsAspectRatio()
    {
        using var source = Png(2400, 1600);

        var stored = await CreateStorage().StoreAsync(Upload(Guid.NewGuid(), source));

        Assert.Equal(stored.Variants[0].Url, stored.Url);
        Assert.Equal(1600, stored.Width);
        Assert.Equal(1067, stored.Height);
        Assert.StartsWith("https://cdn.enlyce.test/media/publicaciones/", stored.Url);
    }

    [Fact]
    public async Task StoreAsync_NeverUpscalesSmallOriginal()
    {
        using var source = Png(1000, 800);

        var stored = await CreateStorage().StoreAsync(Upload(Guid.NewGuid(), source));

        Assert.Equal(1000, stored.Width);
        Assert.Equal([1000, 800, 400], stored.Variants.Select(variant => variant.Width));
    }

    [Fact]
    public async Task StoreAsync_RejectsImageBelowMinimumDimensions()
    {
        using var source = Png(640, 480);

        await Assert.ThrowsAsync<DomainError>(() =>
            CreateStorage().StoreAsync(Upload(Guid.NewGuid(), source)));
    }

    [Fact]
    public async Task StoreAsync_RejectsContentThatIsNotAnImage()
    {
        using var source = new MemoryStream(new byte[2048]);

        await Assert.ThrowsAsync<DomainError>(() =>
            CreateStorage().StoreAsync(Upload(Guid.NewGuid(), source)));
    }

    [Fact]
    public async Task StoreAsync_SameContentTwiceReusesFingerprint()
    {
        var publicationId = Guid.NewGuid();
        var storage = CreateStorage();

        using var first = Png(1800, 1200);
        var one = await storage.StoreAsync(Upload(publicationId, first));
        using var second = Png(1800, 1200);
        var two = await storage.StoreAsync(Upload(publicationId, second));

        Assert.Equal(one.Url, two.Url);
        Assert.Equal(3, Directory.GetFiles(
            Path.Combine(_root, "publicaciones", publicationId.ToString()), "*.webp").Length);
    }

    [Fact]
    public async Task RemoveAsync_DeletesEveryVariantOfTheSamePhoto()
    {
        var publicationId = Guid.NewGuid();
        var storage = CreateStorage();
        using var source = Png(1800, 1200);

        var stored = await storage.StoreAsync(Upload(publicationId, source));
        await storage.RemoveAsync(publicationId, stored.Url);

        Assert.Empty(Directory.GetFiles(
            Path.Combine(_root, "publicaciones", publicationId.ToString()), "*.webp"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
