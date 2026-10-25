using System.Net;
using System.Net.Http.Json;
using Enlyce.Api.Endpoints.Blog;

namespace Enlyce.IntegrationTests;

public sealed class BlogEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static BlogPostRequest Request(string slug, bool published) => new(
        slug, "Guía para elegir vivienda", "Consejos para comparar inmuebles en Medellín.",
        "Compare ubicación, presupuesto y estado físico antes de tomar una decisión inmobiliaria.",
        null, null, null, published);

    [Fact]
    public async Task Draft_IsVisibleOnlyToAdministrator_UntilPublished()
    {
        var slug = $"guia-{Guid.NewGuid():N}";
        var (admin, _) = factory.CreateAuthenticatedClient();
        using (admin)
        using (var publicClient = factory.CreateClient())
        {
            var created = await admin.PostAsJsonAsync("/api/configuracion/blog/", Request(slug, false));
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var draft = await created.Content.ReadFromJsonAsync<BlogPostResponse>();
            Assert.NotNull(draft);

            Assert.Equal(HttpStatusCode.NotFound,
                (await publicClient.GetAsync($"/api/public/blog/{slug}")).StatusCode);
            var publicList = await publicClient.GetFromJsonAsync<BlogPostResponse[]>("/api/public/blog");
            Assert.DoesNotContain(publicList!, post => post.Id == draft.Id);

            var updated = await admin.PutAsJsonAsync($"/api/configuracion/blog/{draft.Id}", Request(slug, true));
            Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
            Assert.Equal(HttpStatusCode.OK,
                (await publicClient.GetAsync($"/api/public/blog/{slug}")).StatusCode);
        }
    }

    [Fact]
    public async Task Advisor_CannotCreateOrListDrafts()
    {
        var (advisor, _) = factory.CreateAuthenticatedClient("Asesor");
        using (advisor)
        {
            Assert.Equal(HttpStatusCode.Forbidden,
                (await advisor.GetAsync("/api/configuracion/blog/")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden,
                (await advisor.PostAsJsonAsync("/api/configuracion/blog/", Request("asesor-post", true))).StatusCode);
        }
    }

    [Fact]
    public async Task InvalidSlugAndDuplicateSlug_AreRejected()
    {
        var slug = $"guia-{Guid.NewGuid():N}";
        var (admin, _) = factory.CreateAuthenticatedClient();
        using (admin)
        {
            Assert.Equal(HttpStatusCode.BadRequest,
                (await admin.PostAsJsonAsync("/api/configuracion/blog/", Request("Ruta Con Espacios", false))).StatusCode);
            Assert.Equal(HttpStatusCode.Created,
                (await admin.PostAsJsonAsync("/api/configuracion/blog/", Request(slug, false))).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict,
                (await admin.PostAsJsonAsync("/api/configuracion/blog/", Request(slug, false))).StatusCode);
        }
    }
}
