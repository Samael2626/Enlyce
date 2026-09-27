using System.Security.Cryptography;
using System.Text;

namespace Enlyce.Application.Security;

internal static class OwnerInquiryContinuationToken
{
    private const int TokenBytes = 32;

    public static (string Token, string Hash) Issue()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenBytes);
        var token = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return (token, Hash(token));
    }

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
