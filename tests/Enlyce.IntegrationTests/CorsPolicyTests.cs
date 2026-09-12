using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class CorsPolicyTests
{
    [Fact]
    public async Task DefaultPolicy_AllowsLocalhostButRejectsTryCloudflare()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.True(await AllowsOrigin(client, "http://localhost:4173"));
        Assert.False(await AllowsOrigin(client, "https://demo.trycloudflare.com"));
    }

    [Fact]
    public async Task DemoPolicy_AllowsTryCloudflareHostOnlyOverHttps()
    {
        using var factory = new TestWebApplicationFactory();
        using var demoFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cors:AllowTrycloudflareOrigins"] = "true"
                })));
        using var client = demoFactory.CreateClient();
        Assert.True(demoFactory.Services.GetRequiredService<IConfiguration>()
            .GetValue<bool>("Cors:AllowTrycloudflareOrigins"));

        Assert.True(await AllowsOrigin(client, "http://localhost:4173"));
        Assert.True(await AllowsOrigin(client, "https://demo.trycloudflare.com"));
        Assert.False(await AllowsOrigin(client, "http://demo.trycloudflare.com"));
        Assert.False(await AllowsOrigin(client, "https://demo.trycloudflare.com.evil.test"));
        Assert.False(await AllowsOrigin(client, "https://nested.demo.trycloudflare.com"));
        Assert.False(await AllowsOrigin(client, "https://demo.trycloudflare.com:8443"));
    }

    private static async Task<bool> AllowsOrigin(HttpClient client, string origin)
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/public/inmuebles");
        request.Headers.TryAddWithoutValidation("Origin", origin);
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");
        using var response = await client.SendAsync(request);

        return response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values) &&
               values.Single() == origin;
    }
}
