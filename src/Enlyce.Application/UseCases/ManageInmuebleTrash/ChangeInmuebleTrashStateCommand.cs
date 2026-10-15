namespace Enlyce.Application.UseCases.ManageInmuebleTrash;

public sealed record ChangeInmuebleTrashStateCommand(Guid Id, bool Restore);
