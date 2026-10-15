using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;
using EmailAddress = Enlyce.Domain.ValueObjects.Email;

namespace Enlyce.Domain.Entities;

public sealed class Contact
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Surname { get; private set; }
    public string? CustomerType { get; private set; }
    public string[] CustomerTypes { get; private set; } = [];
    public string[] CustomerLabels { get; private set; } = [];
    public string CustomerStatus { get; private set; } = "Nuevo";
    public string CustomerFunnel { get; private set; } = "Clientes";
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string Country { get; private set; } = "Colombia";
    public string? Department { get; private set; }
    public string? City { get; private set; }
    public Guid? OwnerAdvisorId { get; private set; }
    public bool CanReceiveAutomatedEmail { get; private set; }
    public Telefono? Phone { get; private set; }
    public Telefono? Landline { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? IdentificationNumber { get; private set; }
    public string? ReferredBy { get; private set; }
    public string? AcquisitionMedium { get; private set; }
    public string? AdditionalData { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    private Contact() { }

    private Contact(Guid id, string name, string? surname, string? email, Telefono? phone, Guid? ownerAdvisorId,
        bool canReceiveAutomatedEmail, string? customerType, string customerStatus,
        string? address, string country, string? department, string? city,
        DateTime createdAt, DateTime updatedAt, bool active, Telefono? landline = null,
        DateOnly? dateOfBirth = null, string? identificationNumber = null, string? referredBy = null,
        string? acquisitionMedium = null, string? additionalData = null, string customerFunnel = "Clientes",
        string[]? customerTypes = null, string[]? customerLabels = null)
    {
        Id = id;
        Name = name;
        Surname = surname;
        CustomerType = customerType;
        CustomerTypes = customerTypes ?? (customerType is null ? [] : [customerType]);
        CustomerLabels = customerLabels ?? [];
        CustomerStatus = customerStatus;
        CustomerFunnel = customerFunnel;
        Email = email;
        Address = address;
        Country = country;
        Department = department;
        City = city;
        Phone = phone;
        Landline = landline;
        DateOfBirth = dateOfBirth;
        IdentificationNumber = identificationNumber;
        ReferredBy = referredBy;
        AcquisitionMedium = acquisitionMedium;
        AdditionalData = additionalData;
        OwnerAdvisorId = ownerAdvisorId;
        CanReceiveAutomatedEmail = canReceiveAutomatedEmail;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Active = active;
    }

    public static Contact Create(string name, Email email, Telefono? phone = null,
        string customerStatus = "Nuevo", string customerFunnel = "Clientes")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainError("El nombre del contacto no puede ser vacio.");
        if (name.Trim().Length > 200)
            throw new DomainError("El nombre no puede superar 200 caracteres.");

        var now = DateTime.UtcNow;
        ValidateStatus(customerStatus);
        customerFunnel = ValidateFunnel(customerFunnel);
        return new Contact(Guid.NewGuid(), name.Trim(), null, email.Value, phone, null, false,
            null, customerStatus.Trim(), null, "Colombia", null, null, now, now, true,
            customerFunnel: customerFunnel);
    }

    public static Contact CreateImported(string name, string? surname, string? email, Telefono? phone,
        Guid? ownerAdvisorId, bool canReceiveAutomatedEmail, string? customerType = null,
        string customerStatus = "Nuevo", string? address = null, string? country = null,
        string? department = null, string? city = null, Telefono? landline = null,
        DateOnly? dateOfBirth = null, string? identificationNumber = null, string? referredBy = null,
        string? acquisitionMedium = null, string? additionalData = null, string customerFunnel = "Clientes",
        string[]? customerTypes = null, string[]? customerLabels = null)
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
        var normalizedTypes = NormalizeCustomerTypes(customerTypes, customerType);
        var normalizedLabels = NormalizeCustomerLabels(customerLabels);
        customerType = normalizedTypes.FirstOrDefault();
        ValidateStatus(customerStatus);
        customerFunnel = ValidateFunnel(customerFunnel);
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainError("La fecha de nacimiento no puede estar en el futuro.");
        return new Contact(Guid.NewGuid(), name.Trim(), CleanOptional(surname), email, phone, ownerAdvisorId,
            canReceiveAutomatedEmail, CleanOptional(customerType), customerStatus, CleanOptional(address),
            Normalize(country, "Colombia", 100)!, CleanOptional(department), CleanOptional(city), now, now, true,
            landline, dateOfBirth, Normalize(identificationNumber, null, 50), Normalize(referredBy, null, 200),
            Normalize(acquisitionMedium, null, 200), Normalize(additionalData, null, 4000), customerFunnel, normalizedTypes, normalizedLabels);
    }

    public static Contact Reconstitute(Guid id, string name, string? email, Telefono? phone,
        DateTime createdAt, DateTime updatedAt, bool active, Guid? ownerAdvisorId = null,
        bool canReceiveAutomatedEmail = false, string? surname = null, string? customerType = null,
        string customerStatus = "Nuevo", string? address = null, string country = "Colombia",
        string? department = null, string? city = null, string customerFunnel = "Clientes",
        string[]? customerTypes = null, string[]? customerLabels = null) =>
        new(id, name, surname, email, phone, ownerAdvisorId, canReceiveAutomatedEmail, customerType,
            customerStatus, address, country, department, city, createdAt, updatedAt, active,
            customerFunnel: customerFunnel, customerTypes: NormalizeCustomerTypes(customerTypes, customerType),
            customerLabels: NormalizeCustomerLabels(customerLabels));

    public void AssignTo(Guid? advisorId)
    {
        OwnerAdvisorId = advisorId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? surname, Telefono? phone, string? customerType,
        string customerStatus, string? address, string? country, string? department, string? city,
        Telefono? landline = null, DateOnly? dateOfBirth = null, string? identificationNumber = null,
        string? referredBy = null, string? acquisitionMedium = null, string? additionalData = null,
        string customerFunnel = "Clientes", string[]? customerTypes = null, string[]? customerLabels = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainError("El nombre del contacto no puede ser vacio.");
        if (name.Trim().Length > 200)
            throw new DomainError("El nombre no puede superar 200 caracteres.");
        if (surname?.Trim().Length > 200)
            throw new DomainError("El apellido no puede superar 200 caracteres.");
        var normalizedTypes = NormalizeCustomerTypes(customerTypes, customerType);
        var normalizedLabels = customerLabels is null ? CustomerLabels : NormalizeCustomerLabels(customerLabels);
        customerType = normalizedTypes.FirstOrDefault();
        ValidateStatus(customerStatus);
        customerFunnel = ValidateFunnel(customerFunnel);
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainError("La fecha de nacimiento no puede estar en el futuro.");

        Name = name.Trim();
        Surname = CleanOptional(surname);
        CustomerTypes = normalizedTypes;
        CustomerLabels = normalizedLabels;
        CustomerType = customerType;
        CustomerStatus = customerStatus;
        CustomerFunnel = customerFunnel;
        Phone = phone;
        Landline = landline;
        DateOfBirth = dateOfBirth;
        IdentificationNumber = Normalize(identificationNumber, null, 50);
        ReferredBy = Normalize(referredBy, null, 200);
        AcquisitionMedium = Normalize(acquisitionMedium, null, 200);
        AdditionalData = Normalize(additionalData, null, 4000);
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

    private static string[] NormalizeCustomerTypes(string[]? values, string? fallback)
    {
        var source = values is { Length: > 0 } ? values : fallback is null ? [] : [fallback];
        var types = source.Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (types.Length > 10 || types.Any(value => value.Length > 30))
            throw new DomainError("Cada cliente admite hasta 10 tipos de máximo 30 caracteres.");
        return types;
    }

    private static string[] NormalizeCustomerLabels(string[]? values)
    {
        var labels = (values ?? []).Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (labels.Length > 20 || labels.Any(value => value.Length > 50))
            throw new DomainError("Cada cliente admite hasta 20 etiquetas de máximo 50 caracteres.");
        return labels;
    }

    private static void ValidateStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 100)
            throw new DomainError("La etapa del cliente es obligatoria y no puede superar 100 caracteres.");
    }

    private static string ValidateFunnel(string? value)
    {
        var funnel = value?.Trim();
        if (string.IsNullOrWhiteSpace(funnel) || funnel.Length > 100)
            throw new DomainError("El embudo del cliente es obligatorio y no puede superar 100 caracteres.");
        return funnel;
    }
}
