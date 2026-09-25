using System.Text.Json;
using Enlyce.Domain.Entities;

namespace Enlyce.Application.Billing;

public sealed record PaymentCheckoutData(
    string CheckoutUrl,
    string PublicKey,
    string Currency,
    long AmountInCents,
    string Reference,
    string IntegritySignature,
    string RedirectUrl,
    string CustomerEmail);

public sealed record VerifiedPaymentNotification(
    string ProviderTransactionId,
    string Reference,
    long AmountInCents,
    string Currency,
    PaymentOrderStatus Status);

public interface IPaymentCheckoutGateway
{
    PaymentCheckoutData CreateCheckout(PaymentOrder order);
}

public interface IPaymentEventVerifier
{
    VerifiedPaymentNotification VerifyAndParse(JsonElement eventBody);
}

public sealed class PaymentGatewayUnavailableException(string message) : Exception(message);
