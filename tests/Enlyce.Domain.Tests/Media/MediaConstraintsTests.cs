using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;

namespace Enlyce.Domain.Tests.Media;

public sealed class MediaConstraintsTests
{
    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("IMAGE/PNG")]
    public void ValidateDescriptor_AcceptsSupportedTypes(string contentType) =>
        MediaConstraints.ValidateDescriptor(contentType, 1024);

    [Theory]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData("text/html")]
    [InlineData("")]
    public void ValidateDescriptor_RejectsUnsupportedTypes(string contentType) =>
        Assert.Throws<DomainError>(() => MediaConstraints.ValidateDescriptor(contentType, 1024));

    [Fact]
    public void ValidateDescriptor_RejectsEmptyFile() =>
        Assert.Throws<DomainError>(() => MediaConstraints.ValidateDescriptor("image/png", 0));

    [Fact]
    public void ValidateDescriptor_RejectsOversizedFile() =>
        Assert.Throws<DomainError>(() =>
            MediaConstraints.ValidateDescriptor("image/png", MediaConstraints.MaxSizeBytes + 1));

    [Fact]
    public void ValidateDimensions_AcceptsMinimum() =>
        MediaConstraints.ValidateDimensions(MediaConstraints.MinWidth, MediaConstraints.MinHeight);

    [Theory]
    [InlineData(799, 600)]
    [InlineData(800, 599)]
    public void ValidateDimensions_RejectsTooSmall(int width, int height) =>
        Assert.Throws<DomainError>(() => MediaConstraints.ValidateDimensions(width, height));

    [Fact]
    public void ValidateDimensions_RejectsOversized() =>
        Assert.Throws<DomainError>(() =>
            MediaConstraints.ValidateDimensions(MediaConstraints.MaxDimension + 1, 600));

    [Fact]
    public void ValidatePhotoCount_RejectsAtLimit() =>
        Assert.Throws<DomainError>(() =>
            MediaConstraints.ValidatePhotoCount(MediaConstraints.MaxPhotosPerPublication));

    [Fact]
    public void ValidatePhotoCount_AcceptsBelowLimit() =>
        MediaConstraints.ValidatePhotoCount(MediaConstraints.MaxPhotosPerPublication - 1);
}
