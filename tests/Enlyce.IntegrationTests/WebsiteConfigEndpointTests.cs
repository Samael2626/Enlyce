using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.WebsiteConfig;

namespace Enlyce.IntegrationTests;

public sealed class WebsiteConfigEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private const string PublicUrl = "/api/public/sitio-web";
    private const string AdminUrl = "/api/configuracion/sitio-web";

    [Fact]
    public async Task GetPublicConfig_Anonymous_ReturnsOkWithDefaultOrStoredConfig()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync(PublicUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var config = await response.Content.ReadFromJsonAsync<PublicWebsiteConfigResponse>();
        Assert.NotNull(config);
        Assert.NotEmpty(config.AgencyName);
        Assert.NotEmpty(config.HeroTitle);
    }

    [Fact]
    public async Task GetAdminConfig_Anonymous_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync(AdminUrl);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminConfig_AdvisorRole_ReturnsForbidden()
    {
        var (advisorClient, _) = factory.CreateAuthenticatedClient("Asesor");
        using (advisorClient)
        {
            var response = await advisorClient.GetAsync(AdminUrl);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task PutAdminConfig_InvalidPayload_ReturnsBadRequest()
    {
        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        using (adminClient)
        {
            var invalidRequest = new UpdateWebsiteConfigRequest(
                "", null, "", null, null, null, null, "", null, "", null, null, null, null, null, null, null);
            var response = await adminClient.PutAsJsonAsync(AdminUrl, invalidRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task PutAdminConfig_AdminUser_UpdatesAndReflectsInPublicEndpoint()
    {
        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        var updatedHero = $"Propiedades Exclusivas {Guid.NewGuid():N}";
        var updatedSlogan = "Asesoría Inmobiliaria de Élite";

        using (adminClient)
        {
            var request = new UpdateWebsiteConfigRequest(
                "L&C Inmobiliaria Test",
                updatedSlogan,
                updatedHero,
                "Subtítulo actualizado",
                "/custom-banner.jpg",
                "Sobre Nosotros",
                "Texto corporativo detallado",
                "+57 300 123 4567",
                "573001234567",
                "info@test.com",
                "Calle 10 # 40-20, Medellín",
                "https://instagram.com/test",
                "https://facebook.com/test",
                "L&C Test SEO Title",
                "Meta descripción de prueba",
                "#0f1a3d",
                "#b78a2d");

            var response = await adminClient.PutAsJsonAsync(AdminUrl, request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<PublicWebsiteConfigResponse>();
            Assert.NotNull(updated);
            Assert.Equal(updatedHero, updated.HeroTitle);
            Assert.Equal(updatedSlogan, updated.Slogan);
            Assert.Equal("/custom-banner.jpg", updated.HeroBannerUrl);
        }

        // Verify public endpoint immediately returns updated values
        using var publicClient = factory.CreateClient();
        var publicResponse = await publicClient.GetAsync(PublicUrl);
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);

        var publicConfig = await publicResponse.Content.ReadFromJsonAsync<PublicWebsiteConfigResponse>();
        Assert.NotNull(publicConfig);
        Assert.Equal(updatedHero, publicConfig.HeroTitle);
        Assert.Equal(updatedSlogan, publicConfig.Slogan);
    }
}
