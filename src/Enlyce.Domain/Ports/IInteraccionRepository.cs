using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IInteraccionRepository
{
    Task<InteractionSearchPage> SearchAsync(Guid? advisorId, string? type, string? query,
        DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default);
    Task<List<Interaccion>> ObtenerPorLeadAsync(Guid leadId);
    Task<List<Interaccion>> ObtenerHastaPrimerContactoAsync(
        IReadOnlyCollection<Guid> leadIds,
        CancellationToken ct = default);
    Task<int> ContarPorLeadAsync(Guid leadId);
    Task AgregarAsync(Interaccion interaccion);
}

public sealed record InteractionSearchPage(int Total, IReadOnlyList<InteractionSearchItem> Items);
public sealed record InteractionSearchItem(Guid Id, Guid LeadId, Guid? ContactId, Guid AdvisorId,
    string Type, string? Summary, DateTime Date, string ClientName);
