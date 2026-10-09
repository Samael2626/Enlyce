using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Entities;

public sealed class Contact
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Telefono? Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    private Contact() { }

    private Contact(Guid id, string name, string email, Telefono? phone, DateTime createdAt, DateTime updatedAt, bool active)
    {
        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Active = active;
    }

    public static Contact Create(string name, Email email, Telefono? phone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainError("El nombre del contacto no puede ser vacio.");
        if (name.Trim().Length > 200)
            throw new DomainError("El nombre no puede superar 200 caracteres.");

        var now = DateTime.UtcNow;
        return new Contact(Guid.NewGuid(), name.Trim(), email.Value, phone, now, now, true);
    }

    public static Contact Reconstitute(Guid id, string name, string email, Telefono? phone, DateTime createdAt, DateTime updatedAt, bool active) =>
        new(id, name, email, phone, createdAt, updatedAt, active);

    public void Update(string name, Telefono? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainError("El nombre del contacto no puede ser vacio.");
        if (name.Trim().Length > 200)
            throw new DomainError("El nombre no puede superar 200 caracteres.");

        Name = name.Trim();
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }
}
