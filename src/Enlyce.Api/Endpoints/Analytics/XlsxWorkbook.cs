using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace Enlyce.Api.Endpoints.Analytics;

internal static class XlsxWorkbook
{
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public static byte[] Build(string sheetName, IReadOnlyList<string> headers, IEnumerable<object?[]> dataRows)
        => Build([new WorkbookSheet(sheetName, headers, dataRows.ToArray())]);

    public static byte[] Build(IReadOnlyList<WorkbookSheet> sheets)
    {
        if (sheets.Count == 0)
            throw new ArgumentException("El libro debe tener al menos una hoja.", nameof(sheets));

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var contentTypes = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
            var workbookSheets = new StringBuilder();
            var relationships = new StringBuilder();
            for (var index = 0; index < sheets.Count; index++)
            {
                var id = index + 1;
                contentTypes.Append($"<Override PartName=\"/xl/worksheets/sheet{id}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
                workbookSheets.Append($"<sheet name=\"{XmlEscape(sheets[index].Name)}\" sheetId=\"{id}\" r:id=\"rId{id}\"/>");
                relationships.Append($"<Relationship Id=\"rId{id}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{id}.xml\"/>");
            }
            contentTypes.Append("</Types>");
            WriteEntry(archive, "[Content_Types].xml", contentTypes.ToString());
            WriteEntry(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>
                """);
            WriteEntry(archive, "xl/workbook.xml", $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"{SpreadsheetNamespace}\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>{workbookSheets}</sheets></workbook>");
            WriteEntry(archive, "xl/_rels/workbook.xml.rels", $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">{relationships}</Relationships>");

            for (var index = 0; index < sheets.Count; index++)
            {
                var sheet = sheets[index];
                var rows = new List<object?[]> { sheet.Headers.Cast<object?>().ToArray() };
                rows.AddRange(sheet.Rows);
                using var sheetStream = new MemoryStream();
                using (var writer = XmlWriter.Create(sheetStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
                    WriteWorksheet(writer, rows);
                sheetStream.Position = 0;
                var sheetEntry = archive.CreateEntry($"xl/worksheets/sheet{index + 1}.xml", CompressionLevel.Fastest);
                using var entryStream = sheetEntry.Open();
                sheetStream.CopyTo(entryStream);
            }
        }
        return stream.ToArray();
    }

    private static void WriteWorksheet(XmlWriter writer, IReadOnlyList<object?[]> rows)
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
                    writer.WriteString(SanitizeText(text));
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

    internal sealed record WorkbookSheet(string Name, IReadOnlyList<string> Headers, IReadOnlyList<object?[]> Rows);

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        using var writer = new StreamWriter(archive.CreateEntry(path, CompressionLevel.Fastest).Open(), new UTF8Encoding(false));
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

    private static string SanitizeText(string value)
    {
        var sanitized = new StringBuilder(Math.Min(value.Length, 32_767));
        foreach (var rune in value.EnumerateRunes())
        {
            if (rune.Value is 0x9 or 0xA or 0xD || rune.Value is >= 0x20 and <= 0xD7FF ||
                rune.Value is >= 0xE000 and <= 0xFFFD || rune.Value is >= 0x10000 and <= 0x10FFFF)
                sanitized.Append(rune.ToString());
            if (sanitized.Length >= 32_767) break;
        }
        return sanitized.ToString();
    }

    private static string XmlEscape(string value) => value
        .Replace("&", "&amp;", StringComparison.Ordinal)
        .Replace("\"", "&quot;", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal);
}
