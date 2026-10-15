using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class CustomerCatalogItem
{
    public static readonly IReadOnlyDictionary<string, int> CatalogLimits = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["CustomerType"] = 30,
        ["CustomerLabel"] = 50,
        ["CustomerFunnel"] = 100,
        ["AcquisitionMedium"] = 200
    };

    public Guid Id { get; private set; }
    public string Catalog { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private CustomerCatalogItem() { }

    public static CustomerCatalogItem Create(string catalog, string name)
    {
        if (catalog is null || !CatalogLimits.TryGetValue(catalog, out var maxLength))
            throw new DomainError("El tipo de catalogo no es valido.");

        var normalized = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maxLength)
            throw new DomainError($"El valor debe tener entre 1 y {maxLength} caracteres.");

        return new CustomerCatalogItem
        {
            Id = Guid.NewGuid(),
            Catalog = catalog,
            Name = normalized,
            NormalizedName = normalized.ToUpperInvariant(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
