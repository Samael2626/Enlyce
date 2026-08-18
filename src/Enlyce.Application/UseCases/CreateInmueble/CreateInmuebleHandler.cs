using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.UseCases.CreateInmueble;

public class CreateInmuebleHandler : ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse>
{
    private readonly IInmuebleRepository _inmuebleRepo;
    private readonly IPropietarioRepository _propietarioRepo;

    public CreateInmuebleHandler(IInmuebleRepository inmuebleRepo, IPropietarioRepository propietarioRepo)
    {
        _inmuebleRepo = inmuebleRepo;
        _propietarioRepo = propietarioRepo;
    }

    public async Task<CreateInmuebleResponse> HandleAsync(CreateInmuebleCommand command, CancellationToken ct = default)
    {
        var propietario = await _propietarioRepo.GetByIdAsync(command.PropietarioId)
            ?? throw new InvalidOperationException($"Propietario {command.PropietarioId} no encontrado.");

        var direccion = Direccion.Crear(command.Calle, command.Ciudad, command.Barrio);
        var precio = Dinero.Crear(command.Precio, command.Moneda);

        var inmueble = Inmueble.Crear(
            command.Nombre, command.Descripcion ?? string.Empty,
            command.Tipo, command.Modalidad, direccion, precio,
            command.MetrosCuadrados, command.Habitaciones,
            command.Banos, command.Parqueaderos, command.PropietarioId);

        var saved = await _inmuebleRepo.SaveAsync(inmueble);

        return new CreateInmuebleResponse(
            saved.Id, saved.Nombre, saved.Tipo.ToString(),
            saved.Modalidad.ToString(), saved.Direccion.ToString(),
            saved.Precio.Monto, saved.FechaCreacion);
    }
}
