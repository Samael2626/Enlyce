using System.Globalization;
using System.Text;
using Enlyce.Api.Endpoints;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.CsvExport;

public static class CsvExportModule
{
    public static void MapCsvExport(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/opportunities/export.csv", async Task<IResult> (
            DateOnly from,
            DateOnly to,
            HttpContext http,
            ILeadRepository leads,
            IAsesorRepository advisors,
            CancellationToken ct) =>
        {
            if (from > to)
                return Results.BadRequest(new { message = "El rango de fechas no es válido." });

            var advisorId = EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            var rows = http.User.IsInRole("Administrador")
                ? await leads.GetAllAsync()
                : await leads.GetByAsesorIdAsync(advisorId!.Value);
            var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endExclusive = to == DateOnly.MaxValue
                ? DateTime.MaxValue
                : to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            var advisorNames = new Dictionary<Guid, string>();
            var assignedIds = rows.Where(row => row.AsesorAsignadoId.HasValue)
                .Select(row => row.AsesorAsignadoId!.Value).Distinct();
            foreach (var id in assignedIds)
            {
                var advisor = await advisors.ObtenerPorIdAsync(id);
                if (advisor is not null) advisorNames[id] = advisor.Nombre;
            }

            var csv = new StringBuilder();
            AppendRow(csv, ["id", "contacto_nombre", "contacto_email", "contacto_telefono", "operacion", "etapa", "origen", "creado", "primera_respuesta", "asesor"]);
            foreach (var lead in rows.Where(row => row.FechaCreacion >= start && row.FechaCreacion < endExclusive))
            {
                var advisor = lead.AsesorAsignadoId is Guid id && advisorNames.TryGetValue(id, out var name)
                    ? name
                    : string.Empty;
                AppendRow(csv, [
                    lead.Id.ToString(), lead.Nombre, lead.Email.Value, lead.Telefono?.Value ?? string.Empty,
                    lead.TipoOperacion, lead.EtapaPipeline, lead.Fuente,
                    lead.FechaCreacion.ToString("O", CultureInfo.InvariantCulture),
                    lead.FechaPrimerContacto?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                    advisor]);
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return Results.File(bytes, "text/csv; charset=utf-8", $"oportunidades-{from:yyyyMMdd}-{to:yyyyMMdd}.csv");
        })
        .RequireAuthorization()
        .WithName("ExportOpportunitiesCsv")
        .Produces(StatusCodes.Status200OK, contentType: "text/csv")
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static void AppendRow(StringBuilder csv, IEnumerable<string> values)
    {
        csv.AppendJoin(',', values.Select(value => $"\"{EscapeFormula(value).Replace("\"", "\"\"")}\""));
        csv.Append("\r\n");
    }

    private static string EscapeFormula(string value)
    {
        var first = value.TrimStart(' ', '\t', '\r', '\n', '\u0000');
        return first.Length > 0 && first[0] is '=' or '+' or '-' or '@' ? $"'{value}" : value;
    }
}
