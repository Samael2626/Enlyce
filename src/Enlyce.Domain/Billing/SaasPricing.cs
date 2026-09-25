namespace Enlyce.Domain.Billing;

public static class SaasPricing
{
    public const long BasePlanInCents = 8_900_000;
    public const long AdditionalAdvisorInCents = 1_900_000;
    public const long SetupInCents = 15_000_000;
    public const long WhatsAppInCents = 4_500_000;
    public const long PortalInCents = 3_500_000;
    public const long AdvancedReportsInCents = 2_500_000;

    public static long CalculateAmountInCents(SubscriptionSelection selection)
    {
        selection.Validate();

        return BasePlanInCents
            + (selection.AdditionalAdvisors * AdditionalAdvisorInCents)
            + (selection.IncludeSetup ? SetupInCents : 0)
            + (selection.IncludeWhatsApp ? WhatsAppInCents : 0)
            + (selection.IncludePortal ? PortalInCents : 0)
            + (selection.IncludeAdvancedReports ? AdvancedReportsInCents : 0);
    }
}
