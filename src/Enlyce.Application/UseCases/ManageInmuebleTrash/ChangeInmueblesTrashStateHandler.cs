using Enlyce.Application.Abstractions;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.UseCases.ManageInmuebleTrash;

public sealed class ChangeInmueblesTrashStateHandler(IInmuebleRepository properties)
    : ICommandHandler<ChangeInmueblesTrashStateCommand, bool>
{
    public async Task<bool> HandleAsync(ChangeInmueblesTrashStateCommand command, CancellationToken ct = default)
    {
        if (command.Ids.Count is < 1 or > 100 || command.Ids.Distinct().Count() != command.Ids.Count)
            return false;

        var items = await properties.GetByIdsForUpdateAsync(command.Ids, ct);
        if (items.Count != command.Ids.Count || items.Any(item => item.Activo == command.Restore))
            return false;

        foreach (var item in items)
        {
            if (command.Restore) item.RestaurarDePapelera();
            else item.Desactivar();
        }

        await properties.SaveChangesAsync(ct);
        return true;
    }
}
