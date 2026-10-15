namespace Enlyce.Application.UseCases.ManageInmuebleTrash;

public sealed record ChangeInmueblesTrashStateCommand(IReadOnlyList<Guid> Ids, bool Restore);
