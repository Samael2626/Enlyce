using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using ExcelDataReader;
using Enlyce.Application.Imports;
using OpenMcdf;

namespace Enlyce.Infrastructure.Imports;

public sealed class PropertyImportTableReader : IPropertyImportTableReader
{
    private const int MaximumColumns = 100;
    private const int MaximumRows = 2_000;
    private const int MaximumFileBytes = 16 * 1024 * 1024;

    static PropertyImportTableReader() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public Task<PropertyImportTable> ReadAsync(Stream file, string fileName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => Task.FromResult(ReadCsv(file, ct)),
            ".xls" or ".xlsx" => Task.FromResult(ReadWorkbook(file, extension, ct)),
            _ => throw new InvalidDataException("El archivo debe ser CSV, XLS o XLSX.")
        };
    }

    private static PropertyImportTable ReadCsv(Stream file, CancellationToken ct)
    {
        using var reader = ExcelReaderFactory.CreateCsvReader(file, new ExcelReaderConfiguration
        {
            AutodetectSeparators = [',', ';', '\t', '|'],
            FallbackEncoding = Encoding.GetEncoding(1252),
            TrimWhiteSpace = true,
            LeaveOpen = true
        });
        return ReadRows(reader, rejectTextFormulas: true, ct);
    }

    private static PropertyImportTable ReadWorkbook(Stream file, string extension, CancellationToken ct)
    {
        using var content = new MemoryStream();
        var chunk = new byte[81920];
        int count;
        while ((count = file.Read(chunk)) != 0)
        {
            ct.ThrowIfCancellationRequested();
            if (content.Length + count > MaximumFileBytes)
                throw new InvalidDataException("El archivo supera el máximo de 16 MB.");
            content.Write(chunk, 0, count);
        }

        content.Position = 0;
        if (extension == ".xlsx") RejectOpenXmlFormulas(content, ct);
        else RejectBinaryFormulas(content, ct);
        content.Position = 0;
        using var reader = ExcelReaderFactory.CreateReader(content);
        return ReadRows(reader, rejectTextFormulas: false, ct);
    }

    private static PropertyImportTable ReadRows(IExcelDataReader reader, bool rejectTextFormulas, CancellationToken ct)
    {
        if (!reader.Read())
            throw new InvalidDataException("El archivo no contiene encabezados.");
        if (reader.FieldCount is < 1 or > MaximumColumns)
            throw new InvalidDataException($"El archivo debe tener entre 1 y {MaximumColumns} columnas.");

        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(index => CellText(reader.GetValue(index), reader.GetNumberFormatString(index)) ?? string.Empty)
            .ToArray();
        if (rejectTextFormulas) RejectCsvFormulas(columns);
        if (columns.All(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("La primera fila debe contener los encabezados.");

        var rows = new List<IReadOnlyList<string?>>();
        while (reader.Read())
        {
            ct.ThrowIfCancellationRequested();
            var row = Enumerable.Range(0, reader.FieldCount)
                .Select(index => CellText(reader.GetValue(index), reader.GetNumberFormatString(index)))
                .ToArray();
            if (rejectTextFormulas) RejectCsvFormulas(row);
            if (row.All(string.IsNullOrWhiteSpace)) continue;
            if (rows.Count >= MaximumRows)
                throw new InvalidDataException($"El archivo supera el máximo de {MaximumRows} inmuebles.");
            rows.Add(row);
        }

        return new PropertyImportTable(columns, rows);
    }

    private static void RejectOpenXmlFormulas(Stream content, CancellationToken ct)
    {
        using var archive = new ZipArchive(content, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in archive.Entries)
        {
            ct.ThrowIfCancellationRequested();
            if (!entry.FullName.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ||
                !entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                continue;
            using var stream = entry.Open();
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 32_000_000
            });
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "f")
                    throw new InvalidDataException("El archivo contiene fórmulas. Convierte sus resultados a valores y vuelve a cargarlo.");
            }
        }
    }

    private static void RejectBinaryFormulas(MemoryStream content, CancellationToken ct)
    {
        using var compoundContent = new MemoryStream(content.ToArray());
        using var storage = RootStorage.Open(compoundContent);
        if (!storage.TryOpenStream("Workbook", out var workbook) &&
            !storage.TryOpenStream("Book", out workbook))
            throw new InvalidDataException("El archivo XLS no contiene un libro de trabajo.");
        using (workbook)
        {
            using var reader = new BinaryReader(workbook, Encoding.UTF8, leaveOpen: true);
            while (workbook.Position + 4 <= workbook.Length)
            {
                ct.ThrowIfCancellationRequested();
                var recordType = reader.ReadUInt16();
                var recordLength = reader.ReadUInt16();
                if (recordLength > 8224 || workbook.Position + recordLength > workbook.Length)
                    throw new InvalidDataException("El archivo XLS contiene un registro dañado.");
                if (recordType is 0x0006 or 0x0221 or 0x0236)
                    throw new InvalidDataException("El archivo contiene fórmulas. Convierte sus resultados a valores y vuelve a cargarlo.");
                workbook.Position += recordLength;
            }
            if (workbook.Position != workbook.Length)
                throw new InvalidDataException("El archivo XLS contiene un registro incompleto.");
        }
    }

    private static void RejectCsvFormulas(IEnumerable<string?> cells)
    {
        if (cells.Any(value => value?.TrimStart().StartsWith('=') == true))
            throw new InvalidDataException("El archivo contiene celdas que parecen fórmulas. Convierte sus resultados a valores y vuelve a cargarlo.");
    }

    private static string? CellText(object? value, string? format) => value switch
    {
        null => null,
        DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        double number when format is { Length: > 0 } mask && mask.All(character => character == '0') &&
                           number >= 0 && number <= long.MaxValue && number == Math.Truncate(number) =>
            ((long)number).ToString(new string('0', Math.Min(mask.Length, 20)), CultureInfo.InvariantCulture),
        double number => number.ToString("G", CultureInfo.InvariantCulture),
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } text ? text : null
    };
}
