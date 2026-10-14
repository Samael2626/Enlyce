using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;
using EmailAddress = Enlyce.Domain.ValueObjects.Email;

namespace Enlyce.Domain.Entities;

public sealed class Contact
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Surname { get; private set; }
    public string? Email { get; private set; }
    public Guid? OwnerAdvisorId { get; private set; }
    public bool CanReceiveAutomatedEmail { get; private set; }
    public Telefono? Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    private Contact() { }

    private Contact(Guid id, string name, string? surname, string? email, Telefono? phone, Guid? ownerAdvisorId,
        bool canReceiveAutomatedEmail,
        DateTime createdAt, DateTime updatedAt, bool active)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Email = email;
        Phone = phone;
        OwnerAdvisorId = ownerAdvisorId;
        CanReceiveAutomatedEmail = canReceiveAutomatedEmail;
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
        return new Contact(Guid.NewGuid(), name.Trim(), null, email.Value, phone, null, false, now, now, true);
    }

    public static Contact CreateImported(string name, string? surname, string? email, Telefono? phone,
        Guid? ownerAdvisorId, bool canReceiveAutomatedEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainError("El nombre del contacto no puede ser vacio.");
        if (name.Trim().Length > 200)
            throw new DomainError("El nombre no puede superar 200 caracteres.");
        if (surname?.Trim().Length > 200)
            throw new DomainError("El apellido no puede superar 200 caracteres.");
        if (email is { Length: > 0 })
            email = EmailAddress.Create(email).Value;

        var now = DateTime.UtcNow;
        return new Contact(Guid.NewGuid(), name.Trim(), CleanOptional(surname), email, phone, ownerAdvisorId,
            canReceiveAutomatedEmail, now, now, true);
    }

    public static Contact Reconstitute(Guid id, string name, string? email, Telefono? phone,
        DateTime createdAt, DateTime updatedAt, bool active, Guid? ownerAdvisorId = null,
        bool canReceiveAutomatedEmail = false, string? surname = null) =>
        new(id, name, surname, email, phone, ownerAdvisorId, canReceiveAutomatedEmail, createdAt, updatedAt, active);

    public void AssignTo(Guid? advisorId)
    {
        OwnerAdvisorId = advisorId;
        UpdatedAt = DateTime.UtcNow;
    }

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

    public void SetAutomatedEmailPermission(bool allowed)
    {
        CanReceiveAutomatedEmail = allowed;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
