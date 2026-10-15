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
            .Select(index => CellText(reader.GetValue(index), reader.GetNumberFormatString(index)) ?? string.Empty)
            .ToArray();
        if (columns.All(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("La primera fila debe contener los encabezados.");

        var rows = new List<IReadOnlyList<string?>>();
        var sourceRowNumbers = new List<int>();
        var unsafeNumericIdentityCells = new HashSet<(int Row, int Column)>();
        var sourceRowNumber = 1;
        while (reader.Read())
        {
            ct.ThrowIfCancellationRequested();
            sourceRowNumber++;
            var rowIndex = rows.Count;
            for (var columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++)
                if (IsUnsafeNumericIdentifier(reader.GetValue(columnIndex), reader.GetNumberFormatString(columnIndex)))
                    unsafeNumericIdentityCells.Add((rowIndex, columnIndex));
            var row = Enumerable.Range(0, reader.FieldCount)
                .Select(index => CellText(reader.GetValue(index), reader.GetNumberFormatString(index)))
                .ToArray();
            if (row.All(string.IsNullOrWhiteSpace))
                continue;
            if (rows.Count == MaximumRows)
                throw new InvalidDataException($"El archivo supera el máximo de {MaximumRows} filas de datos.");
            rows.Add(row);
            sourceRowNumbers.Add(sourceRowNumber);
        }

        return Task.FromResult(new ClientImportTable(columns, rows, unsafeNumericIdentityCells, sourceRowNumbers));
    }

    private static ExcelReaderConfiguration CreateCsvConfiguration() => new()
    {
        AutodetectSeparators = [',', ';', '\t', '|'],
        FallbackEncoding = Encoding.GetEncoding(1252),
        TrimWhiteSpace = true,
        LeaveOpen = true
    };

    private static string? CellText(object? value, string? numberFormat) => value switch
    {
        null => null,
        DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        double number when numberFormat is { Length: > 0 } format && format.All(character => character == '0') &&
                           number >= 0 && number <= long.MaxValue && number == Math.Truncate(number) =>
            ((long)number).ToString(new string('0', Math.Min(format.Length, 20)), CultureInfo.InvariantCulture),
        double number => number.ToString("G", CultureInfo.InvariantCulture),
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } text ? text : null
    };

    private static bool IsUnsafeNumericIdentifier(object? value, string? numberFormat) => value switch
    {
        double number => numberFormat is not { Length: > 0 } format || format.Any(character => character != '0') ||
                         Math.Abs(number) >= 1_000_000_000_000_000d && number == Math.Truncate(number),
        decimal number => numberFormat is not { Length: > 0 } format || format.Any(character => character != '0') ||
                          Math.Abs(number) >= 1_000_000_000_000_000m && number == decimal.Truncate(number),
        _ => false
    };
}
