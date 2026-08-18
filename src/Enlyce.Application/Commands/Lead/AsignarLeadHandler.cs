using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Lead;

public sealed class AsignarLeadHandler
{
    private readonly ILeadRepository _leadRepo;

    public AsignarLeadHandler(ILeadRepository leadRepo) => _leadRepo = leadRepo;

    public async Task<bool> HandleAsync(AsignarLeadCommand command, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(command.LeadId);
        if (lead is null) return false;

        lead.AsignarAsesor(command.AsesorId);
        await _leadRepo.SaveAsync(lead);
        return true;
    }
}
