using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Billing;

public readonly record struct SubscriptionSelection(
    int AdditionalAdvisors,
    bool IncludeSetup,
    bool IncludeWhatsApp,
    bool IncludePortal,
    bool IncludeAdvancedReports)
{
    public const int MaximumAdditionalAdvisors = 5;

    public void Validate()
    {
        if (AdditionalAdvisors is < 0 or > MaximumAdditionalAdvisors)
            throw new DomainError("Los asesores adicionales deben estar entre 0 y 5.");
    }
}
