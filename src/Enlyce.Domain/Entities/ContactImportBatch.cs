namespace Enlyce.Domain.Entities;

public sealed class ContactImportBatch
{
    public Guid RequestId { get; private set; }
    public string ActorId { get; private set; } = string.Empty;
    public string PayloadHash { get; private set; } = string.Empty;
    public int TotalRows { get; private set; }
    public int ImportedRows { get; private set; }
    public int InvalidRows { get; private set; }
    public int DuplicateRows { get; private set; }
    public string ErrorsJson { get; private set; } = "[]";
    public DateTime CreatedAt { get; private set; }

    private ContactImportBatch() { }

    public static ContactImportBatch Create(Guid requestId, string actorId, string payloadHash,
        int totalRows, int importedRows, int invalidRows, int duplicateRows, string errorsJson)
    {
        if (requestId == Guid.Empty || string.IsNullOrWhiteSpace(actorId) ||
            payloadHash.Length != 64 || payloadHash.Any(character => !Uri.IsHexDigit(character)))
            throw new ArgumentException("La clave de importacion no es valida.");
        if (totalRows < 0 || importedRows < 0 || invalidRows < 0 || duplicateRows < 0 ||
            importedRows + invalidRows + duplicateRows > totalRows)
            throw new ArgumentException("Los conteos de importacion no son validos.");

        return new ContactImportBatch
        {
            RequestId = requestId,
            ActorId = actorId.Trim(),
            PayloadHash = payloadHash.ToLowerInvariant(),
            TotalRows = totalRows,
            ImportedRows = importedRows,
            InvalidRows = invalidRows,
            DuplicateRows = duplicateRows,
            ErrorsJson = errorsJson,
            CreatedAt = DateTime.UtcNow
        };
    }
}
