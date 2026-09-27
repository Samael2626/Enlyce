using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.UseCases.GetLeadById;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public class LeadsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public LeadsEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient().Client;
    }

    [Fact]
    public async Task PostLead_ValidRequest_ReturnsOpaqueAcceptedResponse()
    {
        var request = new CreateLeadRequest(
            "Juan Perez", "juan@test.com", "3101234567", "Web", true);

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PublicLeadSubmissionResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.ContinuationToken));
    }

    [Fact]
    public async Task PostLead_DoesNotRevealInternalLocation()
    {
        var request = new CreateLeadRequest(
            "Maria Lopez", "maria@test.com", null, "Referido", true);

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Null(response.Headers.Location);
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
        var created = await FindLeadByEmailAsync("testget@test.com");

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
        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

        var created = await FindLeadByEmailAsync("pedro@test.com");

        // 2. Get
        var getResponse = await _client.GetAsync($"/api/leads/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var lead = await getResponse.Content.ReadFromJsonAsync<GetLeadByIdResponse>();
        Assert.Equal("Pedro Garcia", lead!.Nombre);
        Assert.Equal("pedro@test.com", lead.Email);
        Assert.True(lead.AutorizacionDatos);
        Assert.True(lead.Activo);
    }

    [Fact]
    public async Task PostOwnerLeads_PersistsRentAndManageAsDistinctStructuredValues()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var rentEmail = $"rent-{suffix}@test.com";
        var manageEmail = $"manage-{suffix}@test.com";

        var rentResponse = await _client.PostAsJsonAsync("/api/leads", new CreateLeadRequest(
            "Propietario Arriendo",
            rentEmail,
            "3101111111",
            "WebsiteOwner:Apartamento:Medellin:Laureles",
            true,
            "Arriendo",
            "Rent"));
        var manageResponse = await _client.PostAsJsonAsync("/api/leads", new CreateLeadRequest(
            "Propietario Administracion",
            manageEmail,
            "3102222222",
            "WebsiteOwner:Apartamento:Medellin:Belen",
            true,
            "Arriendo",
            "Manage"));

        Assert.Equal(HttpStatusCode.Accepted, rentResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, manageResponse.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT "OwnerService"
            FROM "Leads"
            WHERE "Email" IN ($rentEmail, $manageEmail)
            ORDER BY "OwnerService"
            """;
        var rentParameter = command.CreateParameter();
        rentParameter.ParameterName = "$rentEmail";
        rentParameter.Value = rentEmail;
        command.Parameters.Add(rentParameter);
        var manageParameter = command.CreateParameter();
        manageParameter.ParameterName = "$manageEmail";
        manageParameter.Value = manageEmail;
        command.Parameters.Add(manageParameter);

        var values = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            values.Add(reader.GetString(0));

        Assert.Equal(["Manage", "Rent"], values);
    }

    [Fact]
    public async Task PostOwnerLead_WithOperationMismatch_ReturnsBadRequest()
    {
        var request = new CreateLeadRequest(
            "Propietario Invalido",
            $"invalid-{Guid.NewGuid():N}@test.com",
            "3103333333",
            "WebsiteOwner",
            true,
            "Venta",
            "Manage");

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutOwnerDetails_AfterMinimalCapture_EnrichesSameOpportunity()
    {
        var email = $"owner-details-{Guid.NewGuid():N}@test.com";
        var createResponse = await _client.PostAsJsonAsync("/api/leads", new CreateLeadRequest(
            "Propietario Progresivo",
            email,
            "3105551111",
            "PropietarioWeb:Vender",
            true,
            "Venta",
            "Sell"));
        var created = await createResponse.Content.ReadFromJsonAsync<PublicLeadSubmissionResponse>();

        var enrichResponse = await _client.PutAsJsonAsync(
            "/api/leads/owner-details",
            new EnrichOwnerInquiryRequest(
                created!.ContinuationToken,
                "Apartment",
                "Medellin",
                "Laureles",
                650_000_000m,
                "Apartamento remodelado y con balcon.",
                "WhatsApp"));

        Assert.Equal(HttpStatusCode.Accepted, enrichResponse.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var lead = await db.Leads.AsNoTracking().SingleAsync(item => item.Email.Value == email);
        Assert.Equal(OwnerPropertyType.Apartment, lead.OwnerPropertyType);
        Assert.Equal("Medellin", lead.OwnerPropertyCity);
        Assert.Equal("Laureles", lead.OwnerPropertyNeighborhood);
        Assert.Equal(650_000_000m, lead.OwnerExpectedPrice);
        Assert.Equal(PreferredContactChannel.WhatsApp, lead.OwnerPreferredContactChannel);
    }

    [Fact]
    public async Task PutOwnerDetails_WithInvalidToken_ReturnsAcceptedAndKeepsDataEmpty()
    {
        var email = $"owner-protected-{Guid.NewGuid():N}@test.com";
        var createResponse = await _client.PostAsJsonAsync("/api/leads", new CreateLeadRequest(
            "Propietario Protegido",
            email,
            null,
            "PropietarioWeb:Administrar",
            true,
            "Arriendo",
            "Manage"));
        var response = await _client.PutAsJsonAsync(
            "/api/leads/owner-details",
            new EnrichOwnerInquiryRequest(
                "invalid-token",
                "House",
                "Medellin",
                null,
                null,
                null,
                "Phone"));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var lead = await db.Leads.AsNoTracking().SingleAsync(item => item.Email.Value == email);
        Assert.Null(lead.OwnerPropertyType);
    }

    [Fact]
    public async Task PostVisitLead_WithPublishedPublication_PersistsReferenceAndAssignsAdvisor()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        await PublicCatalogTestData.SeedAsync(db);
        var publication = await db.PropertyPublications
            .SingleAsync(item => item.Slug == PublicCatalogTestData.PublishedApartmentSlug);
        var email = $"visit-{Guid.NewGuid():N}@test.com";
        var request = new CreateLeadRequest(
            "Visitante Publicacion",
            email,
            "3104444444",
            $"Website:{publication.Slug}",
            true,
            "Venta",
            null,
            publication.Id.ToString());

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var result = await db.Leads.AsNoTracking().SingleAsync(item => item.Email.Value == email);
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT "PublicationId", "AsesorAsignadoId"
            FROM "Leads"
            WHERE "Id" = $leadId
            """;
        var leadParameter = command.CreateParameter();
        leadParameter.ParameterName = "$leadId";
        leadParameter.Value = result.Id;
        command.Parameters.Add(leadParameter);
        await using var reader = await command.ExecuteReaderAsync();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(publication.Id, Guid.Parse(reader.GetString(0)));
        Assert.Equal(publication.AdvisorId, Guid.Parse(reader.GetString(1)));
    }

    [Fact]
    public async Task PostVisitLead_WithMissingPublication_CreatesUnassignedLead()
    {
        var publicationId = Guid.NewGuid();
        var email = $"missing-{Guid.NewGuid():N}@test.com";
        var request = new CreateLeadRequest(
            "Visitante Sin Publicacion",
            email,
            null,
            "Website:retirada",
            true,
            "Venta",
            null,
            publicationId.ToString());

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var lead = await db.Leads.AsNoTracking().SingleAsync(item => item.Email.Value == email);
        Assert.Equal(publicationId, lead.PublicationId);
        Assert.Null(lead.AsesorAsignadoId);
        Assert.Contains("PublicationReview:", lead.Fuente);
    }

    [Fact]
    public async Task PostVisitLead_WithMalformedPublicationId_CreatesLeadForReview()
    {
        var email = $"invalid-publication-{Guid.NewGuid():N}@test.com";
        var request = new CreateLeadRequest(
            "Visitante Id Invalido",
            email,
            null,
            "Website:referencia-manual",
            true,
            "Venta",
            null,
            "not-a-guid");

        var response = await _client.PostAsJsonAsync("/api/leads", request);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var lead = await db.Leads.AsNoTracking().SingleAsync(item => item.Email.Value == email);
        Assert.Null(lead.PublicationId);
        Assert.Null(lead.AsesorAsignadoId);
        Assert.Contains("PublicationReview:not-a-guid", lead.Fuente);
    }

    private async Task<Lead> FindLeadByEmailAsync(string email)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        return await db.Leads.AsNoTracking().SingleAsync(item => item.Email.Value == email);
    }
}
