using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Lead;

public sealed class SuprimirDatosLeadHandler
{
    private readonly ILeadRepository _leadRepo;

    public SuprimirDatosLeadHandler(ILeadRepository leadRepo)
    {
        _leadRepo = leadRepo;
    }

    public async Task<bool> HandleAsync(
        SuprimirDatosLeadCommand command, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(command.LeadId);
        if (lead is null) return false;

        lead.Desactivar();
        await _leadRepo.SaveAsync(lead);
        return true;
    }
}
