using System.Net;
using System.Net.Http.Headers;
using System.IO.Compression;
using System.Xml.Linq;
using Enlyce.Application.Auth;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Enlyce.Infrastructure.Imports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    [Fact]
    public async Task EmailContacts_SendsFilteredXlsxWithTextCells()
    {
        var sender = new CapturingAttachmentSender();
        using var scopedFactory = new TestWebApplicationFactory().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailAttachmentSender>();
                services.AddSingleton<IEmailAttachmentSender>(sender);
            }));
        var client = scopedFactory.CreateClient();
        using (client)
        {
            var marker = Guid.NewGuid().ToString("N");
            using (var scope = scopedFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
                var user = Asesor.Crear("Export Admin", Email.Create($"export-{marker}@test.com"),
                    BCrypt.Net.BCrypt.HashPassword("OnlyForTests123!"), "Administrador");
                db.Asesores.Add(user);
                db.Contacts.Add(Contact.Create($"=SUM(1,1) {marker}", Email.Create($"selected-{marker}@test.com")));
                db.Contacts.Add(Contact.Create("Unrelated Contact", Email.Create($"other-{Guid.NewGuid():N}@test.com")));
                await db.SaveChangesAsync();
                var token = scope.ServiceProvider.GetRequiredService<ITokenService>().GenerarToken(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/contactos/exportar-correo?q={marker}", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sender.ContentType);
            Assert.EndsWith(".xlsx", sender.FileName);
            using var archive = new ZipArchive(new MemoryStream(sender.Content!), ZipArchiveMode.Read);
            var sheet = archive.GetEntry("xl/worksheets/sheet1.xml");
            Assert.NotNull(sheet);
            using var reader = new StreamReader(sheet.Open());
            var xml = XDocument.Parse(await reader.ReadToEndAsync());
            var spreadsheet = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            var rows = xml.Descendants(spreadsheet + "row").ToArray();
            Assert.Equal(2, rows.Length);
            Assert.Equal("nombre", rows[0].Descendants(spreadsheet + "t").First().Value);
            Assert.Equal($"=SUM(1,1) {marker}", rows[1].Descendants(spreadsheet + "t").First().Value);
            Assert.Empty(xml.Descendants(spreadsheet + "f"));

            using var workbook = new MemoryStream(sender.Content!);
            var imported = await new ClientImportTableReader().ReadAsync(workbook, "clientes.xlsx");
            Assert.Equal(22, imported.Columns.Count);
            Assert.Single(imported.Rows);
            Assert.Equal($"=SUM(1,1) {marker}", imported.Rows[0][0]);
        }
    }

    private sealed class CapturingAttachmentSender : IEmailAttachmentSender
    {
        public string? FileName { get; private set; }
        public string? ContentType { get; private set; }
        public byte[]? Content { get; private set; }

        public Task<bool> SendAttachmentAsync(string to, string subject, string body, string fileName,
            string contentType, byte[] content, CancellationToken ct = default)
        {
            FileName = fileName;
            ContentType = contentType;
            Content = content;
            return Task.FromResult(true);
        }
    }
}
