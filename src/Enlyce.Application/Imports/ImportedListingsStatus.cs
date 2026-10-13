namespace Enlyce.Application.Imports;

// Lo que realmente quedo en la base para un origen, sin depender del resultado de una corrida.
public sealed record ImportedListingsCounts(
    int Inmuebles,
    int ConPublicacion,
    int Publicados,
    int Borradores,
    int SinPublicacion,
    int Fotos);

public interface IImportedListingsReadRepository
{
    Task<ImportedListingsCounts> CountAsync(string source, CancellationToken ct = default);
}
