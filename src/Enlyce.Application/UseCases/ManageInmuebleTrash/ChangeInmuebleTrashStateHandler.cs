using Enlyce.Application.Abstractions;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.UseCases.ManageInmuebleTrash;

public sealed class ChangeInmuebleTrashStateHandler(IInmuebleRepository properties)
    : ICommandHandler<ChangeInmuebleTrashStateCommand, bool>
{
    public async Task<bool> HandleAsync(ChangeInmuebleTrashStateCommand command, CancellationToken ct = default)
    {
        var property = await properties.GetByIdAsync(command.Id);
        if (property is null || property.Activo == command.Restore) return false;

        if (command.Restore) property.RestaurarDePapelera();
        else property.Desactivar();

        await properties.SaveAsync(property);
        return true;
    }
}
