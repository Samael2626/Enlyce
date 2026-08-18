using Enlyce.Domain.Entities;

namespace Enlyce.Application.UseCases.CreateInmueble;

public record CreateInmuebleCommand(
    string Nombre,
    string? Descripcion,
    TipoInmueble Tipo,
    ModalidadInmueble Modalidad,
    string Calle,
    string Ciudad,
    string? Barrio,
    decimal Precio,
    string Moneda,
    int MetrosCuadrados,
    int Habitaciones,
    int Banos,
    int Parqueaderos,
    Guid PropietarioId);
