using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Lead;

public sealed class ReasignarLeadHandler
{
    private readonly ILeadRepository _leadRepo;

    public ReasignarLeadHandler(ILeadRepository leadRepo) => _leadRepo = leadRepo;

    public async Task<bool> HandleAsync(ReasignarLeadCommand command, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(command.LeadId);
        if (lead is null) return false;

        lead.AsignarAsesor(command.NuevoAsesorId);
        await _leadRepo.SaveAsync(lead);
        return true;
    }
}
