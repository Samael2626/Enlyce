using System.Globalization;
using System.Text;
using ExcelDataReader;
using Enlyce.Application.Imports;

namespace Enlyce.Infrastructure.Imports;

public sealed class ClientImportTableReader : IClientImportTableReader
{
    private const int MaximumColumns = 100;
    private const int MaximumRows = 10_000;

    static ClientImportTableReader() =>
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public Task<ClientImportTable> ReadAsync(Stream file, string fileName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not (".csv" or ".xls" or ".xlsx"))
            throw new InvalidDataException("El archivo debe ser CSV, XLS o XLSX.");

        using var reader = extension == ".csv"
            ? ExcelReaderFactory.CreateCsvReader(file, CreateCsvConfiguration())
            : ExcelReaderFactory.CreateReader(file);

        if (!reader.Read())
            throw new InvalidDataException("El archivo no contiene encabezados.");

        if (reader.FieldCount is < 1 or > MaximumColumns)
            throw new InvalidDataException($"El archivo debe tener entre 1 y {MaximumColumns} columnas.");

        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(index => CellText(reader.GetValue(index)) ?? string.Empty)
            .ToArray();
        if (columns.All(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("La primera fila debe contener los encabezados.");

        var rows = new List<IReadOnlyList<string?>>();
        while (reader.Read())
        {
            ct.ThrowIfCancellationRequested();
            var row = Enumerable.Range(0, reader.FieldCount)
                .Select(index => CellText(reader.GetValue(index)))
                .ToArray();
            if (row.All(string.IsNullOrWhiteSpace))
                continue;
            if (rows.Count == MaximumRows)
                throw new InvalidDataException($"El archivo supera el máximo de {MaximumRows} filas de datos.");
            rows.Add(row);
        }

        return Task.FromResult(new ClientImportTable(columns, rows));
    }

    private static ExcelReaderConfiguration CreateCsvConfiguration() => new()
    {
        AutodetectSeparators = [',', ';', '\t', '|'],
        FallbackEncoding = Encoding.GetEncoding(1252),
        TrimWhiteSpace = true,
        LeaveOpen = true
    };

    private static string? CellText(object? value) => value switch
    {
        null => null,
        DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        double number => number.ToString("G", CultureInfo.InvariantCulture),
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } text ? text : null
    };
}
