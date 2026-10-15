using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Imports;

public enum ExternalAvailability
{
    Disponible,
    Vendido,
    Arrendado,
    Desconocido
}

public sealed record ExternalPhoto(string Url, int Position, string Description);

// Contenido descargado de una foto externa; el Stream lo consume una sola vez IMediaStorage.
public sealed record ExternalPhotoContent(string FileName, string ContentType, long SizeBytes, Stream Content);

// Inmueble tal como lo entrega un portal externo, ya traducido a tipos del dominio.
// Tipo es null cuando el origen usa un tipo sin equivalente; el importador lo reporta.
public sealed record ExternalListing(
    string Source,
    string SourceId,
    string Title,
    string Description,
    TipoInmueble? Tipo,
    string SourceTypeLabel,
    decimal? SalePrice,
    decimal? RentPrice,
    string Currency,
    string Street,
    string City,
    string? Neighborhood,
    decimal? Latitude,
    decimal? Longitude,
    decimal Area,
    int Bedrooms,
    int Bathrooms,
    int Garages,
    int? Stratum,
    ExternalAvailability Availability,
    IReadOnlyList<ExternalPhoto> Photos,
    bool ForTransfer = false);
