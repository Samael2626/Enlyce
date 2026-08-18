using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.Inmuebles;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Application.UseCases.GetInmuebleById;
using Enlyce.Domain.Entities;
using Xunit;

namespace Enlyce.IntegrationTests;

public class InmueblesEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Guid _propietarioId = TestWebApplicationFactory.PropietarioSeedId;

    public InmueblesEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostInmueble_ValidRequest_ReturnsCreated()
    {
        var request = new CreateInmuebleRequest(
            "Apto Chapinero", "3 hab, buen estado", TipoInmueble.Apartamento,
            ModalidadInmueble.Venta, "Cra 10 #5-20", "Bogota", "Chapinero",
            500_000_000m, "COP", 80, 3, 2, 1, _propietarioId);

        var response = await _client.PostAsJsonAsync("/api/inmuebles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateInmuebleResponse>();
        Assert.NotNull(result);
        Assert.Equal("Apto Chapinero", result!.Nombre);
        Assert.Equal("Apartamento", result.Tipo);
    }

    [Fact]
    public async Task PostInmueble_ReturnsLocationHeader()
    {
        var request = new CreateInmuebleRequest(
            "Cta Norte", null, TipoInmueble.Casa,
            ModalidadInmueble.Arriendo, "Calle 80 #15-30", "Medellin", null,
            2_500_000m, "COP", 120, 4, 3, 2, _propietarioId);

        var response = await _client.PostAsJsonAsync("/api/inmuebles", request);

        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/inmuebles/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task PostInmueble_EmptyName_ReturnsBadRequest()
    {
        var request = new CreateInmuebleRequest(
            "", "desc", TipoInmueble.Casa, ModalidadInmueble.Venta,
            "Calle 10", "Bogota", null, 100m, "COP", 50, 2, 1, 1, _propietarioId);

        var response = await _client.PostAsJsonAsync("/api/inmuebles", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInmueble_ExistingId_ReturnsOk()
    {
        var createRequest = new CreateInmuebleRequest(
            "Local Centro", "Local comercial", TipoInmueble.Local,
            ModalidadInmueble.Arriendo, "Cra 7 #40-50", "Bogota", "Centro",
            3_000_000m, "COP", 60, 1, 1, 1, _propietarioId);
        var createResponse = await _client.PostAsJsonAsync("/api/inmuebles", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateInmuebleResponse>();

        var getResponse = await _client.GetAsync($"/api/inmuebles/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var inmueble = await getResponse.Content.ReadFromJsonAsync<GetInmuebleByIdResponse>();
        Assert.NotNull(inmueble);
        Assert.Equal("Local Centro", inmueble!.Nombre);
        Assert.Equal("Local", inmueble.Tipo);
    }

    [Fact]
    public async Task GetInmueble_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/inmuebles/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullFlow_CreateGetInmueble()
    {
        // 1. Create
        var request = new CreateInmuebleRequest(
            "Finca La Paz", "Finca campestre", TipoInmueble.Finca,
            ModalidadInmueble.Venta, "Km 5 via Marulanda", "Retiro", null,
            800_000_000m, "COP", 5000, 5, 4, 3, _propietarioId);
        var createResponse = await _client.PostAsJsonAsync("/api/inmuebles", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateInmuebleResponse>();
        Assert.NotNull(created);

        // 2. Get
        var getResponse = await _client.GetAsync($"/api/inmuebles/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var inmueble = await getResponse.Content.ReadFromJsonAsync<GetInmuebleByIdResponse>();
        Assert.Equal("Finca La Paz", inmueble!.Nombre);
        Assert.Equal("Finca", inmueble.Tipo);
        Assert.Equal("Venta", inmueble.Modalidad);
        Assert.Equal(5000, inmueble.MetrosCuadrados);
        Assert.True(inmueble.Activo);
    }
}
