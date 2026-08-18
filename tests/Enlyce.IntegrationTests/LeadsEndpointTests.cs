using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.UseCases.GetLeadById;
using Xunit;

namespace Enlyce.IntegrationTests;

public class LeadsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LeadsEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostLead_ValidRequest_ReturnsCreated()
    {
        var request = new CreateLeadRequest(
            "Juan Perez", "juan@test.com", "3101234567", "Web", true);

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateLeadResponse>();
        Assert.NotNull(result);
        Assert.Equal("Juan Perez", result!.Nombre);
        Assert.Equal("juan@test.com", result.Email);
        Assert.Equal("Nuevo", result.Estado);
    }

    [Fact]
    public async Task PostLead_ReturnsLocationHeader()
    {
        var request = new CreateLeadRequest(
            "Maria Lopez", "maria@test.com", null, "Referido", true);

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/leads/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task PostLead_WithoutAuthorization_ReturnsBadRequest()
    {
        var request = new CreateLeadRequest(
            "Sin Auth", "sin@test.com", null, "Web", false);

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostLead_EmptyName_ReturnsBadRequest()
    {
        var request = new CreateLeadRequest(
            "", "test@test.com", null, "Web", true);

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetLead_ExistingId_ReturnsOk()
    {
        // Create a lead first
        var createRequest = new CreateLeadRequest(
            "Test Lead", "testget@test.com", "3109999888", "Web", true);
        var createResponse = await _client.PostAsJsonAsync("/api/leads", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateLeadResponse>();

        // Get it
        var getResponse = await _client.GetAsync($"/api/leads/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var lead = await getResponse.Content.ReadFromJsonAsync<GetLeadByIdResponse>();
        Assert.NotNull(lead);
        Assert.Equal("Test Lead", lead!.Nombre);
        Assert.Equal("testget@test.com", lead.Email);
    }

    [Fact]
    public async Task GetLead_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/leads/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullFlow_CreateGetLead()
    {
        // 1. Create
        var request = new CreateLeadRequest(
            "Pedro Garcia", "pedro@test.com", "3105555666", "Facebook", true);
        var createResponse = await _client.PostAsJsonAsync("/api/leads", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateLeadResponse>();
        Assert.NotNull(created);

        // 2. Get
        var getResponse = await _client.GetAsync($"/api/leads/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var lead = await getResponse.Content.ReadFromJsonAsync<GetLeadByIdResponse>();
        Assert.Equal("Pedro Garcia", lead!.Nombre);
        Assert.Equal("pedro@test.com", lead.Email);
        Assert.True(lead.AutorizacionDatos);
        Assert.True(lead.Activo);
    }
}
