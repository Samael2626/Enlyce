using System.Globalization;
using System.IO.Compression;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Xml;
using Enlyce.Api.Endpoints;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.CsvExport;

public static class CsvExportModule
{
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public static void MapCsvExport(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inmuebles/exportar-correo", async Task<IResult> (
            HttpContext http,
            IInmuebleRepository properties,
            IEmailAttachmentSender email,
            CancellationToken ct) =>
        {
            var recipient = http.User.FindFirst(ClaimTypes.Email)?.Value;
            if (!MailAddress.TryCreate(recipient, out var recipientAddress))
                return Results.BadRequest(new { error = "La cuenta no tiene correo asociado." });

            var rows = await properties.GetAllAsync();
            if (rows.Count == 0)
                return Results.BadRequest(new { error = "No hay inmuebles para exportar." });
            if (rows.Count > 50_000)
                return Results.Problem("La exportacion supera el limite de 50.000 inmuebles.",
                    statusCode: StatusCodes.Status413PayloadTooLarge);

            var content = BuildPropertiesXlsx(rows);
            if (content.Length > 20_000_000)
                return Results.Problem("El archivo supera el limite de 20 MB para envio por correo.",
                    statusCode: StatusCodes.Status413PayloadTooLarge);

            var fileName = $"inmuebles-enlyce-{DateTime.UtcNow:yyyyMMdd}.xlsx";
            var sent = await email.SendAttachmentAsync(recipientAddress.Address, "Inventario de inmuebles Enlyce",
                $"Archivo Excel con {rows.Count} inmuebles activos visibles para tu cuenta.", fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", content, ct);
            return sent
                ? Results.Ok(new { sent = true, count = rows.Count })
                : Results.Problem("No se pudo enviar el archivo. Revisa la configuracion del correo.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .RequireAuthorization()
        .RequireRateLimiting("client-export-email")
        .WithName("EmailPropertiesXlsxExport");

        app.MapGet("/api/contactos/export.csv", async Task<IResult> (
            string? q,
            DateOnly? from,
            DateOnly? to,
            string? customerType,
            string? customerStatus,
            string? customerFunnel,
            Guid? advisorId,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var export = await BuildContactsCsvAsync(q, from, to, customerType, customerStatus, customerFunnel,
                advisorId, http, contacts, leads, ct);
            return export.Error ?? Results.File(export.Content!, "text/csv; charset=utf-8", "clientes-enlyce.csv");
        })
        .RequireAuthorization()
        .WithName("ExportContactsCsv")
        .Produces(StatusCodes.Status200OK, contentType: "text/csv");

        app.MapPost("/api/contactos/exportar-correo", async Task<IResult> (
            string? q,
            DateOnly? from,
            DateOnly? to,
            string? customerType,
            string? customerStatus,
            string? customerFunnel,
            Guid? advisorId,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            IEmailAttachmentSender email,
            CancellationToken ct) =>
        {
            var recipient = http.User.FindFirst(ClaimTypes.Email)?.Value;
            if (!MailAddress.TryCreate(recipient, out var recipientAddress))
                return Results.BadRequest(new { error = "La cuenta no tiene correo asociado." });

            var export = await BuildContactsCsvAsync(q, from, to, customerType, customerStatus, customerFunnel,
                advisorId, http, contacts, leads, ct);
            if (export.Error is not null)
                return export.Error;
            if (export.Count == 0)
                return Results.BadRequest(new { error = "No hay clientes para exportar." });
            if (export.Content!.Length > 20_000_000)
                return Results.Problem("El archivo supera el limite de 20 MB para envio por correo.",
                    statusCode: StatusCodes.Status413PayloadTooLarge);

            var fileName = $"clientes-enlyce-{DateTime.UtcNow:yyyyMMdd}.csv";
            var sent = await email.SendAttachmentAsync(recipientAddress.Address, "Exportacion de clientes Enlyce",
                $"Archivo CSV con {export.Count} clientes visibles para tu cuenta.", fileName,
                "text/csv; charset=utf-8", export.Content, ct);
            return sent
                ? Results.Ok(new { sent = true, count = export.Count })
                : Results.Problem("No se pudo enviar el archivo. Revisa la configuracion del correo.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .RequireAuthorization()
        .RequireRateLimiting("client-export-email")
        .WithName("EmailContactsCsvExport");

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

    private static byte[] BuildPropertiesXlsx(IReadOnlyCollection<Inmueble> properties)
    {
        var rows = new List<object?[]>
        {
            new object?[] { "id", "nombre", "descripcion", "tipo", "modalidad", "estado", "direccion", "barrio", "ciudad",
                "departamento", "pais", "precio", "moneda", "metros_cuadrados", "habitaciones", "banos",
                "parqueaderos", "creado", "referencia_externa", "caracteristicas_internas", "caracteristicas_externas" }
        };
        rows.AddRange(properties.Select(property => new object?[]
        {
            property.Id.ToString(), property.Nombre, property.Descripcion, property.Tipo.ToString(),
            property.Modalidad.ToString(), property.Estado.ToString(), property.Direccion.Calle,
            property.Direccion.Barrio ?? string.Empty, property.Direccion.Ciudad,
            property.Direccion.Departamento ?? string.Empty, property.Direccion.Pais,
            property.Precio.Monto, property.Precio.Moneda, property.MetrosCuadrados, property.Habitaciones,
            property.Banos, property.Parqueaderos, property.FechaCreacion.ToString("O", CultureInfo.InvariantCulture),
            property.ExternalRef ?? string.Empty, string.Join(", ", property.InternalFeatures),
            string.Join(", ", property.ExternalFeatures)
        }));

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteZipEntry(archive, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>
                """);
            WriteZipEntry(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>
                """);
            WriteZipEntry(archive, "xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Inmuebles" sheetId="1" r:id="rId1"/></sheets></workbook>
                """);
            WriteZipEntry(archive, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/></Relationships>
                """);

            using var sheetStream = new MemoryStream();
            using (var writer = XmlWriter.Create(sheetStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false }))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("worksheet", SpreadsheetNamespace);
                writer.WriteStartElement("sheetData", SpreadsheetNamespace);
                for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    writer.WriteStartElement("row", SpreadsheetNamespace);
                    writer.WriteAttributeString("r", (rowIndex + 1).ToString(CultureInfo.InvariantCulture));
                    for (var columnIndex = 0; columnIndex < rows[rowIndex].Length; columnIndex++)
                    {
                        var value = rows[rowIndex][columnIndex];
                        if (value is null) continue;
                        writer.WriteStartElement("c", SpreadsheetNamespace);
                        writer.WriteAttributeString("r", $"{ColumnName(columnIndex + 1)}{rowIndex + 1}");
                        if (value is string text)
                        {
                            writer.WriteAttributeString("t", "inlineStr");
                            writer.WriteStartElement("is", SpreadsheetNamespace);
                            writer.WriteStartElement("t", SpreadsheetNamespace);
                            writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                            writer.WriteString(SanitizeExcelText(text));
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                        else
                        {
                            writer.WriteAttributeString("t", "n");
                            writer.WriteElementString("v", SpreadsheetNamespace, Convert.ToString(value, CultureInfo.InvariantCulture)!);
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            sheetStream.Position = 0;
            var sheetEntry = archive.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Fastest);
            using var entryStream = sheetEntry.Open();
            sheetStream.CopyTo(entryStream);
        }
        return stream.ToArray();
    }

    private static void WriteZipEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content.Trim());
    }

    private static string ColumnName(int column)
    {
        var name = string.Empty;
        while (column > 0)
        {
            column--;
            name = (char)('A' + column % 26) + name;
            column /= 26;
        }
        return name;
    }

    private static string SanitizeExcelText(string value)
    {
        var sanitized = new StringBuilder(Math.Min(value.Length, 32_767));
        foreach (var rune in value.EnumerateRunes())
        {
            var codePoint = rune.Value;
            if (codePoint is 0x9 or 0xA or 0xD || codePoint is >= 0x20 and <= 0xD7FF ||
                codePoint is >= 0xE000 and <= 0xFFFD || codePoint is >= 0x10000 and <= 0x10FFFF)
                sanitized.Append(rune.ToString());
            if (sanitized.Length >= 32_767)
                break;
        }
        return sanitized.ToString();
    }

    private static string EscapeFormula(string value)
    {
        var first = value.TrimStart(' ', '\t', '\r', '\n', '\u0000');
        return first.Length > 0 && first[0] is '=' or '+' or '-' or '@' ? $"'{value}" : value;
    }

    private static async Task<ContactCsvExport> BuildContactsCsvAsync(string? q, DateOnly? from, DateOnly? to,
        string? customerType, string? customerStatus, string? customerFunnel, Guid? advisorId,
        HttpContext http, IContactRepository contacts, ILeadRepository leads, CancellationToken ct)
    {
        if (from.HasValue && to.HasValue && from > to)
            return new(null, 0, Results.BadRequest(new { error = "El rango de fechas no es valido." }));
        if (advisorId.HasValue && !http.User.IsInRole("Administrador"))
            return new(null, 0, Results.Forbid());
        if (!string.IsNullOrWhiteSpace(customerType) && customerType.Trim().Length > 30)
            return new(null, 0, Results.BadRequest(new { error = "El tipo de cliente no puede superar 30 caracteres." }));
        if (!string.IsNullOrWhiteSpace(customerStatus) &&
            customerStatus is not ("Nuevo" or "EnProceso" or "Convertido" or "Recuperado" or "Perdido"))
            return new(null, 0, Results.BadRequest(new { error = "El estado del cliente no es valido." }));
        if (!string.IsNullOrWhiteSpace(customerFunnel) && customerFunnel.Trim().Length > 100)
            return new(null, 0, Results.BadRequest(new { error = "El embudo no puede superar 100 caracteres." }));

        IReadOnlyCollection<Guid>? allowedIds = null;
        if (!http.User.IsInRole("Administrador"))
        {
            var currentAdvisorId = EndpointAccess.AdvisorId(http.User);
            if (!currentAdvisorId.HasValue)
                return new(null, 0, Results.Forbid());
            allowedIds = await GetAdvisorContactIdsAsync(currentAdvisorId.Value, contacts, leads);
        }
        if (advisorId.HasValue)
        {
            var advisorContactIds = await GetAdvisorContactIdsAsync(advisorId.Value, contacts, leads);
            allowedIds = allowedIds is null ? advisorContactIds : advisorContactIds.Intersect(allowedIds).ToArray();
        }

        var allContacts = new List<Contact>();
        const int batchSize = 1000;
        const int maxRows = 50000;
        var skip = 0;
        int total;
        do
        {
            var result = await contacts.SearchAsync(q, from?.ToDateTime(TimeOnly.MinValue),
                to?.ToDateTime(TimeOnly.MaxValue), allowedIds, skip, batchSize, ct,
                customerType, customerStatus, customerFunnel?.Trim());
            total = result.Total;
            if (total > maxRows)
                return new(null, total, Results.Problem("La exportacion supera el limite de 50.000 clientes.",
                    statusCode: StatusCodes.Status413PayloadTooLarge));
            allContacts.AddRange(result.Items);
            if (result.Items.Count == 0)
                break;
            skip += result.Items.Count;
        } while (skip < total);

        var csv = new StringBuilder();
        AppendRow(csv, ["nombre", "apellidos", "correo", "telefono", "tipo_cliente", "etiquetas", "estado", "embudo", "creado"]);
        foreach (var contact in allContacts)
            AppendRow(csv, [contact.Name, contact.Surname ?? string.Empty, contact.Email ?? string.Empty,
                contact.Phone?.Value ?? string.Empty, string.Join(", ", contact.CustomerTypes), string.Join(", ", contact.CustomerLabels),
                contact.CustomerStatus, contact.CustomerFunnel,
                contact.CreatedAt.ToString("O", CultureInfo.InvariantCulture)]);
        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return new(content, allContacts.Count, null);
    }

    private sealed record ContactCsvExport(byte[]? Content, int Count, IResult? Error);

    private static async Task<IReadOnlyCollection<Guid>> GetAdvisorContactIdsAsync(
        Guid advisorId, IContactRepository contacts, ILeadRepository leads)
    {
        var contactIds = (await contacts.GetIdsByOwnerAdvisorAsync(advisorId)).ToHashSet();
        var assignedLeads = await leads.GetByAsesorIdAsync(advisorId);
        contactIds.UnionWith(assignedLeads.Where(lead => lead.ContactId.HasValue).Select(lead => lead.ContactId!.Value));
        return contactIds;
    }
}
