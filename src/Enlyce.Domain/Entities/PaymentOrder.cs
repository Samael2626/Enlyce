using Enlyce.Domain.Billing;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Entities;

public enum PaymentOrderStatus
{
    Pending = 0,
    Approved = 1,
    Declined = 2,
    Voided = 3,
    Error = 4
}

public sealed class PaymentOrder
{
    public Guid Id { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public int AdditionalAdvisors { get; private set; }
    public bool IncludesSetup { get; private set; }
    public bool IncludesWhatsApp { get; private set; }
    public bool IncludesPortal { get; private set; }
    public bool IncludesAdvancedReports { get; private set; }
    public long AmountInCents { get; private set; }
    public string Currency { get; private set; } = "COP";
    public PaymentOrderStatus Status { get; private set; }
    public string? ProviderTransactionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private PaymentOrder() { }

    public static PaymentOrder Create(
        string companyName,
        string taxId,
        string customerEmail,
        SubscriptionSelection selection,
        DateTime now)
    {
        selection.Validate();
        ValidateRequired(companyName, taxId, customerEmail);

        var id = Guid.NewGuid();
        return new PaymentOrder
        {
            Id = id,
            Reference = $"ENL-{id:N}",
            CompanyName = companyName.Trim(),
            TaxId = taxId.Trim(),
            CustomerEmail = Email.Create(customerEmail.Trim()).Value,
            AdditionalAdvisors = selection.AdditionalAdvisors,
            IncludesSetup = selection.IncludeSetup,
            IncludesWhatsApp = selection.IncludeWhatsApp,
            IncludesPortal = selection.IncludePortal,
            IncludesAdvancedReports = selection.IncludeAdvancedReports,
            AmountInCents = SaasPricing.CalculateAmountInCents(selection),
            Status = PaymentOrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void ApplyProviderUpdate(
        string providerTransactionId,
        PaymentOrderStatus nextStatus,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(providerTransactionId))
            throw new DomainError("La transaccion del proveedor es obligatoria.");

        if (ProviderTransactionId is not null &&
            !string.Equals(ProviderTransactionId, providerTransactionId, StringComparison.Ordinal))
            throw new DomainError("La orden ya esta asociada a otra transaccion.");

        if (!CanTransition(Status, nextStatus))
            throw new DomainError($"No se puede cambiar el pago de {Status} a {nextStatus}.");

        ProviderTransactionId = providerTransactionId;
        Status = nextStatus;
        UpdatedAt = now;
    }

    private static bool CanTransition(PaymentOrderStatus current, PaymentOrderStatus next) =>
        current == next ||
        current == PaymentOrderStatus.Pending ||
        (current == PaymentOrderStatus.Approved && next == PaymentOrderStatus.Voided);

    private static void ValidateRequired(string companyName, string taxId, string customerEmail)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainError("La empresa es obligatoria.");
        if (companyName.Trim().Length > 160)
            throw new DomainError("La empresa no puede superar 160 caracteres.");
        if (string.IsNullOrWhiteSpace(taxId) || taxId.Trim().Length > 32)
            throw new DomainError("El NIT es obligatorio y no puede superar 32 caracteres.");
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new DomainError("El correo del pagador es obligatorio.");
        if (customerEmail.Trim().Length > 254)
            throw new DomainError("El correo del pagador no puede superar 254 caracteres.");
        _ = Email.Create(customerEmail.Trim());
    }
}
