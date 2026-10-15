using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.UseCases.CreateInmueble;

public class CreateInmuebleHandler : ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse>
{
    private readonly IInmuebleRepository _inmuebleRepo;
    private readonly IPropietarioRepository _propietarioRepo;
    private readonly IAsesorRepository _asesorRepo;

    public CreateInmuebleHandler(IInmuebleRepository inmuebleRepo, IPropietarioRepository propietarioRepo,
        IAsesorRepository asesorRepo)
    {
        _inmuebleRepo = inmuebleRepo;
        _propietarioRepo = propietarioRepo;
        _asesorRepo = asesorRepo;
    }

    public async Task<CreateInmuebleResponse> HandleAsync(CreateInmuebleCommand command, CancellationToken ct = default)
    {
        var propietario = await _propietarioRepo.GetByIdAsync(command.PropietarioId)
            ?? throw new InvalidOperationException($"Propietario {command.PropietarioId} no encontrado.");

        if (command.AsesorResponsableId is Guid advisorId &&
            await _asesorRepo.ObtenerPorIdAsync(advisorId) is not { Activo: true, Rol: "Asesor" })
            throw new InvalidOperationException($"Asesor responsable {advisorId} no encontrado o inactivo.");

        var direccion = Direccion.Crear(command.Calle, command.Ciudad, command.Barrio, command.Pais, command.Departamento);
        var precio = Dinero.Crear(command.Precio, command.Moneda);

        var inmueble = Inmueble.Crear(
            command.Nombre, command.Descripcion ?? string.Empty,
            command.Tipo, command.Modalidad, direccion, precio,
            command.MetrosCuadrados, command.Habitaciones,
            command.Banos, command.Parqueaderos, command.PropietarioId,
            command.CaracteristicasInternas, command.CaracteristicasExternas, command.AnioConstruccion,
            command.EstadoFisico, command.Etiquetas, command.AsesorResponsableId);

        var saved = await _inmuebleRepo.SaveAsync(inmueble);

        return new CreateInmuebleResponse(
            saved.Id, saved.Nombre, saved.Tipo.ToString(),
            saved.Modalidad.ToString(), saved.Direccion.ToString(),
            saved.Precio.Monto, saved.FechaCreacion);
    }
}
