using Enlyce.Application.Abstractions;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.UseCases.GetInmuebleById;

public class GetInmuebleByIdHandler : IQueryHandler<GetInmuebleByIdQuery, GetInmuebleByIdResponse?>
{
    private readonly IInmuebleRepository _inmuebleRepo;

    public GetInmuebleByIdHandler(IInmuebleRepository inmuebleRepo)
    {
        _inmuebleRepo = inmuebleRepo;
    }

    public async Task<GetInmuebleByIdResponse?> HandleAsync(GetInmuebleByIdQuery query, CancellationToken ct = default)
    {
        var inmueble = await _inmuebleRepo.GetByIdAsync(query.Id);
        if (inmueble is null) return null;

        return new GetInmuebleByIdResponse(
            inmueble.Id,
            inmueble.Nombre,
            inmueble.Descripcion,
            inmueble.Tipo.ToString(),
            inmueble.Modalidad.ToString(),
            inmueble.Estado.ToString(),
            inmueble.Direccion.ToString(),
            inmueble.Precio.Monto,
            inmueble.Precio.Moneda,
            inmueble.MetrosCuadrados,
            inmueble.Habitaciones,
            inmueble.Banos,
            inmueble.Parqueaderos,
            inmueble.PropietarioId,
            inmueble.FechaCreacion,
            inmueble.Activo);
    }
}
