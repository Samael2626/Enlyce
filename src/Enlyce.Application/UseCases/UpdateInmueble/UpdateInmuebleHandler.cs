using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.UseCases.UpdateInmueble;

public sealed class UpdateInmuebleHandler(IInmuebleRepository properties)
    : ICommandHandler<UpdateInmuebleCommand, CreateInmuebleResponse?>
{
    public async Task<CreateInmuebleResponse?> HandleAsync(UpdateInmuebleCommand command, CancellationToken ct = default)
    {
        var property = await properties.GetByIdAsync(command.Id);
        if (property is null || !property.Activo)
            return null;

        property.ActualizarDatos(command.Nombre, command.Descripcion ?? string.Empty, command.Tipo,
            command.Modalidad,
            Direccion.Crear(command.Calle, command.Ciudad, command.Barrio, command.Pais, command.Departamento),
            Dinero.Crear(command.Precio, command.Moneda), command.MetrosCuadrados,
            command.Habitaciones, command.Banos, command.Parqueaderos,
            command.CaracteristicasInternas, command.CaracteristicasExternas, command.AnioConstruccion,
            command.EstadoFisico);

        var saved = await properties.SaveAsync(property);
        return new CreateInmuebleResponse(saved.Id, saved.Nombre, saved.Tipo.ToString(),
            saved.Modalidad.ToString(), saved.Direccion.ToString(), saved.Precio.Monto, saved.FechaCreacion);
    }
}
