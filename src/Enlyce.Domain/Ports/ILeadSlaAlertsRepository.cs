namespace Enlyce.Domain.Ports;

public interface ILeadSlaAlertsRepository
{
    Task<IReadOnlyList<LeadSlaAlertCandidate>> GetCandidatesAsync(
        Guid? advisorId, CancellationToken ct = default);
}

public sealed record LeadSlaAlertCandidate(
    Guid LeadId,
    string Nombre,
    string Email,
    string TipoOperacion,
    string Fuente,
    string EtapaPipeline,
    DateTime? FechaPrimerContacto,
    DateTime? FechaUltimaInteraccion,
    DateTime? FechaPrimeraAsignacion);
