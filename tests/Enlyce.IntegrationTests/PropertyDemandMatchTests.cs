using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

// Cruce inverso inmueble -> demandas: misma regla que demanda -> inmuebles
public sealed class PropertyDemandMatchTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task ReturnsOnlyCompatibleActiveDemandsConsistentWithForwardMatch()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var city = $"Ciudad-{Guid.NewGuid():N}";
            var property = CreateProperty(city, 400_000_000);
            var (contact, lead) = SeedContact(advisor.Id);
            var matching = Demand(contact.Id, lead.Id, city, 300_000_000, 500_000_000);
            var tooCheap = Demand(contact.Id, lead.Id, city, null, 200_000_000);
            var otherCity = Demand(contact.Id, lead.Id, "Otra", null, null);
            var inactive = Demand(contact.Id, lead.Id, city, null, null);
            inactive.Deactivate();
            Seed(property, matching, tooCheap, otherCity, inactive);

            var reverse = await client.GetFromJsonAsync<MatchDto[]>($"/api/inmuebles/{property.Id}/demandas");
            Assert.NotNull(reverse);
            var only = Assert.Single(reverse);
            Assert.Equal(matching.Id, only.Demand.Id);
            Assert.Equal(contact.Name, only.ContactName);
            Assert.Null(only.RelationshipStatus);

            foreach (var demand in new[] { matching, tooCheap, otherCity })
            {
                var forward = await client.GetFromJsonAsync<ForwardDto[]>($"/api/demandas/{demand.Id}/coincidencias");
                Assert.NotNull(forward);
                Assert.Equal(demand == matching, forward.Any(item => item.PropertyId == property.Id));
            }
        }
    }

    [Fact]
    public async Task ReturnsEmptyForUnavailablePropertyAndNotFoundForMissing()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var city = $"Ciudad-{Guid.NewGuid():N}";
            var property = CreateProperty(city, 400_000_000);
            property.MarcarVendido();
            var (contact, lead) = SeedContact(advisor.Id);
            Seed(property, Demand(contact.Id, lead.Id, city, null, null));

            var sold = await client.GetFromJsonAsync<MatchDto[]>($"/api/inmuebles/{property.Id}/demandas");
            Assert.NotNull(sold);
            Assert.Empty(sold);

            var missing = await client.GetAsync($"/api/inmuebles/{Guid.NewGuid()}/demandas");
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }
    }

    [Fact]
    public async Task AdvisorsOnlySeeTheirOwnDemandsAndAdminSeesAll()
    {
        var (clientA, advisorA) = factory.CreateAuthenticatedClient("Asesor");
        var (clientB, advisorB) = factory.CreateAuthenticatedClient("Asesor");
        var (admin, _) = factory.CreateAuthenticatedClient("Administrador");
        using (clientA)
        using (clientB)
        using (admin)
        {
            var city = $"Ciudad-{Guid.NewGuid():N}";
            var property = CreateProperty(city, 400_000_000);
            var (contactA, leadA) = SeedContact(advisorA.Id);
            var (contactB, leadB) = SeedContact(advisorB.Id);
            var demandA = Demand(contactA.Id, leadA.Id, city, null, null);
            var demandB = Demand(contactB.Id, leadB.Id, city, null, null);
            Seed(property, demandA, demandB);

            var url = $"/api/inmuebles/{property.Id}/demandas";
            var seenA = await clientA.GetFromJsonAsync<MatchDto[]>(url);
            var seenB = await clientB.GetFromJsonAsync<MatchDto[]>(url);
            var seenAdmin = await admin.GetFromJsonAsync<MatchDto[]>(url);

            Assert.Equal([demandA.Id], seenA!.Select(item => item.Demand.Id));
            Assert.Equal([demandB.Id], seenB!.Select(item => item.Demand.Id));
            Assert.Equal(new[] { demandA.Id, demandB.Id }.Order(), seenAdmin!.Select(item => item.Demand.Id).Order());

            var foreignLink = await clientB.PostAsync($"/api/demandas/{demandA.Id}/inmuebles/{property.Id}", null);
            Assert.Equal(HttpStatusCode.Forbidden, foreignLink.StatusCode);
        }
    }

    [Fact]
    public async Task FollowUpFromPropertySideIsPersistedAndVisibleInBothDirections()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var city = $"Ciudad-{Guid.NewGuid():N}";
            var property = CreateProperty(city, 400_000_000);
            var (contact, lead) = SeedContact(advisor.Id);
            var demand = Demand(contact.Id, lead.Id, city, null, null);
            Seed(property, demand);

            var linked = await client.PostAsync($"/api/demandas/{demand.Id}/inmuebles/{property.Id}", null);
            Assert.Equal(HttpStatusCode.Created, linked.StatusCode);
            var status = await client.PutAsJsonAsync(
                $"/api/demandas/{demand.Id}/inmuebles/{property.Id}/estado", new { status = "Shared" });
            Assert.Equal(HttpStatusCode.OK, status.StatusCode);

            var reverse = await client.GetFromJsonAsync<MatchDto[]>($"/api/inmuebles/{property.Id}/demandas");
            var forward = await client.GetFromJsonAsync<ForwardDto[]>($"/api/demandas/{demand.Id}/coincidencias");
            Assert.Equal("Shared", Assert.Single(reverse!).RelationshipStatus);
            Assert.Equal("Shared", Assert.Single(forward!, item => item.PropertyId == property.Id).RelationshipStatus);

            using var scope = factory.Services.CreateScope();
            var stored = await scope.ServiceProvider.GetRequiredService<EnlyceDbContext>()
                .DemandPropertyLinks.SingleAsync(item => item.DemandId == demand.Id && item.PropertyId == property.Id);
            Assert.Equal(DemandPropertyStatus.Shared, stored.Status);
        }
    }

    private static Inmueble CreateProperty(string city, decimal price) =>
        Inmueble.Crear("Apartamento cruce", "", TipoInmueble.Apartamento, ModalidadInmueble.Venta,
            Direccion.Crear("Calle 1", city, "Centro"), Dinero.Crear(price), 80, 2, 2, 1,
            TestWebApplicationFactory.PropietarioSeedId);

    private static CustomerDemand Demand(Guid contactId, Guid leadId, string city, decimal? min, decimal? max) =>
        CustomerDemand.Create(contactId, leadId, ModalidadInmueble.Venta, TipoInmueble.Apartamento,
            city, null, min, max, 2, null, null, null);

    private void Seed(Inmueble property, params CustomerDemand[] demands)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        db.Inmuebles.Add(property);
        db.CustomerDemands.AddRange(demands);
        db.SaveChanges();
    }

    private (Contact Contact, Lead Lead) SeedContact(Guid advisorId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var email = Email.Create($"cruce-{Guid.NewGuid():N}@test.com");
        var contact = Contact.Create("Contacto cruce", email, Telefono.Create("3101234567"));
        var lead = Lead.Crear(contact.Name, email, Telefono.Create("3101234567"), "Test", true,
            contactId: contact.Id);
        lead.AsignarAsesor(advisorId);
        db.Contacts.Add(contact);
        db.Leads.Add(lead);
        db.SaveChanges();
        return (contact, lead);
    }

    private sealed record DemandDto(Guid Id);
    private sealed record MatchDto(DemandDto Demand, string ContactName, string? RelationshipStatus);
    private sealed record ForwardDto(Guid PropertyId, string? RelationshipStatus);
}
