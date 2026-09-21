using Enlyce.Domain.Errors;
using Enlyce.Domain.Media;

namespace Enlyce.Domain.Tests.Media;

public sealed class MediaUploadTests
{
    private static MemoryStream Content() => new([1, 2, 3, 4]);

    [Fact]
    public void Create_NormalizesFileNameAndContentType()
    {
        var upload = MediaUpload.Create(Guid.NewGuid(), "  Fachada.PNG ", " IMAGE/PNG ", 4, Content());

        Assert.Equal("Fachada.PNG", upload.FileName);
        Assert.Equal("image/png", upload.ContentType);
    }

    [Fact]
    public void Create_RejectsEmptyPublication() =>
        Assert.Throws<DomainError>(() =>
            MediaUpload.Create(Guid.Empty, "a.png", "image/png", 4, Content()));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankFileName(string fileName) =>
        Assert.Throws<DomainError>(() =>
            MediaUpload.Create(Guid.NewGuid(), fileName, "image/png", 4, Content()));

    [Fact]
    public void Create_RejectsUnsupportedContentType() =>
        Assert.Throws<DomainError>(() =>
            MediaUpload.Create(Guid.NewGuid(), "a.gif", "image/gif", 4, Content()));

    [Fact]
    public void Create_RejectsNullContent() =>
        Assert.Throws<ArgumentNullException>(() =>
            MediaUpload.Create(Guid.NewGuid(), "a.png", "image/png", 4, null!));
}
