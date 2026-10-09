using System.Net;
using System.Net.Http.Json;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class LeadHistoryEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task UnifiedHistoryCombinesAssignmentsAndStagesWithActorAndReasonAndRespectsScope()
    {
        var (ownerClient, owner) = factory.CreateAuthenticatedClient("Asesor");
        var (otherClient, _) = factory.CreateAuthenticatedClient("Asesor");
        using (ownerClient)
        using (otherClient)
        {
            var lead = Lead.Reconstituir(
                Guid.NewGuid(), "Audited Lead", Email.Create($"audit-{Guid.NewGuid():N}@test.com"), null,
                "Test", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, owner.Id,
                DateTime.UtcNow, null, DateTime.UtcNow, true, true, "Venta", EtapasPipeline.LeadNuevo);
            var assignment = LeadAssignmentHistory.Create(
                lead.Id, null, owner.Id, null, "Asignacion inicial", LeadAssignmentSource.Legacy,
                DateTime.UtcNow.AddMinutes(-1));
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.Leads.Add(lead);
                db.LeadAssignmentHistory.Add(assignment);
                db.SaveChanges();
            }

            var moved = await ownerClient.PutAsJsonAsync($"/api/pipeline/{lead.Id}/mover-etapa", new
            {
                nuevaEtapa = EtapasPipeline.Contactado,
                reason = "Primera llamada"
            });
            Assert.Equal(HttpStatusCode.OK, moved.StatusCode);

            var history = await ownerClient.GetFromJsonAsync<HistoryItem[]>(
                $"/api/pipeline/{lead.Id}/historial");
            Assert.NotNull(history);
            Assert.Equal(2, history.Length);
            Assert.Equal("Asignacion", history[0].Type);
            Assert.Equal("Etapa", history[1].Type);
            Assert.Equal(EtapasPipeline.LeadNuevo, history[1].From);
            Assert.Equal(EtapasPipeline.Contactado, history[1].To);
            Assert.Equal(owner.Nombre, history[1].Actor);
            Assert.Equal("Primera llamada", history[1].Reason);

            Assert.Equal(HttpStatusCode.Forbidden,
                (await otherClient.GetAsync($"/api/pipeline/{lead.Id}/historial")).StatusCode);
        }
    }

    private sealed record HistoryItem(Guid Id, string Type, string From, string To,
        string Actor, string Reason, DateTime OccurredAt);
}
