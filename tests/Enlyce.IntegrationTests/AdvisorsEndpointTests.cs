using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class AdvisorsEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task AdvisorRosterIsAdminOnlyAndReturnsMinimalActiveAdvisorData()
    {
        var (advisorClient, advisor) = factory.CreateAuthenticatedClient("Asesor");
        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        using (advisorClient)
        using (adminClient)
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await advisorClient.GetAsync("/api/asesores")).StatusCode);
            var advisors = await adminClient.GetFromJsonAsync<AdvisorDto[]>("/api/asesores");

            Assert.NotNull(advisors);
            Assert.Contains(advisors, item => item.Id == advisor.Id && item.Name == advisor.Nombre);
        }
    }

    private sealed record AdvisorDto(Guid Id, string Name);
}
