using Enlyce.Domain.Errors;
using System.Globalization;
using System.Text;

namespace Enlyce.Domain.Entities;

public sealed class CustomerDemand
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public Guid? LeadId { get; private set; }
    public ModalidadInmueble Operation { get; private set; }
    public TipoInmueble? PropertyType { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = "Colombia";
    public string? Department { get; private set; }
    public string? Neighborhood { get; private set; }
    public decimal? MinimumPrice { get; private set; }
    public decimal? MaximumPrice { get; private set; }
    public string Currency { get; private set; } = "COP";
    public decimal? MinimumArea { get; private set; }
    public decimal? MaximumArea { get; private set; }
    public int? Bedrooms { get; private set; }
    public int? Bathrooms { get; private set; }
    public int? ParkingSpaces { get; private set; }
    public string[] InternalFeatures { get; private set; } = [];
    public string[] ExternalFeatures { get; private set; } = [];
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    private CustomerDemand() { }

    private CustomerDemand(Guid id, Guid contactId, Guid? leadId, ModalidadInmueble operation,
        TipoInmueble? propertyType, string city, string country, string? department,
        string? neighborhood, decimal? minimumPrice,
        decimal? maximumPrice, string currency, decimal? minimumArea, decimal? maximumArea,
        int? bedrooms, int? bathrooms, int? parkingSpaces, string[]? internalFeatures,
        string[]? externalFeatures,
        string? notes, DateTime createdAt, DateTime updatedAt, bool active)
    {
        Id = id;
        ContactId = contactId;
        LeadId = leadId;
        Operation = operation;
        PropertyType = propertyType;
        City = city;
        Country = country;
        Department = department;
        Neighborhood = neighborhood;
        MinimumPrice = minimumPrice;
        MaximumPrice = maximumPrice;
        Currency = currency;
        MinimumArea = minimumArea;
        MaximumArea = maximumArea;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        ParkingSpaces = parkingSpaces;
        InternalFeatures = internalFeatures ?? [];
        ExternalFeatures = externalFeatures ?? [];
        Notes = notes;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Active = active;
    }

    public static CustomerDemand Create(Guid contactId, Guid? leadId, ModalidadInmueble operation,
        TipoInmueble? propertyType, string? city, string? neighborhood, decimal? minimumPrice,
        decimal? maximumPrice, int? bedrooms, int? bathrooms, int? parkingSpaces, string? notes,
        string currency = "COP", decimal? minimumArea = null, decimal? maximumArea = null,
        string country = "Colombia", string? department = null,
        string[]? internalFeatures = null, string[]? externalFeatures = null)
    {
        if (contactId == Guid.Empty || leadId == Guid.Empty)
            throw new DomainError("El contacto o la oportunidad no son validos.");
        if (operation is not (ModalidadInmueble.Venta or ModalidadInmueble.Arriendo))
            throw new DomainError("La operacion debe ser Venta o Arriendo.");
        if (propertyType.HasValue && !Enum.IsDefined(propertyType.Value))
            throw new DomainError("El tipo de inmueble no es valido.");
        // Ciudad vacia = cualquier municipio: las alertas de la web no siempre la traen.
        if (city?.Trim().Length > 100)
            throw new DomainError("La ciudad no puede superar 100 caracteres.");
        if (string.IsNullOrWhiteSpace(country) || country.Trim().Length > 100 || department?.Trim().Length > 100)
            throw new DomainError("El pais o departamento no es valido.");
        if (minimumPrice is < 0 || maximumPrice is < 0 ||
            minimumPrice.HasValue && maximumPrice.HasValue && minimumPrice > maximumPrice)
            throw new DomainError("El rango de presupuesto no es valido.");
        if (minimumArea is < 0 || maximumArea is < 0 ||
            minimumArea.HasValue && maximumArea.HasValue && minimumArea > maximumArea)
            throw new DomainError("El rango de area no es valido.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3 ||
            currency.Trim().Any(character => !char.IsAsciiLetter(character)))
            throw new DomainError("La moneda debe ser un codigo de tres letras.");
        if (bedrooms is < 0 || bathrooms is < 0 || parkingSpaces is < 0)
            throw new DomainError("Las caracteristicas del inmueble no pueden ser negativas.");

        var now = DateTime.UtcNow;
        return new CustomerDemand(Guid.NewGuid(), contactId, leadId, operation, propertyType,
            city?.Trim() ?? string.Empty, country.Trim(), Normalize(department, 100),
            Normalize(neighborhood, 100), minimumPrice, maximumPrice,
            currency.Trim().ToUpperInvariant(), minimumArea, maximumArea, bedrooms,
            bathrooms, parkingSpaces, NormalizeFeatures(internalFeatures), NormalizeFeatures(externalFeatures),
            Normalize(notes, 2_000), now, now, true);
    }

    public static CustomerDemand Reconstitute(Guid id, Guid contactId, Guid? leadId,
        ModalidadInmueble operation, TipoInmueble? propertyType, string city,
        string? neighborhood, decimal? minimumPrice, decimal? maximumPrice, int? bedrooms,
        int? bathrooms, int? parkingSpaces, string? notes, DateTime createdAt,
        DateTime updatedAt, bool active, string currency = "COP", decimal? minimumArea = null,
        decimal? maximumArea = null, string country = "Colombia", string? department = null,
        string[]? internalFeatures = null, string[]? externalFeatures = null) =>
        new(id, contactId, leadId, operation, propertyType, city, country, department, neighborhood,
            minimumPrice, maximumPrice, currency, minimumArea, maximumArea, bedrooms, bathrooms, parkingSpaces,
            internalFeatures, externalFeatures, notes,
            createdAt, updatedAt, active);

    public bool Matches(Inmueble property)
    {
        if (!Active || !property.Activo || property.Estado != EstadoInmueble.Disponible)
            return false;
        if (property.Modalidad != Operation && property.Modalidad != ModalidadInmueble.VentaYArriendo)
            return false;
        if (PropertyType.HasValue && PropertyType != property.Tipo)
            return false;
        if (City.Length > 0 && !string.Equals(City, property.Direccion.Ciudad, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.Equals(Country, property.Direccion.Pais, StringComparison.OrdinalIgnoreCase))
            return false;
        if (Department is not null && !string.Equals(Department, property.Direccion.Departamento, StringComparison.OrdinalIgnoreCase))
            return false;
        if (Neighborhood is not null && !string.Equals(Neighborhood, property.Direccion.Barrio, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!ContainsAll(property.InternalFeatures, InternalFeatures) ||
            !ContainsAll(property.ExternalFeatures, ExternalFeatures))
            return false;
        if (MinimumPrice.HasValue && property.Precio.Monto < MinimumPrice.Value)
            return false;
        if (MaximumPrice.HasValue && property.Precio.Monto > MaximumPrice.Value)
            return false;
        if (!string.Equals(Currency, property.Precio.Moneda, StringComparison.OrdinalIgnoreCase))
            return false;
        if (MinimumArea.HasValue && property.MetrosCuadrados < MinimumArea.Value)
            return false;
        if (MaximumArea.HasValue && property.MetrosCuadrados > MaximumArea.Value)
            return false;
        return (!Bedrooms.HasValue || property.Habitaciones >= Bedrooms.Value) &&
               (!Bathrooms.HasValue || property.Banos >= Bathrooms.Value) &&
               (!ParkingSpaces.HasValue || property.Parqueaderos >= ParkingSpaces.Value);
    }

    public void Update(ModalidadInmueble operation, TipoInmueble? propertyType, string? city,
        string? neighborhood, decimal? minimumPrice, decimal? maximumPrice, int? bedrooms,
        int? bathrooms, int? parkingSpaces, string? notes, string currency = "COP",
        decimal? minimumArea = null, decimal? maximumArea = null, string country = "Colombia",
        string? department = null, string[]? internalFeatures = null,
        string[]? externalFeatures = null)
    {
        var validated = Create(ContactId, LeadId, operation, propertyType, city, neighborhood,
            minimumPrice, maximumPrice, bedrooms, bathrooms, parkingSpaces, notes, currency,
            minimumArea, maximumArea, country, department, internalFeatures, externalFeatures);
        Operation = validated.Operation;
        PropertyType = validated.PropertyType;
        City = validated.City;
        Country = validated.Country;
        Department = validated.Department;
        Neighborhood = validated.Neighborhood;
        MinimumPrice = validated.MinimumPrice;
        MaximumPrice = validated.MaximumPrice;
        Currency = validated.Currency;
        MinimumArea = validated.MinimumArea;
        MaximumArea = validated.MaximumArea;
        Bedrooms = validated.Bedrooms;
        Bathrooms = validated.Bathrooms;
        ParkingSpaces = validated.ParkingSpaces;
        InternalFeatures = validated.InternalFeatures;
        ExternalFeatures = validated.ExternalFeatures;
        Notes = validated.Notes;
        UpdatedAt = DateTime.UtcNow;
    }

    // Baja logica: la alerta deja de enviar avisos pero queda auditada.
    public void Deactivate()
    {
        if (!Active)
            return;
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new DomainError($"El texto no puede superar {maxLength} caracteres.");
        return normalized;
    }

    private static string[] NormalizeFeatures(string[]? values)
    {
        var normalized = (values ?? []).Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (normalized.Length > 30 || normalized.Any(value => value.Length > 50))
            throw new DomainError("Se permiten hasta 30 caracteristicas de 50 caracteres cada una.");
        return normalized;
    }

    private static bool ContainsAll(string[] available, string[] requested)
    {
        var availableKeys = available.Select(FeatureKey).ToHashSet(StringComparer.Ordinal);
        return requested.All(feature => availableKeys.Contains(FeatureKey(feature)));
    }

    private static string FeatureKey(string value)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(decomposed
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        return string.Join(' ', withoutDiacritics.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }
}

public enum DemandPropertyStatus { Suggested, Shared, Visited, Discarded }

public sealed class DemandPropertyLink
{
    public Guid DemandId { get; private set; }
    public Guid PropertyId { get; private set; }
    public DemandPropertyStatus Status { get; private set; }
    public DateTime LinkedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DemandPropertyLink() { }

    public static DemandPropertyLink Create(Guid demandId, Guid propertyId) =>
        demandId == Guid.Empty || propertyId == Guid.Empty
            ? throw new DomainError("La demanda y el inmueble son obligatorios.")
            : new DemandPropertyLink { DemandId = demandId, PropertyId = propertyId,
                Status = DemandPropertyStatus.Suggested, LinkedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

    public void SetStatus(DemandPropertyStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new DomainError("El estado del inmueble relacionado no es valido.");
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
