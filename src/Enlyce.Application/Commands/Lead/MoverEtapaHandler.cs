using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Lead;

public sealed class MoverEtapaHandler
{
    private readonly ILeadRepository _leadRepo;

    public MoverEtapaHandler(ILeadRepository leadRepo) => _leadRepo = leadRepo;

    public async Task<bool> HandleAsync(MoverEtapaCommand command, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(command.LeadId);
        if (lead is null) return false;

        lead.MoverEtapa(command.NuevaEtapa);
        await _leadRepo.SaveAsync(lead);
        return true;
    }
}
