namespace Enlyce.Infrastructure.Billing;

public sealed class WompiOptions
{
    public const string SectionName = "Wompi";

    public string Environment { get; init; } = "test";
    public string CheckoutUrl { get; init; } = "https://checkout.wompi.co/p/";
    public string PublicKey { get; init; } = string.Empty;
    public string IntegritySecret { get; init; } = string.Empty;
    public string EventsSecret { get; init; } = string.Empty;
    public string RedirectUrl { get; init; } = string.Empty;
}
