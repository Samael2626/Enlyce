using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.ValueObjects;

public class TelefonoTests
{
    [Theory]
    [InlineData("3101234567")]
    [InlineData("+57 310 123 4567")]
    [InlineData("310-123-4567")]
    [InlineData("(310) 123 4567")]
    public void Create_ValidPhone_ReturnsCleaned(string input)
    {
        var phone = Telefono.Create(input);
        Assert.NotEmpty(phone.Value);
        Assert.True(phone.Value.Length >= 7);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_EmptyOrNull_ThrowsDomainError(string? input)
    {
        Assert.Throws<DomainError>(() => Telefono.Create(input!));
    }

    [Fact]
    public void Create_RemovesSpacesAndDashes()
    {
        var phone = Telefono.Create("310-123-4567");
        Assert.Equal("3101234567", phone.Value);
    }

    [Fact]
    public void Create_TooShort_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() => Telefono.Create("12345"));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var phone = Telefono.Create("3101234567");
        Assert.Equal("3101234567", phone.ToString());
    }
}
