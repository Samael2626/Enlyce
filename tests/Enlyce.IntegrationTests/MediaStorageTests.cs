using System.Text;
using Enlyce.Domain.Media;
using Enlyce.Infrastructure;
using Enlyce.Infrastructure.Media;
using Enlyce.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Enlyce.IntegrationTests;

public class ImageVariantProcessorTests
{
    private static MediaUpload BuildUpload(byte[] png)
        => MediaUpload.Create(Guid.NewGuid(), "foto.png", "image/png", png.Length, new MemoryStream(png));

    private static byte[] CreatePng(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        using var buffer = new MemoryStream();
        image.Save(buffer, new PngEncoder());
        return buffer.ToArray();
    }

    private static bool IsWebp(byte[] data)
        => data.Length > 12
           && Encoding.ASCII.GetString(data, 0, 4) == "RIFF"
           && Encoding.ASCII.GetString(data, 8, 4) == "WEBP";

    [Fact]
    public async Task ProcessAsync_GeneratesWebpVariantsPerWidth()
    {
        var png = CreatePng(1000, 800);
        var processed = await new ImageVariantProcessor()
            .ProcessAsync(BuildUpload(png), new[] { 1600, 800, 400 }, 82);

        // 1600 se recorta a 1000 (no agranda); 800 y 400 quedan igual.
        Assert.Equal(new[] { 1000, 800, 400 }, processed.Variants.Select(v => v.Width).ToArray());
        Assert.All(processed.Variants, v => Assert.True(IsWebp(v.Content), "la variante debe ser WebP"));
        Assert.All(processed.Variants, v => Assert.True(v.Width <= processed.OriginalWidth));
        Assert.Equal(1000, processed.OriginalWidth);
    }

    [Fact]
    public async Task ProcessAsync_FingerprintIsStableForSameContent()
    {
        var png = CreatePng(1000, 800);
        var processor = new ImageVariantProcessor();

        var a = await processor.ProcessAsync(BuildUpload(png), new[] { 800 }, 82);
        var b = await processor.ProcessAsync(BuildUpload(png), new[] { 800 }, 82);

        Assert.Equal(a.Fingerprint, b.Fingerprint);
        Assert.Matches("^[0-9a-f]{16}$", a.Fingerprint);
    }
}

public class MediaStorageProviderSelectionTests
{
    private static IServiceProvider Build(Dictionary<string, string?> settings)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddInfrastructure(config).BuildServiceProvider();
    }

    [Fact]
    public void Provider_Local_ResolvesLocalAdapter()
    {
        var provider = Build(new() { ["MediaStorage:Provider"] = "local" });
        Assert.IsType<LocalMediaStorage>(provider.GetRequiredService<IMediaStorage>());
    }

    [Fact]
    public void Provider_S3_ResolvesS3Adapter()
    {
        var provider = Build(new()
        {
            ["MediaStorage:Provider"] = "s3",
            ["MediaStorage:S3:ServiceUrl"] = "https://acc.r2.cloudflarestorage.com",
            ["MediaStorage:S3:Bucket"] = "enlyce-media",
            ["MediaStorage:S3:AccessKey"] = "test-access",
            ["MediaStorage:S3:SecretKey"] = "test-secret",
            ["MediaStorage:S3:PublicBaseUrl"] = "https://media.enlyce.test",
        });
        Assert.IsType<S3MediaStorage>(provider.GetRequiredService<IMediaStorage>());
    }

    [Fact]
    public void Provider_S3_WithoutCredentials_Throws()
    {
        Assert.ThrowsAny<Exception>(() => Build(new()
        {
            ["MediaStorage:Provider"] = "s3",
            ["MediaStorage:S3:Bucket"] = "enlyce-media",
            ["MediaStorage:S3:PublicBaseUrl"] = "https://media.enlyce.test",
        }));
    }
}
