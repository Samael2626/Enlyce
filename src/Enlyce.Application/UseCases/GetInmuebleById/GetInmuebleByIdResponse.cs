namespace Enlyce.Application.UseCases.GetInmuebleById;

public record GetInmuebleByIdResponse(
    Guid Id,
    string Nombre,
    string Descripcion,
    string Tipo,
    string Modalidad,
    string Estado,
    string Direccion,
    decimal Precio,
    string Moneda,
    int MetrosCuadrados,
    int Habitaciones,
    int Banos,
    int Parqueaderos,
    Guid PropietarioId,
    DateTime FechaCreacion,
    bool Activo);
