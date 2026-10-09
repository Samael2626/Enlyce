using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Ports;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Contact?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Contact>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<Contact> CreateOrGetAsync(Contact contact, CancellationToken ct = default);
    Task SaveAsync(Contact contact, CancellationToken ct = default);
}
