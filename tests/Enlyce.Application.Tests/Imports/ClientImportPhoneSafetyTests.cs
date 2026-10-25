using Enlyce.Application.Imports;

namespace Enlyce.Application.Tests.Imports;

public sealed class ClientImportPhoneSafetyTests
{
    [Theory]
    [InlineData("3001234567", true)]
    [InlineData("573001234567", true)]
    [InlineData("12345678901", true)]
    [InlineData("1234567890123", true)]
    [InlineData("03001234567", false)]
    [InlineData("123456", false)]
    [InlineData("1234567890123456", false)]
    [InlineData("3.001234567E9", false)]
    public void NumericPhoneAcceptsOnlyExactDigitsWithinPhoneLength(string value, bool expected) =>
        Assert.Equal(expected, ClientImportPhoneSafety.IsExactNumericPhone(value));
}
