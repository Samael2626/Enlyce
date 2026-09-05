namespace Enlyce.Application.PublicCatalog;

public sealed class PublicCatalogValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public PublicCatalogValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Los parámetros de búsqueda no son válidos.")
    {
        Errors = errors;
    }
}
