using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Ports;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Contact?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<IReadOnlySet<string>> GetExistingEmailsAsync(
        IReadOnlyCollection<string> emails,
        IReadOnlyCollection<Guid>? allowedContactIds = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetIdsByOwnerAdvisorAsync(Guid advisorId, CancellationToken ct = default);
    Task AssignContactAsync(Contact contact, Guid? advisorId, CancellationToken ct = default);
    Task AddAsync(Contact contact, CancellationToken ct = default);
    Task<ContactImportBatch?> GetImportBatchAsync(string actorId, Guid requestId, CancellationToken ct = default);
    Task<IContactImportLock> AcquireClientImportLockAsync(CancellationToken ct = default);
    Task AddImportedAsync(IReadOnlyCollection<Contact> contacts, ContactImportBatch batch, CancellationToken ct = default);
    Task SetEmailPermissionAsync(Contact contact, ContactEmailPermissionEvent permissionEvent, CancellationToken ct = default);
    Task<IReadOnlyList<ContactEmailPermissionEvent>> GetEmailPermissionHistoryAsync(Guid contactId, CancellationToken ct = default);
    Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default);
    Task<ContactSearchPage> SearchAsync(
        string? query,
        DateTime? createdFrom,
        DateTime? createdThrough,
        IReadOnlyCollection<Guid>? allowedContactIds,
        int skip,
        int take,
        CancellationToken ct = default,
        string? customerType = null,
        string? customerStatus = null,
        string? customerFunnel = null,
        string? customerLabel = null);
    Task<IReadOnlyList<Contact>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<Contact> CreateOrGetAsync(Contact contact, CancellationToken ct = default);
    Task SaveAsync(Contact contact, CancellationToken ct = default);
}

public interface IContactImportLock : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}

public sealed record ContactSearchPage(int Total, IReadOnlyList<Contact> Items);
