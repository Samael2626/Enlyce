using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Lead;

public sealed class RegistrarVisitaHandler
{
    private readonly ILeadRepository _leadRepo;
    private readonly IVisitaRepository _visitaRepo;

    public RegistrarVisitaHandler(
        ILeadRepository leadRepo, IVisitaRepository visitaRepo)
    {
        _leadRepo = leadRepo;
        _visitaRepo = visitaRepo;
    }

    public async Task<bool> HandleAsync(
        RegistrarVisitaCommand command, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(command.LeadId);
        if (lead is null) return false;

        lead.AgendarVisita();

        var visita = Visita.Programar(
            command.LeadId, command.InmuebleId, command.AsesorId, command.FechaProgramada);

        await _visitaRepo.AgregarAsync(visita);
        await _leadRepo.SaveAsync(lead);
        return true;
    }
}
