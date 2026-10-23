using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Xml.Linq;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.IntegrationTests;

public sealed class OwnerReportExportEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private const string ExportPath = "/api/analytics/owners.xlsx?from=2041-06-01&to=2041-06-30";

    [Fact]
    public async Task ExportRequiresAdministratorAndValidRange()
    {
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(ExportPath)).StatusCode);

        var (advisor, _) = factory.CreateAuthenticatedClient("Asesor");
        using (advisor)
            Assert.Equal(HttpStatusCode.Forbidden, (await advisor.GetAsync(ExportPath)).StatusCode);

        var (administrator, _) = factory.CreateAuthenticatedClient();
        using (administrator)
        {
            Assert.Equal(HttpStatusCode.BadRequest, (await administrator.GetAsync(
                "/api/analytics/owners.xlsx?from=2041-07-01&to=2041-06-30")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await administrator.GetAsync(
                "/api/analytics/owners.xlsx?from=2041-01-01&to=2042-01-02")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await administrator.GetAsync(
                "/api/analytics/owners.xlsx?from=9999-12-31&to=9999-12-31")).StatusCode);
        }
    }

    [Fact]
    public async Task ExportContainsSameOwnerAndMetricsAsVisibleReport()
    {
        var name = $"Propietario {Guid.NewGuid():N}";
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            db.Propietarios.Add(Propietario.Crear(name, Email.Create($"{Guid.NewGuid():N}@test.com")));
            await db.SaveChangesAsync();
        }

        var (administrator, _) = factory.CreateAuthenticatedClient();
        using (administrator)
        {
            var visible = await administrator.GetFromJsonAsync<OwnerReportPayload>(
                "/api/analytics/owners?from=2041-06-01&to=2041-06-30");
            var owner = Assert.Single(visible!.Owners, item => item.OwnerName == name);

            using var response = await administrator.GetAsync(ExportPath);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("propietarios-20410601-20410630.xlsx",
                response.Content.Headers.ContentDisposition?.FileName);

            using var archive = new ZipArchive(await response.Content.ReadAsStreamAsync());
            using var sheet = archive.GetEntry("xl/worksheets/sheet1.xml")!.Open();
            var document = XDocument.Load(sheet);
            var rows = document.Descendants().Where(element => element.Name.LocalName == "row");
            var cells = rows.Select(row => row.Elements().Select(cell => cell.Value).ToArray()).ToArray();
            Assert.Equal("Propietario", cells[0][0]);
            var ownerRow = Assert.Single(cells, row => row[0] == name);
            Assert.Equal(new[]
            {
                name, owner.PageVisits.ToString(), owner.Calls.ToString(), owner.Emails.ToString(),
                owner.Events.ToString(), owner.Visits.ToString(), owner.ReceivedContacts.ToString()
            }, ownerRow);
        }
    }

    private sealed record OwnerReportPayload(IReadOnlyList<OwnerRow> Owners);
    private sealed record OwnerRow(string OwnerName, int PageVisits, int Calls, int Emails,
        int Events, int Visits, int ReceivedContacts);
}
