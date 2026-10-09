using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Lead;

public sealed class MoverEtapaHandler
{
    private readonly ILeadStageHistoryRepository _history;

    public MoverEtapaHandler(ILeadStageHistoryRepository history) => _history = history;

    public async Task<bool> HandleAsync(MoverEtapaCommand command, CancellationToken ct = default)
    {
        return await _history.MoveStageAsync(
            command.LeadId, command.NuevaEtapa, command.ActorId, command.Reason, ct);
    }
}
