namespace Enlyce.Application.UseCases.CreateInmueble;

public record CreateInmuebleResponse(
    Guid Id,
    string Nombre,
    string Tipo,
    string Modalidad,
    string Direccion,
    decimal Precio,
    DateTime FechaCreacion);
