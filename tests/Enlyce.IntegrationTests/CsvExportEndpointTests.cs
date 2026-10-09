using System.Net;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enlyce.IntegrationTests;

public sealed class CsvExportEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Export_RequiresAuthentication()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/opportunities/export.csv?from=2026-01-01&to=2026-01-31");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Export_ReturnsCsvWithUtf8BomAndOperationalHeaders()
    {
        var (client, _) = factory.CreateAuthenticatedClient("Administrador");
        using (client)
        {
            var response = await client.GetAsync("/api/opportunities/export.csv?from=2026-01-01&to=2026-01-31");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.StartsWith("text/csv", response.Content.Headers.ContentType?.ToString());
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.True(bytes.Take(3).SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }));
            var csv = System.Text.Encoding.UTF8.GetString(bytes[3..]);
            Assert.StartsWith("\"id\",\"contacto_nombre\",\"contacto_email\",\"contacto_telefono\",\"operacion\",\"etapa\",\"origen\",\"creado\",\"primera_respuesta\",\"asesor\"\r\n", csv);
            Assert.DoesNotContain("consentimiento", csv, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ip", csv, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Export_RejectsInvertedDateRange()
    {
        var (client, _) = factory.CreateAuthenticatedClient("Administrador");
        using (client)
        {
            var response = await client.GetAsync("/api/opportunities/export.csv?from=2026-02-01&to=2026-01-31");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task Export_AsesorSeesOnlyAssignedLeadsAndNeutralizesFormulaValues()
    {
        var (client, advisorA) = factory.CreateAuthenticatedClient("Asesor");
        using (client)
        {
            var suffix = Guid.NewGuid().ToString("N");
            var advisorB = Asesor.Crear(
                "Asesor B",
                Email.Create($"csv-advisor-b-{suffix}@test.com"),
                BCrypt.Net.BCrypt.HashPassword("OnlyForTests123!"));
            var createdAt = DateTime.UtcNow;
            var leadA = Lead.Reconstituir(
                Guid.NewGuid(), $"=HYPERLINK(\"https://example.com/{suffix}\")",
                Email.Create($"csv-a-{suffix}@test.com"), Telefono.Create("3101234567"),
                "Web", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, advisorA.Id,
                createdAt, null, createdAt, true, true);
            var leadB = Lead.Reconstituir(
                Guid.NewGuid(), "Lead asesor B", Email.Create($"csv-b-{suffix}@test.com"), null,
                "Web", EstadoLead.Nuevo, MotivoCierre.Ninguno, null, advisorB.Id,
                createdAt, null, createdAt, true, true);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                db.Asesores.Add(advisorB);
                db.Leads.AddRange(leadA, leadB);
                await db.SaveChangesAsync();
            }

            var date = DateOnly.FromDateTime(createdAt).ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/api/opportunities/export.csv?from={date}&to={date}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var csv = System.Text.Encoding.UTF8.GetString(bytes[3..]);

            Assert.Contains($"csv-a-{suffix}@test.com", csv);
            Assert.DoesNotContain($"csv-b-{suffix}@test.com", csv);
            Assert.Contains($"\"'=HYPERLINK(\"\"https://example.com/{suffix}\"\")\"", csv);
        }
    }
}
