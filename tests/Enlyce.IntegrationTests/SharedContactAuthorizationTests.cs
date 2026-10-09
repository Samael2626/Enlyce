using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class SharedContactAuthorizationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task AdvisorsOnlyListDemandsForTheirOwnLeadsOnSharedContact()
    {
        var (clientA, advisorA) = factory.CreateAuthenticatedClient("Asesor");
        var (clientB, advisorB) = factory.CreateAuthenticatedClient("Asesor");
        using (clientA)
        using (clientB)
        {
            var (contact, leadA, leadB) = SeedSharedContact(advisorA.Id, advisorB.Id);
            var demandA = CreateDemand(contact.Id, leadA.Id, "Zona A");
            var demandB = CreateDemand(contact.Id, leadB.Id, "Zona B");
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.CustomerDemands.AddRange(demandA, demandB);
                db.SaveChanges();
            }

            var responseA = await clientA.GetFromJsonAsync<DemandDto[]>(
                $"/api/demandas?contactId={contact.Id}");
            var responseB = await clientB.GetFromJsonAsync<DemandDto[]>(
                $"/api/demandas?contactId={contact.Id}");

            Assert.NotNull(responseA);
            Assert.NotNull(responseB);
            Assert.Collection(responseA, demand => Assert.Equal(demandA.Id, demand.Id));
            Assert.Collection(responseB, demand => Assert.Equal(demandB.Id, demand.Id));

            var foreignDemandResponse = await clientB.GetAsync(
                $"/api/demandas/{demandA.Id}/coincidencias");
            Assert.Equal(HttpStatusCode.Forbidden, foreignDemandResponse.StatusCode);
        }
    }

    [Fact]
    public async Task SharedContactHistoryOnlyShowsCurrentAdvisorsInteractionsAndVisits()
    {
        var (clientA, advisorA) = factory.CreateAuthenticatedClient("Asesor");
        var (clientB, advisorB) = factory.CreateAuthenticatedClient("Asesor");
        using (clientA)
        using (clientB)
        {
            var (contact, leadA, leadB) = SeedSharedContact(advisorA.Id, advisorB.Id);
            var interactionA = Interaccion.Registrar(leadA.Id, advisorA.Id, "Llamada", "Resumen A");
            var interactionB = Interaccion.Registrar(leadB.Id, advisorB.Id, "Correo", "Resumen B");
            var visitA = Visita.Programar(leadA.Id, Guid.NewGuid(), advisorA.Id, DateTime.UtcNow.AddDays(1));
            var visitB = Visita.Programar(leadB.Id, Guid.NewGuid(), advisorB.Id, DateTime.UtcNow.AddDays(2));
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.Interacciones.AddRange(interactionA, interactionB);
                db.Visitas.AddRange(visitA, visitB);
                db.SaveChanges();
            }

            var responseA = await clientA.GetFromJsonAsync<ContactDetailDto>($"/api/contactos/{contact.Id}");
            var responseB = await clientB.GetFromJsonAsync<ContactDetailDto>($"/api/contactos/{contact.Id}");

            Assert.NotNull(responseA);
            Assert.NotNull(responseB);
            Assert.Collection(responseA.Opportunities, opportunity =>
            {
                Assert.Equal(leadA.Id, opportunity.Id);
                Assert.Collection(opportunity.Interactions, item => Assert.Equal(interactionA.Id, item.Id));
                Assert.Collection(opportunity.Visits, item => Assert.Equal(visitA.Id, item.Id));
            });
            Assert.Collection(responseB.Opportunities, opportunity =>
            {
                Assert.Equal(leadB.Id, opportunity.Id);
                Assert.Collection(opportunity.Interactions, item => Assert.Equal(interactionB.Id, item.Id));
                Assert.Collection(opportunity.Visits, item => Assert.Equal(visitB.Id, item.Id));
            });
        }
    }

    private (Contact Contact, Lead LeadA, Lead LeadB) SeedSharedContact(Guid advisorAId, Guid advisorBId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var email = Email.Create($"shared-{Guid.NewGuid():N}@test.com");
        var contact = Contact.Create("Contacto compartido", email, Telefono.Create("3101234567"));
        var leadA = Lead.Crear(contact.Name, email, Telefono.Create("3101234567"), "Test A", true,
            contactId: contact.Id);
        var leadB = Lead.Crear(contact.Name, email, Telefono.Create("3101234567"), "Test B", true,
            publicationId: Guid.NewGuid(), contactId: contact.Id);
        leadA.AsignarAsesor(advisorAId);
        leadB.AsignarAsesor(advisorBId);
        db.Contacts.Add(contact);
        db.Leads.AddRange(leadA, leadB);
        db.SaveChanges();
        return (contact, leadA, leadB);
    }

    private static CustomerDemand CreateDemand(Guid contactId, Guid leadId, string neighborhood) =>
        CustomerDemand.Create(contactId, leadId, ModalidadInmueble.Venta, TipoInmueble.Apartamento,
            "Medellin", neighborhood, null, null, null, null, null, null);

    private sealed record DemandDto(Guid Id, Guid ContactId, Guid? LeadId);
    private sealed record ContactDetailDto(ContactOpportunityDto[] Opportunities);
    private sealed record ContactOpportunityDto(Guid Id, ContactInteractionDto[] Interactions, ContactVisitDto[] Visits);
    private sealed record ContactInteractionDto(Guid Id);
    private sealed record ContactVisitDto(Guid Id);
}
