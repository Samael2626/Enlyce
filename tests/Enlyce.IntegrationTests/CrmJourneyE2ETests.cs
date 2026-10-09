using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class CrmJourneyE2ETests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task ContactToWonOpportunityJourneyKeepsEveryRecordLinked()
    {
        var (advisorClient, advisor) = factory.CreateAuthenticatedClient("Asesor");
        var (adminClient, _) = factory.CreateAuthenticatedClient("Administrador");
        using (advisorClient)
        using (adminClient)
        using (var publicClient = factory.CreateClient())
        {
            var email = $"journey-{Guid.NewGuid():N}@test.com";
            var submitted = await publicClient.PostAsJsonAsync("/api/leads", new
            {
                nombre = "CRM Journey",
                email,
                telefono = "3101234567",
                fuente = "Integration test",
                autorizacionDatos = true,
                tipoOperacion = "Venta"
            });
            Assert.Equal(HttpStatusCode.Accepted, submitted.StatusCode);

            Guid contactId;
            Guid leadId;
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                var lead = await db.Leads.SingleAsync(item => item.Email.Value == email);
                Assert.NotNull(lead.ContactId);
                contactId = lead.ContactId.Value;
                leadId = lead.Id;
            }

            var assigned = await adminClient.PutAsJsonAsync($"/api/pipeline/{leadId}/asignar", new
            {
                asesorId = advisor.Id,
                reason = "Asignacion para recorrido E2E"
            });
            Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);

            var interaction = await advisorClient.PostAsJsonAsync("/api/interacciones", new
            {
                leadId,
                asesorId = advisor.Id,
                tipo = "Llamada",
                resumen = "Califica necesidad de compra"
            });
            Assert.Equal(HttpStatusCode.OK, interaction.StatusCode);

            var taskResponse = await advisorClient.PostAsJsonAsync("/api/tareas", new
            {
                contactId,
                leadId,
                type = "Call",
                title = "Confirmar presupuesto",
                dueAt = DateTime.UtcNow.AddDays(1),
                priority = "High"
            });
            Assert.Equal(HttpStatusCode.Created, taskResponse.StatusCode);
            var task = await taskResponse.Content.ReadFromJsonAsync<TaskResponse>();
            Assert.NotNull(task);

            foreach (var stage in new[]
                     {
                         EtapasPipeline.Contactado,
                         EtapasPipeline.Cualificado,
                         EtapasPipeline.VisitaAgendada
                     })
            {
                var moved = await advisorClient.PutAsJsonAsync($"/api/pipeline/{leadId}/mover-etapa",
                    new { nuevaEtapa = stage });
                Assert.Equal(HttpStatusCode.OK, moved.StatusCode);
            }

            var visitResponse = await advisorClient.PostAsJsonAsync("/api/visitas", new
            {
                leadId,
                inmuebleId = Guid.NewGuid(),
                asesorId = advisor.Id,
                fechaProgramada = DateTime.UtcNow.AddDays(2)
            });
            Assert.Equal(HttpStatusCode.OK, visitResponse.StatusCode);
            var visitList = await advisorClient.GetFromJsonAsync<VisitResponse[]>($"/api/visitas/lead/{leadId}");
            Assert.NotNull(visitList);
            var visit = Assert.Single(visitList);

            var visitCompleted = await advisorClient.PutAsJsonAsync($"/api/visitas/{visit.Id}/realizada",
                new { feedback = "Cliente confirma interes" });
            Assert.Equal(HttpStatusCode.OK, visitCompleted.StatusCode);

            var demandResponse = await advisorClient.PostAsJsonAsync("/api/demandas", new
            {
                contactId,
                leadId,
                operation = "Venta",
                propertyType = "Apartamento",
                city = "Medellin",
                neighborhood = "Laureles",
                minimumPrice = 300000000,
                maximumPrice = 500000000,
                bedrooms = 2,
                bathrooms = 2,
                parkingSpaces = 1
            });
            Assert.Equal(HttpStatusCode.Created, demandResponse.StatusCode);
            var demand = await demandResponse.Content.ReadFromJsonAsync<DemandResponse>();
            Assert.NotNull(demand);
            Assert.Equal(contactId, demand.ContactId);
            Assert.Equal(leadId, demand.LeadId);

            foreach (var stage in new[]
                     {
                         EtapasPipeline.VisitaRealizada,
                         EtapasPipeline.OfertaNegociacion,
                         EtapasPipeline.BajoContrato,
                         EtapasPipeline.CerradoGanado
                     })
            {
                var moved = await advisorClient.PutAsJsonAsync($"/api/pipeline/{leadId}/mover-etapa",
                    new { nuevaEtapa = stage });
                Assert.Equal(HttpStatusCode.OK, moved.StatusCode);
            }

            using var verifyScope = factory.Services.CreateScope();
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            var savedLead = await verifyDb.Leads.SingleAsync(item => item.Id == leadId);
            var savedTask = await verifyDb.CommercialTasks.SingleAsync(item => item.Id == task.Id);
            var savedVisit = await verifyDb.Visitas.SingleAsync(item => item.Id == visit.Id);
            var savedDemand = await verifyDb.CustomerDemands.SingleAsync(item => item.Id == demand.Id);

            Assert.Equal(advisor.Id, savedLead.AsesorAsignadoId);
            Assert.Equal(EstadoLead.CerradoGanado, savedLead.Estado);
            Assert.Equal(EtapasPipeline.CerradoGanado, savedLead.EtapaPipeline);
            Assert.Equal(contactId, savedTask.ContactId);
            Assert.Equal(leadId, savedTask.LeadId);
            Assert.Equal(advisor.Id, savedTask.AdvisorId);
            Assert.Equal(leadId, savedVisit.LeadId);
            Assert.Equal(advisor.Id, savedVisit.AsesorId);
            Assert.Equal("Realizada", savedVisit.Estado);
            Assert.Equal(contactId, savedDemand.ContactId);
            Assert.Equal(leadId, savedDemand.LeadId);
        }
    }

    private sealed record TaskResponse(Guid Id, Guid ContactId, Guid? LeadId, Guid AdvisorId, string Status);
    private sealed record VisitResponse(Guid Id, Guid LeadId, Guid InmuebleId, Guid AsesorId);
    private sealed record DemandResponse(Guid Id, Guid ContactId, Guid? LeadId);
}
