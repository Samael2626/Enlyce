using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Enlyce.Api.Endpoints.Publications;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class PropertyPublicationsAdminEndpointTests
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public PropertyPublicationsAdminEndpointTests(TestWebApplicationFactory factory) =>
        _factory = factory;

    [Fact]
    public async Task Advisor_CreatesDraftAndListsOnlyOwnPublications()
    {
        var (client, advisor) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var property = SeedProperty();
            var slug = $"casa-{Guid.NewGuid():N}";
            var response = await client.PostAsJsonAsync("/api/publicaciones", Request(property.Id, null, slug));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var list = await client.GetStringAsync("/api/publicaciones");
            Assert.Contains(slug, list);
            Assert.Contains(advisor.Nombre, list);
        }
    }

    [Fact]
    public async Task Advisor_CannotEditAnotherAdvisorsPublication()
    {
        var (ownerClient, owner) = _factory.CreateAuthenticatedClient("Asesor");
        ownerClient.Dispose();
        var publication = SeedPublication(owner.Id);
        var (client, _) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.PutAsJsonAsync(
                $"/api/publicaciones/{publication.Id}",
                UpdateRequest("slug-ajeno"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Admin_PublishesPausesAndWithdrawsReadyPublication()
    {
        var (client, admin) = _factory.CreateAuthenticatedClient("Administrador");
        using (client)
        {
            var publication = SeedPublication(admin.Id, ready: true);

            Assert.Equal(HttpStatusCode.OK, (await ChangeStatus(client, publication.Id, "publish")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await ChangeStatus(client, publication.Id, "pause")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await ChangeStatus(client, publication.Id, "withdraw")).StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            Assert.Equal(PublicationStatus.Withdrawn, db.PropertyPublications.Single(item => item.Id == publication.Id).Status);
        }
    }

    [Fact]
    public async Task Advisor_CannotUploadPhotoToAnotherAdvisorsPublication()
    {
        var (ownerClient, owner) = _factory.CreateAuthenticatedClient("Asesor");
        ownerClient.Dispose();
        var publication = SeedPublication(owner.Id);
        var (client, _) = _factory.CreateAuthenticatedClient("Asesor");
        using (client)
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new ByteArrayContent([1, 2, 3]), "archivo", "foto.png");
            form.Add(new StringContent("Fachada"), "textoAlternativo");
            form.Add(new StringContent("true"), "esPortada");

            var response = await client.PostAsync(
                $"/api/publicaciones/{publication.Id}/fotos", form);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Anonymous_PublicationManagementReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/publicaciones")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/publicaciones/opciones")).StatusCode);
    }

    private Inmueble SeedProperty()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var property = Inmueble.Crear(
            $"Casa {Guid.NewGuid():N}",
            "Descripcion",
            TipoInmueble.Casa,
            ModalidadInmueble.Venta,
            Direccion.Crear("Calle 10", "Medellin", "Laureles"),
            Dinero.Crear(700_000_000m),
            120,
            3,
            2,
            1,
            TestWebApplicationFactory.PropietarioSeedId);
        db.Inmuebles.Add(property);
        db.SaveChanges();
        return property;
    }

    private PropertyPublication SeedPublication(Guid advisorId, bool ready = false)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var property = Inmueble.Crear(
            $"Apartamento {Guid.NewGuid():N}",
            "Descripcion",
            TipoInmueble.Apartamento,
            ModalidadInmueble.Venta,
            Direccion.Crear("Carrera 20", "Medellin", "Laureles"),
            Dinero.Crear(620_000_000m),
            90,
            3,
            2,
            1,
            TestWebApplicationFactory.PropietarioSeedId);
        var publication = PropertyPublication.Create(
            property.Id,
            advisorId,
            $"apartamento-{Guid.NewGuid():N}",
            property.Nombre,
            property.Descripcion);
        publication.SetPublicPrice(620_000_000m);
        publication.SetPublicLocation("Medellin", "Laureles", 6.244m, -75.593m);
        if (ready)
            for (var index = 0; index < 3; index++)
                publication.AddPhoto($"https://media.enlyce.test/foto-{index}.webp", $"Foto {index}", index, index == 0);

        db.Inmuebles.Add(property);
        db.PropertyPublications.Add(publication);
        db.SaveChanges();
        return publication;
    }

    private static CreatePropertyPublicationRequest Request(Guid propertyId, Guid? advisorId, string slug) =>
        new(propertyId, advisorId, slug, "Casa publica", "Descripcion publica",
            700_000_000m, "COP", "Medellin", "Laureles", 6.244m, -75.593m);

    private static UpdatePropertyPublicationRequest UpdateRequest(string slug) =>
        new(slug, "Titulo actualizado", "Descripcion actualizada",
            710_000_000m, "COP", "Medellin", "Laureles", 6.244m, -75.593m);

    private static Task<HttpResponseMessage> ChangeStatus(HttpClient client, Guid id, string action) =>
        client.PutAsJsonAsync($"/api/publicaciones/{id}/estado", new { action });
}
