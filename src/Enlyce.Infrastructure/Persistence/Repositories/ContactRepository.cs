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

    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default) =>
        await context.Contacts.Where(contact => contact.Active)
            .OrderBy(contact => contact.Name)
            .ToListAsync(ct);

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
