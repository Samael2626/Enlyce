using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;
using EmailAddress = Enlyce.Domain.ValueObjects.Email;

namespace Enlyce.Domain.Entities;

public sealed class Contact
{
    private static readonly HashSet<string> CustomerTypes = new(StringComparer.OrdinalIgnoreCase)
        { "Comprador", "Arrendatario", "Propietario", "Arrendador" };
    private static readonly HashSet<string> CustomerStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "Nuevo", "EnProceso", "Convertido", "Recuperado", "Perdido" };

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Surname { get; private set; }
    public string? CustomerType { get; private set; }
    public string CustomerStatus { get; private set; } = "Nuevo";
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string Country { get; private set; } = "Colombia";
    public string? Department { get; private set; }
    public string? City { get; private set; }
    public Guid? OwnerAdvisorId { get; private set; }
    public bool CanReceiveAutomatedEmail { get; private set; }
    public Telefono? Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    private Contact() { }

    private Contact(Guid id, string name, string? surname, string? email, Telefono? phone, Guid? ownerAdvisorId,
        bool canReceiveAutomatedEmail, string? customerType, string customerStatus,
        string? address, string country, string? department, string? city,
        DateTime createdAt, DateTime updatedAt, bool active)
    {
        Id = id;
        Name = name;
        Surname = surname;
        CustomerType = customerType;
        CustomerStatus = customerStatus;
        Email = email;
        Address = address;
        Country = country;
        Department = department;
        City = city;
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
        return new Contact(Guid.NewGuid(), name.Trim(), null, email.Value, phone, null, false,
            null, "Nuevo", null, "Colombia", null, null, now, now, true);
    }

    public static Contact CreateImported(string name, string? surname, string? email, Telefono? phone,
        Guid? ownerAdvisorId, bool canReceiveAutomatedEmail, string? customerType = null,
        string customerStatus = "Nuevo", string? address = null, string? country = null,
        string? department = null, string? city = null)
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
        ValidateType(customerType);
        ValidateStatus(customerStatus);
        return new Contact(Guid.NewGuid(), name.Trim(), CleanOptional(surname), email, phone, ownerAdvisorId,
            canReceiveAutomatedEmail, CleanOptional(customerType), customerStatus, CleanOptional(address),
            Normalize(country, "Colombia", 100), CleanOptional(department), CleanOptional(city), now, now, true);
    }

    public static Contact Reconstitute(Guid id, string name, string? email, Telefono? phone,
        DateTime createdAt, DateTime updatedAt, bool active, Guid? ownerAdvisorId = null,
        bool canReceiveAutomatedEmail = false, string? surname = null, string? customerType = null,
        string customerStatus = "Nuevo", string? address = null, string country = "Colombia",
        string? department = null, string? city = null) =>
        new(id, name, surname, email, phone, ownerAdvisorId, canReceiveAutomatedEmail, customerType,
            customerStatus, address, country, department, city, createdAt, updatedAt, active);

    public void AssignTo(Guid? advisorId)
    {
        OwnerAdvisorId = advisorId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? surname, Telefono? phone, string? customerType,
        string customerStatus, string? address, string? country, string? department, string? city)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainError("El nombre del contacto no puede ser vacio.");
        if (name.Trim().Length > 200)
            throw new DomainError("El nombre no puede superar 200 caracteres.");
        if (surname?.Trim().Length > 200)
            throw new DomainError("El apellido no puede superar 200 caracteres.");
        ValidateType(customerType);
        ValidateStatus(customerStatus);

        Name = name.Trim();
        Surname = CleanOptional(surname);
        CustomerType = CleanOptional(customerType);
        CustomerStatus = customerStatus;
        Phone = phone;
        Address = Normalize(address, null, 500);
        Country = Normalize(country, "Colombia", 100);
        Department = Normalize(department, null, 100);
        City = Normalize(city, null, 100);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAutomatedEmailPermission(bool allowed)
    {
        CanReceiveAutomatedEmail = allowed;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Normalize(string? value, string? fallback, int maxLength)
    {
        var normalized = CleanOptional(value) ?? fallback;
        if (normalized?.Length > maxLength)
            throw new DomainError($"El campo no puede superar {maxLength} caracteres.");
        return normalized;
    }

    private static void ValidateType(string? value)
    {
        if (value is not null && !CustomerTypes.Contains(value))
            throw new DomainError("El tipo de cliente no es valido.");
    }

    private static void ValidateStatus(string value)
    {
        if (!CustomerStatuses.Contains(value))
            throw new DomainError("El estado del cliente no es valido.");
    }
}
