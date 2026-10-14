namespace Enlyce.Application.Imports;

public sealed record ClientImportTable(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows);

public interface IClientImportTableReader
{
    Task<ClientImportTable> ReadAsync(Stream file, string fileName, CancellationToken ct = default);
}

public sealed record ClientImportPreviewRequest(
    IReadOnlyDictionary<string, string?> Columns);

public sealed record ClientImportPreviewResponse(
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    int DuplicateRows,
    IReadOnlyList<string> SourceColumns,
    IReadOnlyList<ClientImportPreviewRow> Rows);

public sealed record ClientImportPreviewRow(
    int RowNumber,
    string? Name,
    string? Surname,
    string? Email,
    string? Phone,
    bool CanReceiveAutomatedEmail,
    string? Error,
    bool Duplicate);

public static class ClientImportFields
{
    public const string Name = "name";
    public const string Surname = "surname";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string CanReceiveAutomatedEmail = "canReceiveAutomatedEmail";
}
