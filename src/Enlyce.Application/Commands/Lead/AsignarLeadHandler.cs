using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.Errors;

namespace Enlyce.Application.Commands.Lead;

public sealed class AsignarLeadHandler
{
    private readonly ILeadRepository _leadRepo;
    private readonly IAsesorRepository _asesorRepo;
    private readonly ILeadAssignmentHistoryRepository _historyRepo;

    public AsignarLeadHandler(
        ILeadRepository leadRepo,
        IAsesorRepository asesorRepo,
        ILeadAssignmentHistoryRepository historyRepo)
    {
        _leadRepo = leadRepo;
        _asesorRepo = asesorRepo;
        _historyRepo = historyRepo;
    }

    public async Task<bool> HandleAsync(AsignarLeadCommand command, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(command.LeadId);
        if (lead is null) return false;

        var advisor = await _asesorRepo.ObtenerPorIdAsync(command.AsesorId);
        if (advisor is null || !advisor.Activo || advisor.Rol != "Asesor")
            throw new DomainError("Solo se pueden asignar oportunidades a asesores activos.");

        if (lead.AsesorAsignadoId == command.AsesorId)
            return true;

        var source = lead.AsesorAsignadoId.HasValue
            ? LeadAssignmentSource.ManualReassignment
            : LeadAssignmentSource.ManualAssignment;
        return await _historyRepo.ChangeAssignmentAsync(
            lead, command.AsesorId, command.ActorId, command.Reason, source, ct);
    }
}
