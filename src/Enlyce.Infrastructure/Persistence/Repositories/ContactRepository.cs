using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using EmailAddress = Enlyce.Domain.ValueObjects.Email;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class ContactRepository(EnlyceDbContext context) : IContactRepository
{
    public Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Contacts.SingleOrDefaultAsync(contact => contact.Id == id, ct);

    public Task<Contact?> GetByEmailAsync(EmailAddress email, CancellationToken ct = default) =>
        context.Contacts.SingleOrDefaultAsync(contact => contact.Email == email.Value, ct);

    public async Task<IReadOnlySet<string>> GetExistingEmailsAsync(
        IReadOnlyCollection<string> emails,
        IReadOnlyCollection<Guid>? allowedContactIds = null,
        CancellationToken ct = default)
    {
        if (emails.Count == 0 || allowedContactIds is { Count: 0 })
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var normalized = emails.Select(email => email.Trim().ToLowerInvariant()).Distinct().ToArray();
        var query = context.Contacts.AsNoTracking()
            .Where(contact => contact.Email != null && normalized.Contains(contact.Email.ToLower()))
            .Select(contact => new { contact.Id, contact.Email });
        if (allowedContactIds is not null)
            query = query.Where(contact => allowedContactIds.Contains(contact.Id));

        var existing = await query.Select(contact => contact.Email)
            .Where(email => email != null)
            .Select(email => email!)
            .ToListAsync(ct);
        return new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<Guid>> GetIdsByOwnerAdvisorAsync(Guid advisorId, CancellationToken ct = default) =>
        await context.Contacts.AsNoTracking()
            .Where(contact => contact.Active && contact.OwnerAdvisorId == advisorId)
            .Select(contact => contact.Id)
            .ToListAsync(ct);

    public async Task AssignContactAsync(Contact contact, Guid? advisorId, CancellationToken ct = default)
    {
        contact.AssignTo(advisorId);
        context.Contacts.Update(contact);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddImportedAsync(IReadOnlyCollection<Contact> contacts, CancellationToken ct = default)
    {
        if (contacts.Count == 0)
            return;
        await context.Contacts.AddRangeAsync(contacts, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default) =>
        await context.Contacts.Where(contact => contact.Active)
            .OrderBy(contact => contact.Name)
            .ThenBy(contact => contact.Id)
            .ToListAsync(ct);

    public async Task<ContactSearchPage> SearchAsync(
        string? query,
        DateTime? createdFrom,
        DateTime? createdThrough,
        IReadOnlyCollection<Guid>? allowedContactIds,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        var contacts = context.Contacts.AsNoTracking().Where(contact => contact.Active);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var search = query.Trim().ToLowerInvariant();
            contacts = contacts.Where(contact => contact.Name.ToLower().Contains(search)
                || (contact.Email != null && contact.Email.ToLower().Contains(search))
                || (contact.Phone != null && contact.Phone.Value.ToLower().Contains(search)));
        }

        if (createdFrom.HasValue)
            contacts = contacts.Where(contact => contact.CreatedAt >= createdFrom.Value);
        if (createdThrough.HasValue)
            contacts = contacts.Where(contact => contact.CreatedAt <= createdThrough.Value);
        if (allowedContactIds is not null)
        {
            if (allowedContactIds.Count == 0)
                return new ContactSearchPage(0, []);
            contacts = contacts.Where(contact => allowedContactIds.Contains(contact.Id));
        }

        var total = await contacts.CountAsync(ct);
        var items = await contacts.OrderBy(contact => contact.Name)
            .ThenBy(contact => contact.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
        return new ContactSearchPage(total, items);
    }

    public async Task<IReadOnlyList<Contact>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        return await context.Contacts.Where(contact => ids.Contains(contact.Id) && contact.Active)
            .OrderBy(contact => contact.Name)
            .ToListAsync(ct);
    }

    public async Task<Contact> CreateOrGetAsync(Contact contact, CancellationToken ct = default)
    {
        context.Contacts.Add(contact);
        try
        {
            await context.SaveChangesAsync(ct);
            return contact;
        }
        catch (DbUpdateException)
        {
            context.Entry(contact).State = EntityState.Detached;
            if (contact.Email is null)
                throw;

            var existing = await GetByEmailAsync(EmailAddress.Create(contact.Email), ct);
            if (existing is null)
                throw;

            return existing;
        }
    }

    public async Task SaveAsync(Contact contact, CancellationToken ct = default)
    {
        context.Contacts.Update(contact);
        await context.SaveChangesAsync(ct);
    }
}
