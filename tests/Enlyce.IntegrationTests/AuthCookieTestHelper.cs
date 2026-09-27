using System.Net.Http.Headers;

namespace Enlyce.IntegrationTests;

internal static class AuthCookieTestHelper
{
    public static string ReadToken(HttpResponseMessage response)
    {
        var cookie = response.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("_enlyce_auth=", StringComparison.Ordinal));
        return cookie.Split(';', 2)[0]["_enlyce_auth=".Length..];
    }

    public static void Authenticate(HttpClient client, HttpResponseMessage response) =>
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ReadToken(response));
}
