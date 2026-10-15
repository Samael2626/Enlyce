namespace Enlyce.Application.Imports;

public sealed record PropertyImportTable(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows);

public interface IPropertyImportTableReader
{
    Task<PropertyImportTable> ReadAsync(Stream file, string fileName, CancellationToken ct = default);
}
