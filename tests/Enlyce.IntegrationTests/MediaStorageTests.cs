using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Enlyce.Domain.Media;
using Enlyce.Domain.Errors;
using Enlyce.Infrastructure;
using Enlyce.Infrastructure.Media;
using Enlyce.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using ImageMagick;
using Xunit;

namespace Enlyce.IntegrationTests;

public class ImageVariantProcessorTests
{
    private static MediaUpload BuildUpload(byte[] png)
        => MediaUpload.Create(Guid.NewGuid(), "foto.png", "image/png", png.Length, new MemoryStream(png));

    private static MediaUpload BuildUpload(byte[] data, string contentType)
        => MediaUpload.Create(Guid.NewGuid(), "foto", contentType, data.Length, new MemoryStream(data));

    private static byte[] CreatePng(int width, int height)
    {
        using var image = new MagickImage(MagickColors.White, (uint)width, (uint)height);
        return image.ToByteArray(MagickFormat.Png);
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

    [Theory]
    [InlineData(MagickFormat.Jpeg, "image/jpeg")]
    [InlineData(MagickFormat.WebP, "image/webp")]
    public async Task ProcessAsync_AcceptsDeclaredSupportedFormats(MagickFormat format, string contentType)
    {
        using var image = new MagickImage(MagickColors.White, 1000, 800);
        var bytes = image.ToByteArray(format);

        var result = await new ImageVariantProcessor()
            .ProcessAsync(BuildUpload(bytes, contentType), [800], 82);

        Assert.Equal(800, Assert.Single(result.Variants).Width);
    }

    [Fact]
    public async Task ProcessAsync_RejectsMimeMismatchBeforeDecoding()
    {
        var png = CreatePng(1000, 800);

        var error = await Assert.ThrowsAsync<DomainError>(() => new ImageVariantProcessor()
            .ProcessAsync(BuildUpload(png, "image/jpeg"), [800], 82));

        Assert.Contains("no coincide", error.Message);
    }

    [Fact]
    public async Task ProcessAsync_RejectsStreamLargerThanDeclaredLimit()
    {
        var bytes = new byte[MediaConstraints.MaxSizeBytes + 1];
        var upload = MediaUpload.Create(Guid.NewGuid(), "foto.png", "image/png", 1,
            new MemoryStream(bytes));

        await Assert.ThrowsAsync<DomainError>(() => new ImageVariantProcessor()
            .ProcessAsync(upload, [800], 82));
    }

    [Fact]
    public async Task ProcessAsync_RejectsCorruptPngWithValidSignature()
    {
        var bytes = new byte[2048];
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(bytes, 0);

        await Assert.ThrowsAsync<DomainError>(() => new ImageVariantProcessor()
            .ProcessAsync(BuildUpload(bytes), [800], 82));
    }

    [Fact]
    public async Task ProcessAsync_AppliesExifOrientationBeforeGeneratingVariant()
    {
        using var image = new MagickImage(MagickColors.White, 1000, 800);
        var jpeg = image.ToByteArray(MagickFormat.Jpeg);
        byte[] exif =
        [
            0xff, 0xe1, 0x00, 0x22, 0x45, 0x78, 0x69, 0x66, 0x00, 0x00,
            0x49, 0x49, 0x2a, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        ];
        var bytes = new byte[jpeg.Length + exif.Length];
        jpeg.AsSpan(0, 2).CopyTo(bytes);
        exif.CopyTo(bytes, 2);
        jpeg.AsSpan(2).CopyTo(bytes.AsSpan(exif.Length + 2));

        var result = await new ImageVariantProcessor()
            .ProcessAsync(BuildUpload(bytes, "image/jpeg"), [800], 82);

        Assert.Equal(800, result.OriginalWidth);
        Assert.Equal(1000, result.OriginalHeight);
        Assert.Equal(800, Assert.Single(result.Variants).Width);
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

public sealed class S3OrphanStorageAdapterTests
{
    [Fact]
    public async Task ListsAllConfiguredPublicationObjectsAndDeletesRequestedKeys()
    {
        var client = Substitute.For<IAmazonS3>();
        client.ListObjectsV2Async(Arg.Any<ListObjectsV2Request>(), Arg.Any<CancellationToken>())
            .Returns(new ListObjectsV2Response
            {
                IsTruncated = false,
                S3Objects = [new S3Object { Key = "prefix/publicaciones/abc.webp", Size = 42 }]
            });
        client.DeleteObjectsAsync(Arg.Any<DeleteObjectsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new DeleteObjectsResponse()));
        var storage = new S3MediaStorage(client, Options.Create(new MediaStorageOptions
        {
            S3 = new S3StorageOptions { Bucket = "test-bucket", PublicBaseUrl = "https://media.example", KeyPrefix = "prefix" }
        }), new ImageVariantProcessor());

        var listed = await storage.ListObjectsAsync();
        await storage.DeleteObjectsAsync(["prefix/publicaciones/abc.webp"]);

        Assert.Single(listed);
        Assert.Equal(42, listed[0].Size);
        await client.Received(1).ListObjectsV2Async(
            Arg.Is<ListObjectsV2Request>(request => request.BucketName == "test-bucket" && request.Prefix == "prefix/publicaciones/"),
            Arg.Any<CancellationToken>());
        await client.Received(1).DeleteObjectsAsync(
            Arg.Is<DeleteObjectsRequest>(request => request.BucketName == "test-bucket" && request.Objects.Count == 1 && request.Objects[0].Key == "prefix/publicaciones/abc.webp"),
            Arg.Any<CancellationToken>());
    }
}
