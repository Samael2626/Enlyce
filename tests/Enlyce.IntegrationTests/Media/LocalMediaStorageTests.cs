using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;
using Enlyce.Infrastructure.Media;
using ImageMagick;
using Microsoft.Extensions.Options;

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
        }), new ImageVariantProcessor());

    private static MemoryStream Png(int width, int height)
    {
        using var image = new MagickImage(new MagickColor(180, 120, 90), (uint)width, (uint)height);
        return new MemoryStream(image.ToByteArray(MagickFormat.Png));
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
            using var image = new MagickImage(path);
            Assert.Equal(variant.Width, (int)image.Width);
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
        var original = first.ToArray();
        var one = await storage.StoreAsync(Upload(publicationId, first));
        using var second = new MemoryStream(original);
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

    [Theory]
    [InlineData("https://cdn.enlyce.test/media/legado.webp")]
    [InlineData("http://localhost:4173/assets/property-triptych.png")]
    [InlineData("https://cdn.enlyce.test/media/")]
    public async Task RemoveAsync_UrlWithoutVariantSuffix_DoesNotThrow(string url)
    {
        // El rollback de la carga llama a RemoveAsync; si revienta aqui se pierde
        // el DomainError original que motivo la limpieza.
        await CreateStorage().RemoveAsync(Guid.NewGuid(), url);
    }

    [Fact]
    public async Task RemoveAsync_DoesNotTouchOtherPhotosOfTheSamePublication()
    {
        var publicationId = Guid.NewGuid();
        var storage = CreateStorage();

        using var first = Png(1800, 1200);
        var one = await storage.StoreAsync(Upload(publicationId, first));
        using var second = Png(1400, 1000);
        var two = await storage.StoreAsync(Upload(publicationId, second));

        await storage.RemoveAsync(publicationId, one.Url);

        var folder = Path.Combine(_root, "publicaciones", publicationId.ToString());
        var remaining = Directory.GetFiles(folder, "*.webp");
        Assert.Equal(3, remaining.Length);
        Assert.All(remaining, path =>
            Assert.StartsWith(
                Path.GetFileName(new Uri(two.Url).LocalPath).Split('-')[0],
                Path.GetFileName(path)));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
