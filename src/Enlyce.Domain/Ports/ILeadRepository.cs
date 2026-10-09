using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Ports;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id);
    Task<Lead?> GetByEmailAsync(Email email);
    Task<Lead?> GetByOpportunityKeyAsync(string opportunityKey, CancellationToken ct = default);
    Task<LeadCreationResult> CreateOrGetExistingAsync(
        Lead lead,
        CancellationToken ct = default,
        LeadAssignmentSource assignmentSource = LeadAssignmentSource.AutomaticLoadBalance);
    Task<Lead?> ConsumeOwnerInquiryTokenAsync(string tokenHash, DateTime consumedAt, CancellationToken ct = default);
    Task<IReadOnlyList<Lead>> GetAllAsync();
    Task<IReadOnlyList<Lead>> GetByAsesorIdAsync(Guid asesorId);
    Task<IReadOnlyList<Lead>> GetByContactIdAsync(Guid contactId, CancellationToken ct = default);
    Task<Lead> SaveAsync(Lead lead);
    Task<bool> ExistsByEmailAsync(Email email);
    Task<List<Lead>> ObtenerPorEtapaAsync(string etapa);
    Task<List<Lead>> ObtenerSinAsignarAsync();
    Task<int> ContarPorAsesorAsync(Guid asesorId);
    Task<int> ContarPorEtapaAsync(string etapa);
}

public sealed record LeadCreationResult(Lead Lead, bool Created);
