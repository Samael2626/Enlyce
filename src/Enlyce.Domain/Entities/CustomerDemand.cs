using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class CustomerDemand
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public Guid? LeadId { get; private set; }
    public ModalidadInmueble Operation { get; private set; }
    public TipoInmueble? PropertyType { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? Neighborhood { get; private set; }
    public decimal? MinimumPrice { get; private set; }
    public decimal? MaximumPrice { get; private set; }
    public int? Bedrooms { get; private set; }
    public int? Bathrooms { get; private set; }
    public int? ParkingSpaces { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    private CustomerDemand() { }

    private CustomerDemand(Guid id, Guid contactId, Guid? leadId, ModalidadInmueble operation,
        TipoInmueble? propertyType, string city, string? neighborhood, decimal? minimumPrice,
        decimal? maximumPrice, int? bedrooms, int? bathrooms, int? parkingSpaces,
        string? notes, DateTime createdAt, DateTime updatedAt, bool active)
    {
        Id = id;
        ContactId = contactId;
        LeadId = leadId;
        Operation = operation;
        PropertyType = propertyType;
        City = city;
        Neighborhood = neighborhood;
        MinimumPrice = minimumPrice;
        MaximumPrice = maximumPrice;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        ParkingSpaces = parkingSpaces;
        Notes = notes;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Active = active;
    }

    public static CustomerDemand Create(Guid contactId, Guid? leadId, ModalidadInmueble operation,
        TipoInmueble? propertyType, string city, string? neighborhood, decimal? minimumPrice,
        decimal? maximumPrice, int? bedrooms, int? bathrooms, int? parkingSpaces, string? notes)
    {
        if (contactId == Guid.Empty || leadId == Guid.Empty)
            throw new DomainError("El contacto o la oportunidad no son validos.");
        if (operation is not (ModalidadInmueble.Venta or ModalidadInmueble.Arriendo))
            throw new DomainError("La operacion debe ser Venta o Arriendo.");
        if (propertyType.HasValue && !Enum.IsDefined(propertyType.Value))
            throw new DomainError("El tipo de inmueble no es valido.");
        if (string.IsNullOrWhiteSpace(city) || city.Trim().Length > 100)
            throw new DomainError("La ciudad es obligatoria y no puede superar 100 caracteres.");
        if (minimumPrice is < 0 || maximumPrice is < 0 ||
            minimumPrice.HasValue && maximumPrice.HasValue && minimumPrice > maximumPrice)
            throw new DomainError("El rango de presupuesto no es valido.");
        if (bedrooms is < 0 || bathrooms is < 0 || parkingSpaces is < 0)
            throw new DomainError("Las caracteristicas del inmueble no pueden ser negativas.");

        var now = DateTime.UtcNow;
        return new CustomerDemand(Guid.NewGuid(), contactId, leadId, operation, propertyType,
            city.Trim(), Normalize(neighborhood, 100), minimumPrice, maximumPrice, bedrooms,
            bathrooms, parkingSpaces, Normalize(notes, 2_000), now, now, true);
    }

    public static CustomerDemand Reconstitute(Guid id, Guid contactId, Guid? leadId,
        ModalidadInmueble operation, TipoInmueble? propertyType, string city,
        string? neighborhood, decimal? minimumPrice, decimal? maximumPrice, int? bedrooms,
        int? bathrooms, int? parkingSpaces, string? notes, DateTime createdAt,
        DateTime updatedAt, bool active) =>
        new(id, contactId, leadId, operation, propertyType, city, neighborhood,
            minimumPrice, maximumPrice, bedrooms, bathrooms, parkingSpaces, notes,
            createdAt, updatedAt, active);

    public bool Matches(Inmueble property)
    {
        if (!Active || !property.Activo || property.Estado != EstadoInmueble.Disponible)
            return false;
        if (property.Modalidad != Operation && property.Modalidad != ModalidadInmueble.VentaYArriendo)
            return false;
        if (PropertyType.HasValue && PropertyType != property.Tipo)
            return false;
        if (!string.Equals(City, property.Direccion.Ciudad, StringComparison.OrdinalIgnoreCase))
            return false;
        if (Neighborhood is not null && !string.Equals(Neighborhood, property.Direccion.Barrio, StringComparison.OrdinalIgnoreCase))
            return false;
        if (MinimumPrice.HasValue && property.Precio.Monto < MinimumPrice.Value)
            return false;
        if (MaximumPrice.HasValue && property.Precio.Monto > MaximumPrice.Value)
            return false;
        return (!Bedrooms.HasValue || property.Habitaciones >= Bedrooms.Value) &&
               (!Bathrooms.HasValue || property.Banos >= Bathrooms.Value) &&
               (!ParkingSpaces.HasValue || property.Parqueaderos >= ParkingSpaces.Value);
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
