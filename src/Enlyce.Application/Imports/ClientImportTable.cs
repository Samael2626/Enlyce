namespace Enlyce.Application.Imports;

public sealed record ClientImportTable(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows,
    IReadOnlySet<(int Row, int Column)>? UnsafeNumericIdentityCells = null,
    IReadOnlyList<int>? SourceRowNumbers = null);

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
    IReadOnlyList<ClientImportPreviewRow> Rows,
    int Page,
    int PageSize);

public sealed record ClientImportPreviewRow(
    int RowNumber,
    string? Name,
    string? Surname,
    string? Email,
    string? Phone,
    bool CanReceiveAutomatedEmail,
    IReadOnlyDictionary<string, string?> MappedValues,
    string? Error,
    bool Duplicate)
{
    public bool SourceIndicatesEmailPermission { get; init; }
}

public static class ClientImportFields
{
    public const string Name = "name";
    public const string Surname = "surname";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string CanReceiveAutomatedEmail = "canReceiveAutomatedEmail";
    public const string CustomerType = "customerType";
    public const string CustomerStatus = "customerStatus";
    public const string Address = "address";
    public const string Country = "country";
    public const string Department = "department";
    public const string City = "city";
    public const string Landline = "landline";
    public const string DateOfBirth = "dateOfBirth";
    public const string IdentificationNumber = "identificationNumber";
    public const string ReferredBy = "referredBy";
    public const string AcquisitionMedium = "acquisitionMedium";
    public const string AdditionalData = "additionalData";
    public const string CustomerLabels = "customerLabels";
    public const string CustomerFunnel = "customerFunnel";
}

public static class ClientImportPhoneSafety
{
    public static bool IsExactNumericPhone(string value) =>
        value.Length is >= 7 and <= 15 && value[0] != '0' && value.All(char.IsDigit);
}
