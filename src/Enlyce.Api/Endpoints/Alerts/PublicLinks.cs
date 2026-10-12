using System.Security.Cryptography;
using System.Text;
using Enlyce.Application.Abstractions;
using Enlyce.Application.Auth;
using Microsoft.Extensions.Options;

namespace Enlyce.Api.Endpoints.Alerts;

public sealed class PublicSiteOptions
{
    public const string SectionName = "PublicSite";
    public string BaseUrl { get; init; } = "http://localhost:3000";
}

// El token de baja es un HMAC del id de la alerta: no requiere guardar nada y
// no se puede fabricar sin la clave del servidor.
public sealed class PublicLinks : IPublicLinks
{
    private readonly string _baseUrl;
    private readonly byte[] _key;

    public PublicLinks(IOptions<PublicSiteOptions> site, IOptions<JwtSettings> jwt)
    {
        _baseUrl = site.Value.BaseUrl.TrimEnd('/');
        // Clave derivada: un token de baja nunca sirve como firma JWT ni al reves.
        _key = HMACSHA256.HashData(Encoding.UTF8.GetBytes(jwt.Value.SecretKey ?? string.Empty),
            Encoding.UTF8.GetBytes("enlyce:alert-unsubscribe:v1"));
    }

    public string PropertyUrl(string slug) => $"{_baseUrl}/inmuebles/{Uri.EscapeDataString(slug)}";

    public string UnsubscribeAlertUrl(Guid demandId) =>
        $"{_baseUrl}/alertas/baja?id={demandId}&token={Token(demandId)}";

    public bool IsValidUnsubscribeToken(Guid demandId, string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;
        var expected = Encoding.ASCII.GetBytes(Token(demandId));
        var given = Encoding.ASCII.GetBytes(token);
        return CryptographicOperations.FixedTimeEquals(expected, given);
    }

    private string Token(Guid demandId) =>
        Convert.ToHexStringLower(HMACSHA256.HashData(_key, demandId.ToByteArray()));
}
