using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Lead;

public sealed class RegistrarInteraccionHandler
{
    private readonly ILeadRepository _leadRepo;
    private readonly IInteraccionRepository _interaccionRepo;

    public RegistrarInteraccionHandler(
        ILeadRepository leadRepo, IInteraccionRepository interaccionRepo)
    {
        _leadRepo = leadRepo;
        _interaccionRepo = interaccionRepo;
    }

    public async Task<bool> HandleAsync(
        RegistrarInteraccionCommand command, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(command.LeadId);
        if (lead is null) return false;

        var interaccion = Interaccion.Registrar(
            command.LeadId, command.AsesorId, command.Tipo, command.Resumen);

        lead.IncrementarInteracciones();

        await _interaccionRepo.AgregarAsync(interaccion);
        await _leadRepo.SaveAsync(lead);
        return true;
    }
}
