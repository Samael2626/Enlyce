using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class ContactsEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task AdvisorCanReadContactWithAssignedOpportunities()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var (contact, _) = SeedContact(advisor.Id);
            var response = await client.GetAsync($"/api/contactos/{contact.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task AdvisorCannotReadContactAssignedToAnotherAdvisor()
    {
        var contact = SeedContact(Guid.NewGuid()).Contact;
        var (client, _) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.GetAsync($"/api/contactos/{contact.Id}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task SharedContactOnlyShowsOpportunitiesAndTasksAssignedToCurrentAdvisor()
    {
        var (advisorAClient, advisorA) = factory.CreateAuthenticatedClient("Asesor");
        var (advisorBClient, advisorB) = factory.CreateAuthenticatedClient("Asesor");
        using (advisorAClient)
        using (advisorBClient)
        {
            var (contact, leadA) = SeedContact(advisorA.Id);
            var leadB = Lead.Crear(contact.Name, Email.Create(contact.Email), Telefono.Create("3101234567"),
                "Test compartido", true, publicationId: Guid.NewGuid(), contactId: contact.Id);
            leadB.AsignarAsesor(advisorB.Id);
            var taskA = CommercialTask.Create(contact.Id, leadA.Id, advisorA.Id, CommercialTaskType.Call,
                "Tarea A", null, DateTime.UtcNow.AddHours(2), null, CommercialTaskPriority.Normal);
            var taskB = CommercialTask.Create(contact.Id, leadB.Id, advisorB.Id, CommercialTaskType.Call,
                "Tarea B", null, DateTime.UtcNow.AddHours(2), null, CommercialTaskPriority.Normal);
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.Leads.Add(leadB);
                db.CommercialTasks.AddRange(taskA, taskB);
                db.CommercialTaskEvents.AddRange(
                    CommercialTaskEvent.Create(taskA.Id, advisorA.Id, "Created"),
                    CommercialTaskEvent.Create(taskB.Id, advisorB.Id, "Created"));
                db.SaveChanges();
            }

            var response = await advisorAClient.GetFromJsonAsync<ContactDetailDto>($"/api/contactos/{contact.Id}");

            Assert.NotNull(response);
            Assert.Collection(response.Opportunities, opportunity => Assert.Equal(leadA.Id, opportunity.Id));
            Assert.Collection(response.Tasks, task => Assert.Equal(taskA.Id, task.Id));
        }
    }

    [Fact]
    public async Task AdvisorCannotEditContactAssignedOnlyToAnotherAdvisor()
    {
        var contact = SeedContact(Guid.NewGuid()).Contact;
        var (client, _) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var response = await client.PutAsJsonAsync($"/api/contactos/{contact.Id}", new
            {
                name = "Nombre alterado",
                phone = "3001234567"
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            using var scope = factory.Services.CreateScope();
            var saved = await scope.ServiceProvider.GetRequiredService<EnlyceDbContext>()
                .Contacts.SingleAsync(item => item.Id == contact.Id);
            Assert.Equal("Contacto de prueba", saved.Name);
        }
    }

    [Fact]
    public async Task AdvisorContactListOnlyContainsAssignedContacts()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var owned = SeedContact(advisor.Id).Contact;
            var unassigned = SeedContact(Guid.NewGuid()).Contact;

            var response = await client.GetAsync("/api/contactos?pageSize=100");
            response.EnsureSuccessStatusCode();
            var contacts = await response.Content.ReadFromJsonAsync<ContactListDto>();

            Assert.NotNull(contacts);
            Assert.Contains(contacts.Items, contact => contact.Id == owned.Id);
            Assert.DoesNotContain(contacts.Items, contact => contact.Id == unassigned.Id);
            Assert.True(contacts.Total >= contacts.Items.Length);
        }
    }

    [Fact]
    public async Task AdministratorCanSearchContactsByNameEmailOrPhoneWithinInclusiveDatesAndPageStably()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var createdAt = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var alpha = Contact.Reconstitute(Guid.NewGuid(), "Match Alpha", "match-alpha@example.com",
            Telefono.Create("3105550199"), createdAt, createdAt, true);
        var beta = Contact.Reconstitute(Guid.NewGuid(), "Match Beta", "match-beta@example.com",
            null, createdAt, createdAt, true);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.Contacts.AddRange(alpha, beta);
            db.SaveChanges();
        }

        var (client, _) = factory.CreateAuthenticatedClient();
        using (client)
        {
            var range = $"from={date:yyyy-MM-dd}&to={date:yyyy-MM-dd}";
            var page = await client.GetFromJsonAsync<ContactListDto>(
                $"/api/contactos?q=match&{range}&page=2&pageSize=1");
            Assert.NotNull(page);
            Assert.Equal(2, page.Total);
            Assert.Equal("Match Beta", Assert.Single(page.Items).Name);

            var emailMatch = await client.GetFromJsonAsync<ContactListDto>(
                $"/api/contactos?q=match-alpha%40example.com&{range}");
            Assert.NotNull(emailMatch);
            Assert.Equal(alpha.Id, Assert.Single(emailMatch.Items).Id);

            var phoneMatch = await client.GetFromJsonAsync<ContactListDto>(
                $"/api/contactos?q=3105550199&{range}");
            Assert.NotNull(phoneMatch);
            Assert.Equal(alpha.Id, Assert.Single(phoneMatch.Items).Id);
        }
    }

    [Fact]
    public async Task AdvisorCanCreateAndCompleteTaskForAssignedContact()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var (contact, lead) = SeedContact(advisor.Id);
            var dueAt = DateTime.UtcNow.AddHours(2);
            var created = await client.PostAsJsonAsync("/api/tareas", new
            {
                contactId = contact.Id,
                leadId = lead.Id,
                type = "Call",
                title = "Llamar para confirmar",
                dueAt,
                priority = "High"
            });

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var task = await created.Content.ReadFromJsonAsync<TaskDto>();
            Assert.NotNull(task);
            Assert.Equal(contact.Id, task.ContactId);
            Assert.Equal("Pending", task.Status);

            var completed = await client.PutAsync($"/api/tareas/{task.Id}/completar", null);
            Assert.Equal(HttpStatusCode.OK, completed.StatusCode);
        }
    }

    [Fact]
    public async Task FirstAdvisorInteractionRecordsFirstResponseWithoutAssignmentFakingIt()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var (_, lead) = SeedContact(advisor.Id);
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                Assert.Equal(EstadoLead.Nuevo, (await db.Leads.SingleAsync(item => item.Id == lead.Id)).Estado);
                Assert.Null((await db.Leads.SingleAsync(item => item.Id == lead.Id)).FechaPrimerContacto);
            }

            var response = await client.PostAsJsonAsync("/api/interacciones", new
            {
                leadId = lead.Id,
                asesorId = advisor.Id,
                tipo = "Llamada",
                resumen = "Primera llamada"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var verifyScope = factory.Services.CreateScope();
            var saved = await verifyScope.ServiceProvider.GetRequiredService<EnlyceDbContext>()
                .Leads.SingleAsync(item => item.Id == lead.Id);
            Assert.Equal(EstadoLead.Contactado, saved.Estado);
            Assert.NotNull(saved.FechaPrimerContacto);
            Assert.NotNull(saved.FechaUltimoContacto);
        }
    }

    [Fact]
    public async Task OtherAdvisorCannotCompleteTask()
    {
        var (ownerClient, owner) = factory.CreateAuthenticatedClient("Asesor");
        ownerClient.Dispose();
        var (contact, lead) = SeedContact(owner.Id);
        var task = CommercialTask.Create(contact.Id, lead.Id, owner.Id, CommercialTaskType.Call,
            "Llamar", null, DateTime.UtcNow.AddHours(2), null, CommercialTaskPriority.Normal);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.CommercialTasks.Add(task);
            db.CommercialTaskEvents.Add(CommercialTaskEvent.Create(task.Id, owner.Id, "Created"));
            db.SaveChanges();
        }

        var (otherClient, _) = factory.CreateAuthenticatedClient("Asesor");
        using (otherClient)
        {
            var response = await otherClient.PutAsync($"/api/tareas/{task.Id}/completar", null);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task AssignedAdvisorCanCompleteVisitAndSaveFeedback()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var (_, lead) = SeedContact(advisor.Id);
            var visit = Visita.Programar(lead.Id, Guid.NewGuid(), advisor.Id, DateTime.UtcNow.AddDays(1));
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.Visitas.Add(visit);
                db.SaveChanges();
            }

            var response = await client.PutAsJsonAsync($"/api/visitas/{visit.Id}/realizada", new { feedback = "Interés confirmado" });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var scopeAfter = factory.Services.CreateScope();
            var saved = await scopeAfter.ServiceProvider.GetRequiredService<EnlyceDbContext>()
                .Visitas.SingleAsync(item => item.Id == visit.Id);
            Assert.Equal("Realizada", saved.Estado);
            Assert.Equal("Interés confirmado", saved.Feedback);
        }
    }

    [Fact]
    public async Task AdvisorCanCreateDemandFindMatchesAndLinkProperty()
    {
        var (client, advisor) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var (contact, lead) = SeedContact(advisor.Id);
            var property = Inmueble.Crear("Apartamento Laureles", "", TipoInmueble.Apartamento,
                ModalidadInmueble.Venta, Direccion.Crear("Calle 1", "Medellín", "Laureles"),
                Dinero.Crear(400_000_000), 80, 2, 2, 1, TestWebApplicationFactory.PropietarioSeedId);
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.Inmuebles.Add(property);
                db.SaveChanges();
            }

            var created = await client.PostAsJsonAsync("/api/demandas", new
            {
                contactId = contact.Id,
                leadId = lead.Id,
                operation = "Venta",
                propertyType = "Apartamento",
                city = "Medellín",
                neighborhood = "Laureles",
                minimumPrice = 300_000_000,
                maximumPrice = 500_000_000,
                bedrooms = 2,
                bathrooms = 2,
                parkingSpaces = 1
            });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var demand = await created.Content.ReadFromJsonAsync<DemandDto>();
            Assert.NotNull(demand);

            var matches = await client.GetFromJsonAsync<MatchDto[]>($"/api/demandas/{demand.Id}/coincidencias");
            Assert.NotNull(matches);
            Assert.Contains(matches, item => item.PropertyId == property.Id);

            var linked = await client.PostAsync($"/api/demandas/{demand.Id}/inmuebles/{property.Id}", null);
            Assert.Equal(HttpStatusCode.Created, linked.StatusCode);
        }
    }

    private (Contact Contact, Lead Lead) SeedContact(Guid advisorId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
        var email = Email.Create($"contact-{Guid.NewGuid():N}@test.com");
        var contact = Contact.Create("Contacto de prueba", email, Telefono.Create("3101234567"));
        var lead = Lead.Crear(contact.Name, email, Telefono.Create("3101234567"), "Test", true,
            contactId: contact.Id);
        lead.AsignarAsesor(advisorId);
        db.Contacts.Add(contact);
        db.Leads.Add(lead);
        db.SaveChanges();
        return (contact, lead);
    }

    private sealed record ContactDto(Guid Id, string Name, string Email, string? Phone, DateTime CreatedAt, DateTime UpdatedAt);
    private sealed record ContactListDto(int Total, ContactDto[] Items);
    private sealed record ContactDetailDto(ContactDto Contact, OpportunityDto[] Opportunities, TaskDetailDto[] Tasks);
    private sealed record OpportunityDto(Guid Id);
    private sealed record TaskDetailDto(Guid Id);
    private sealed record TaskDto(Guid Id, Guid ContactId, string Status);
    private sealed record DemandDto(Guid Id);
    private sealed record MatchDto(Guid PropertyId);
}
