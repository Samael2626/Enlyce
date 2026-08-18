namespace Enlyce.Application.Auth;

public sealed class JwtSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = "Enlyce";
    public string Audience { get; init; } = "Enlyce";
    public int ExpirationMinutes { get; init; } = 480;
}
