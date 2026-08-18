using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co")]
    [InlineData("a+b@c.com")]
    public void Create_ValidEmail_ReturnsEmail(string input)
    {
        var email = Email.Create(input);
        Assert.Equal(input.ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_EmptyOrNull_ThrowsDomainError(string? input)
    {
        Assert.Throws<DomainError>(() => Email.Create(input!));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    public void Create_InvalidFormat_ThrowsDomainError(string input)
    {
        Assert.Throws<DomainError>(() => Email.Create(input));
    }

    [Fact]
    public void Create_NormalizesToLowercase()
    {
        var email = Email.Create("User@Example.COM");
        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var email = Email.Create("test@example.com");
        Assert.Equal("test@example.com", email.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Email.Create("test@example.com");
        var b = Email.Create("test@example.com");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        var a = Email.Create("a@example.com");
        var b = Email.Create("b@example.com");
        Assert.NotEqual(a, b);
    }
}
