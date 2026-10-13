using System.Net;

namespace Enlyce.IntegrationTests.Imports;

public sealed class ImportEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ImportEndpointTests(TestWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData("POST", "/api/import/wasi")]
    [InlineData("GET", "/api/import/wasi/status")]
    public async Task SinSesion_Responde401(string method, string path)
    {
        using var client = _factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
