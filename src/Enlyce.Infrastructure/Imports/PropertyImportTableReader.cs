using System.Globalization;
using System.Text;
using ExcelDataReader;
using Enlyce.Application.Imports;
using NPOI.SS.UserModel;

namespace Enlyce.Infrastructure.Imports;

public sealed class PropertyImportTableReader : IPropertyImportTableReader
{
    private const int MaximumColumns = 100;
    private const int MaximumRows = 2_000;

    static PropertyImportTableReader() =>
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public Task<PropertyImportTable> ReadAsync(Stream file, string fileName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => Task.FromResult(ReadCsv(file, ct)),
            ".xls" or ".xlsx" => Task.FromResult(ReadWorkbook(file, ct)),
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

        if (!reader.Read())
            throw new InvalidDataException("El archivo no contiene encabezados.");
        if (reader.FieldCount is < 1 or > MaximumColumns)
            throw new InvalidDataException($"El archivo debe tener entre 1 y {MaximumColumns} columnas.");

        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(index => CellText(reader.GetValue(index), reader.GetNumberFormatString(index)) ?? string.Empty)
            .ToArray();
        RejectCsvFormulas(columns);
        ValidateHeaders(columns);
        var rows = new List<IReadOnlyList<string?>>();
        while (reader.Read())
        {
            ct.ThrowIfCancellationRequested();
            var row = Enumerable.Range(0, reader.FieldCount)
                .Select(index => CellText(reader.GetValue(index), reader.GetNumberFormatString(index)))
                .ToArray();
            RejectCsvFormulas(row);
            if (row.All(string.IsNullOrWhiteSpace))
                continue;
            AddRow(rows, row);
        }

        return new PropertyImportTable(columns, rows);
    }

    private static PropertyImportTable ReadWorkbook(Stream file, CancellationToken ct)
    {
        using var workbook = WorkbookFactory.Create(file);
        if (workbook.NumberOfSheets == 0)
            throw new InvalidDataException("El archivo no contiene hojas.");

        var sheet = workbook.GetSheetAt(0);
        if (sheet.PhysicalNumberOfRows == 0)
            throw new InvalidDataException("El archivo no contiene encabezados.");

        RejectFormulas(workbook, ct);
        var header = sheet.GetRow(sheet.FirstRowNum)
            ?? throw new InvalidDataException("El archivo no contiene encabezados.");
        var columnCount = header.LastCellNum;
        if (columnCount is < 1 or > MaximumColumns)
            throw new InvalidDataException($"El archivo debe tener entre 1 y {MaximumColumns} columnas.");

        var formatter = new DataFormatter(CultureInfo.InvariantCulture);
        var columns = Enumerable.Range(0, columnCount)
            .Select(index => CellText(header.GetCell(index), formatter) ?? string.Empty)
            .ToArray();
        ValidateHeaders(columns);

        var rows = new List<IReadOnlyList<string?>>();
        for (var rowIndex = header.RowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            ct.ThrowIfCancellationRequested();
            var sourceRow = sheet.GetRow(rowIndex);
            var row = Enumerable.Range(0, columnCount)
                .Select(index => CellText(sourceRow?.GetCell(index), formatter))
                .ToArray();
            if (row.All(string.IsNullOrWhiteSpace))
                continue;
            AddRow(rows, row);
        }

        return new PropertyImportTable(columns, rows);
    }

    private static void RejectFormulas(IWorkbook workbook, CancellationToken ct)
    {
        for (var sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
        {
            var sheet = workbook.GetSheetAt(sheetIndex);
            for (var rowIndex = sheet.FirstRowNum; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                ct.ThrowIfCancellationRequested();
                var row = sheet.GetRow(rowIndex);
                if (row is null) continue;
                for (var cellIndex = row.FirstCellNum; cellIndex >= 0 && cellIndex < row.LastCellNum; cellIndex++)
                    if (row.GetCell(cellIndex)?.CellType == CellType.Formula)
                        throw new InvalidDataException("El archivo contiene fórmulas. Convierte sus resultados a valores y vuelve a cargarlo.");
            }
        }
    }

    private static void ValidateHeaders(IReadOnlyList<string?> columns)
    {
        if (columns.All(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("La primera fila debe contener los encabezados.");
    }

    private static void RejectCsvFormulas(IEnumerable<string?> cells)
    {
        if (cells.Any(value => value?.TrimStart().StartsWith('=') == true))
            throw new InvalidDataException("El archivo contiene celdas que parecen fórmulas. Convierte sus resultados a valores y vuelve a cargarlo.");
    }

    private static void AddRow(List<IReadOnlyList<string?>> rows, IReadOnlyList<string?> row)
    {
        if (rows.Count >= MaximumRows)
            throw new InvalidDataException($"El archivo supera el máximo de {MaximumRows} inmuebles.");
        rows.Add(row);
    }

    private static string? CellText(ICell? cell, DataFormatter formatter)
    {
        if (cell is null || cell.CellType is CellType.Blank)
            return null;
        var text = formatter.FormatCellValue(cell).Trim();
        return text.Length == 0 ? null : text;
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
