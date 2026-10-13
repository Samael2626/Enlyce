using System.Net;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class MediaCleanupEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public MediaCleanupEndpointTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CleanupRequiresAdministratorRole()
    {
        var client = _factory.CreateAuthenticatedClient("Asesor").Client;

        var response = await client.PostAsync("/api/media/cleanup-orphans", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CleanupWithLocalStorageReturnsUnavailableWithoutStartingWork()
    {
        var client = _factory.CreateAuthenticatedClient("Administrador").Client;

        var response = await client.PostAsync("/api/media/cleanup-orphans", null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
