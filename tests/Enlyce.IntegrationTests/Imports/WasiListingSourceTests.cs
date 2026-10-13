using System.Net;
using System.Text;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Imports;
using Enlyce.Infrastructure.Imports;
using Microsoft.Extensions.Options;

namespace Enlyce.IntegrationTests.Imports;

public sealed class WasiListingSourceTests
{
    // Forma real de /v1/property/search (recortada) observada el 2026-10-12.
    private static string Property(int id, int type = 7, string forSale = "true", string forRent = "false",
        string builtArea = "336", string availability = "Available") => $$"""
        {
          "id_property": {{id}}, "for_sale": "{{forSale}}", "for_rent": "{{forRent}}",
          "id_property_type": {{type}}, "city_label": "San Vicente", "zone_label": "Vereda Las Frias",
          "iso_currency": "COP", "title": "Finca en venta San Vicente Antioquia ",
          "address": "San vicente antioquia ,vereda las frias ",
          "area": "22000", "built_area": "{{builtArea}}", "sale_price": "3500000000", "rent_price": "0",
          "bedrooms": "3", "bathrooms": "2", "garages": "10", "stratum": "3",
          "observations": "<p>Descubre esta finca id&oacute;nea.<\/p>\n\n<p>Con jacuzzi &amp; chimenea.<\/p>",
          "latitude": "6.285502952585062", "longitude": "-75.33420982064419",
          "availability_label": "{{availability}}",
          "galleries": [ { "id": 9855953,
            "1": { "id": 2, "url": "https://image.wasi.co/s2", "url_original": "https://image.wasi.co/o2", "position": 2, "description": "" },
            "0": { "id": 1, "url": "https://image.wasi.co/s1", "url_original": "https://image.wasi.co/o1", "position": 1, "description": "Fachada" }
          } ]
        }
        """;

    private static string Page(int total, params string[] items) =>
        "{\"total\":" + total + "," +
        string.Concat(items.Select((item, index) => $"\"{index}\":{item},")) +
        "\"status\":\"success\"}";

    private static (WasiListingSource Source, FakeHandler Handler) Create(
        Func<HttpRequestMessage, HttpResponseMessage> respond, bool configured = true)
    {
        var handler = new FakeHandler(respond);
        var options = Options.Create(new WasiOptions
        {
            CompanyId = configured ? "123" : null,
            Token = configured ? "TOKEN_SECRETO" : null
        });
        return (new WasiListingSource(new HttpClient(handler), options), handler);
    }

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task GetListings_MapeaCamposReales()
    {
        var (source, _) = Create(_ => Json(Page(1, Property(10472500))));

        var listing = Assert.Single(await source.GetListingsAsync());

        Assert.Equal("10472500", listing.SourceId);
        Assert.Equal(TipoInmueble.Finca, listing.Tipo);
        Assert.Equal(3_500_000_000m, listing.SalePrice);
        Assert.Null(listing.RentPrice);
        Assert.Equal(336m, listing.Area);
        Assert.Equal(10, listing.Garages);
        Assert.Equal(3, listing.Stratum);
        Assert.Equal("Vereda Las Frias", listing.Neighborhood);
        Assert.Equal(6.285502952585062m, listing.Latitude);
        Assert.Equal(ExternalAvailability.Disponible, listing.Availability);
        Assert.Equal("Descubre esta finca idónea.\n\nCon jacuzzi & chimenea.", listing.Description);
        Assert.Equal(["https://image.wasi.co/o1", "https://image.wasi.co/o2"],
            listing.Photos.OrderBy(p => p.Position).Select(p => p.Url));
    }

    [Theory]
    [InlineData(20, TipoInmueble.Casa)]   // Duplex
    [InlineData(14, TipoInmueble.Apartamento)]
    [InlineData(23, TipoInmueble.Bodega)]
    [InlineData(16, TipoInmueble.Local)]
    public async Task GetListings_MapeaTipos(int wasiType, TipoInmueble expected)
    {
        var (source, _) = Create(_ => Json(Page(1, Property(1, type: wasiType))));

        Assert.Equal(expected, Assert.Single(await source.GetListingsAsync()).Tipo);
    }

    [Fact]
    public async Task GetListings_TipoDesconocido_QuedaNulo()
    {
        var (source, _) = Create(_ => Json(Page(1, Property(1, type: 34))));

        Assert.Null(Assert.Single(await source.GetListingsAsync()).Tipo);
    }

    [Fact]
    public async Task GetListings_AreaConstruidaVacia_UsaArea()
    {
        var (source, _) = Create(_ => Json(Page(1, Property(1, builtArea: ""))));

        Assert.Equal(22000m, Assert.Single(await source.GetListingsAsync()).Area);
    }

    [Fact]
    public async Task GetListings_PaginaHastaTotal_YEnviaCredencialesPorPost()
    {
        var calls = 0;
        var (source, handler) = Create(_ => Json(++calls == 1
            ? Page(3, Property(1), Property(2))
            : Page(3, Property(3))));

        var listings = await source.GetListingsAsync();

        Assert.Equal(["1", "2", "3"], listings.Select(l => l.SourceId));
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.DoesNotContain("TOKEN_SECRETO", request.Uri);
            Assert.Contains("wasi_token=TOKEN_SECRETO", request.Body);
            Assert.Contains("scope=1", request.Body);
        });
        Assert.Contains("skip=2", handler.Requests[1].Body);
    }

    [Fact]
    public async Task GetListings_StatusError_FallaRuidoso()
    {
        var (source, _) = Create(_ => Json(
            "{\"status\":\"error\",\"code\":\"0\",\"message\":\"id_company or wasi_token is wrong.\"}"));

        var error = await Assert.ThrowsAsync<ExternalSourceException>(() => source.GetListingsAsync());
        Assert.Contains("wasi_token is wrong", error.Message);
    }

    [Fact]
    public async Task GetListings_RespuestaNoJson_FallaRuidoso()
    {
        var (source, _) = Create(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html>Whoops</html>")
        });

        await Assert.ThrowsAsync<ExternalSourceException>(() => source.GetListingsAsync());
    }

    [Fact]
    public async Task GetListings_HttpError_FallaRuidoso()
    {
        var (source, _) = Create(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<ExternalSourceException>(() => source.GetListingsAsync());
    }

    [Fact]
    public async Task GetListings_SinConfigurar_FallaRuidoso()
    {
        var (source, handler) = Create(_ => Json(Page(0)), configured: false);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => source.GetListingsAsync());
        Assert.Contains("SIN CONFIGURAR", error.Message);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("http://image.wasi.co/foto")]
    [InlineData("https://169.254.169.254/latest/meta-data")]
    [InlineData("https://wasi.co.evil.test/foto")]
    [InlineData("no-es-url")]
    public async Task DownloadPhoto_FueraDeWasi_SeRechazaSinPeticion(string url)
    {
        var (source, handler) = Create(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ExternalSourceException>(() =>
            source.DownloadPhotoAsync(new ExternalPhoto(url, 1, "")));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task DownloadPhoto_DevuelveContenido()
    {
        var (source, _) = Create(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) };
            response.Content.Headers.ContentType = new("image/jpeg");
            return response;
        });

        var photo = await source.DownloadPhotoAsync(new ExternalPhoto("https://image.wasi.co/o1", 4, ""));

        Assert.Equal("image/jpeg", photo.ContentType);
        Assert.Equal(3, photo.SizeBytes);
        Assert.Equal("wasi-4.jpg", photo.FileName);
    }

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(HttpMethod Method, string Uri, string Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.Method, request.RequestUri!.ToString(), body));
            return respond(request);
        }
    }
}
