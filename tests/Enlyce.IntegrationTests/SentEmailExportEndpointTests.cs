using System.IO.Compression;
using System.Net;
using System.Xml.Linq;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class SentEmailExportEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private const string Path = "/api/analytics/sent-emails.xlsx?from=2040-05-10&to=2040-05-11";

    [Fact]
    public async Task ExportRequiresAdministratorAndValidDateRange()
    {
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Path)).StatusCode);

        var (advisor, _) = factory.CreateAuthenticatedClient("Asesor");
        using (advisor)
            Assert.Equal(HttpStatusCode.Forbidden, (await advisor.GetAsync(Path)).StatusCode);

        var (administrator, _) = factory.CreateAuthenticatedClient();
        using (administrator)
        {
            Assert.Equal(HttpStatusCode.BadRequest, (await administrator.GetAsync(
                "/api/analytics/sent-emails.xlsx?from=2040-05-12&to=2040-05-11")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await administrator.GetAsync(
                "/api/analytics/sent-emails.xlsx?from=2040-01-01&to=2041-01-01")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await administrator.GetAsync(
                "/api/analytics/sent-emails.xlsx?from=9999-12-31&to=9999-12-31")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await administrator.GetAsync(
                "/api/analytics/sent-emails.xlsx?to=0001-01-01")).StatusCode);
        }
    }

    [Fact]
    public async Task ExportIncludesOnlySelectedDatesAndRecordedStatus()
    {
        var marker = Guid.NewGuid().ToString("N");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.EmailDeliveryLogs.AddRange(
                EmailDeliveryLog.Create($"inside-{marker}@test.com", $"Inside {marker}",
                    "Invitacion", EmailDeliveryStatus.AcceptedBySmtp, null,
                    new DateTime(2040, 5, 11, 23, 59, 59, DateTimeKind.Utc)),
                EmailDeliveryLog.Create($"start-{marker}@test.com", $"Start {marker}",
                    "Aviso", EmailDeliveryStatus.Failed, "SMTP",
                    new DateTime(2040, 5, 10, 0, 0, 0, DateTimeKind.Utc)),
                EmailDeliveryLog.Create($"outside-{marker}@test.com", $"Outside {marker}",
                    "Recordatorio", EmailDeliveryStatus.Failed, "SMTP",
                    new DateTime(2040, 5, 12, 0, 0, 0, DateTimeKind.Utc)));
            await db.SaveChangesAsync();
        }

        var (administrator, _) = factory.CreateAuthenticatedClient();
        using (administrator)
        using (var response = await administrator.GetAsync(Path))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("correos-enviados-20400510-20400511.xlsx",
                response.Content.Headers.ContentDisposition?.FileName);

            using var archive = new ZipArchive(await response.Content.ReadAsStreamAsync());
            using var sheet = archive.GetEntry("xl/worksheets/sheet1.xml")!.Open();
            var document = XDocument.Load(sheet);
            var cells = document.Descendants()
                .Where(element => element.Name.LocalName == "t")
                .Select(element => element.Value).ToArray();

            Assert.Contains("Fecha", cells);
            Assert.Contains("Usuario encargado", cells);
            Assert.Contains($"Inside {marker}", cells);
            Assert.Contains($"inside-{marker}@test.com", cells);
            Assert.Contains($"Start {marker}", cells);
            Assert.Contains("Falló el envío", cells);
            Assert.Contains("SMTP", cells);
            Assert.Contains("Aceptado por SMTP", cells);
            Assert.DoesNotContain($"Outside {marker}", cells);
            Assert.DoesNotContain($"outside-{marker}@test.com", cells);
        }
    }
}
