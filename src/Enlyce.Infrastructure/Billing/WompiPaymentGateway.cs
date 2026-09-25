using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enlyce.Application.Billing;
using Enlyce.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Enlyce.Infrastructure.Billing;

public sealed class WompiPaymentGateway(IOptions<WompiOptions> options)
    : IPaymentCheckoutGateway, IPaymentEventVerifier
{
    private readonly WompiOptions _options = options.Value;

    public PaymentCheckoutData CreateCheckout(PaymentOrder order)
    {
        ValidateCheckoutConfiguration();
        var signature = ComputeSha256Hex(
            $"{order.Reference}{order.AmountInCents}{order.Currency}{_options.IntegritySecret}");

        return new PaymentCheckoutData(
            _options.CheckoutUrl,
            _options.PublicKey,
            order.Currency,
            order.AmountInCents,
            order.Reference,
            signature,
            BuildRedirectUrl(order.Reference),
            order.CustomerEmail);
    }

    public VerifiedPaymentNotification VerifyAndParse(JsonElement eventBody)
    {
        if (string.IsNullOrWhiteSpace(_options.EventsSecret))
            throw new PaymentGatewayUnavailableException("Wompi no tiene configurado el secreto de eventos.");

        var environment = GetRequiredString(eventBody, "environment");
        if (!string.Equals(environment, _options.Environment, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("El ambiente del evento de Wompi no coincide.");

        if (!string.Equals(GetRequiredString(eventBody, "event"), "transaction.updated", StringComparison.Ordinal))
            throw new UnauthorizedAccessException("El tipo de evento de Wompi no esta permitido.");

        var data = GetRequiredObject(eventBody, "data");
        var signature = GetRequiredObject(eventBody, "signature");
        var properties = GetRequiredArray(signature, "properties");
        var timestamp = GetRequiredInt64(eventBody, "timestamp");
        var checksum = GetRequiredString(signature, "checksum");

        var signedValue = new StringBuilder();
        foreach (var property in properties.EnumerateArray())
        {
            if (property.ValueKind != JsonValueKind.String)
                throw new UnauthorizedAccessException("La firma del evento de Wompi es invalida.");

            signedValue.Append(ReadPathValue(data, property.GetString()!));
        }

        signedValue.Append(timestamp);
        signedValue.Append(_options.EventsSecret);
        var expectedChecksum = ComputeSha256Hex(signedValue.ToString());
        if (!FixedTimeHexEquals(expectedChecksum, checksum))
            throw new UnauthorizedAccessException("La firma del evento de Wompi es invalida.");

        var transaction = GetRequiredObject(data, "transaction");
        return new VerifiedPaymentNotification(
            GetRequiredString(transaction, "id"),
            GetRequiredString(transaction, "reference"),
            GetRequiredInt64(transaction, "amount_in_cents"),
            GetRequiredString(transaction, "currency"),
            ParseStatus(GetRequiredString(transaction, "status")));
    }

    private void ValidateCheckoutConfiguration()
    {
        if (_options.Environment is not ("test" or "prod"))
            throw new PaymentGatewayUnavailableException("El ambiente de Wompi no es valido.");

        var expectedPrefix = _options.Environment == "prod" ? "pub_prod_" : "pub_test_";
        if (!_options.PublicKey.StartsWith(expectedPrefix, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(_options.IntegritySecret) ||
            !Uri.TryCreate(_options.CheckoutUrl, UriKind.Absolute, out var checkoutUri) ||
            checkoutUri.Scheme != Uri.UriSchemeHttps ||
            !Uri.TryCreate(_options.RedirectUrl, UriKind.Absolute, out _))
            throw new PaymentGatewayUnavailableException("Wompi sandbox no esta configurado correctamente.");
    }

    private string BuildRedirectUrl(string reference)
    {
        var builder = new UriBuilder(_options.RedirectUrl);
        var referenceQuery = $"reference={Uri.EscapeDataString(reference)}";
        builder.Query = string.IsNullOrWhiteSpace(builder.Query)
            ? referenceQuery
            : $"{builder.Query.TrimStart('?')}&{referenceQuery}";
        return builder.Uri.AbsoluteUri;
    }

    private static PaymentOrderStatus ParseStatus(string status) => status switch
    {
        "PENDING" => PaymentOrderStatus.Pending,
        "APPROVED" => PaymentOrderStatus.Approved,
        "DECLINED" => PaymentOrderStatus.Declined,
        "VOIDED" => PaymentOrderStatus.Voided,
        "ERROR" => PaymentOrderStatus.Error,
        _ => throw new UnauthorizedAccessException("El estado de Wompi no es reconocido.")
    };

    private static string ReadPathValue(JsonElement root, string path)
    {
        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
                throw new UnauthorizedAccessException("La firma referencia un campo inexistente.");
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString() ?? string.Empty,
            JsonValueKind.Number => current.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            _ => throw new UnauthorizedAccessException("La firma referencia un valor no escalar.")
        };
    }

    private static JsonElement GetRequiredObject(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Object)
            throw new UnauthorizedAccessException($"Wompi no envio el objeto {name}.");
        return value;
    }

    private static JsonElement GetRequiredArray(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
            throw new UnauthorizedAccessException($"Wompi no envio el arreglo {name}.");
        return value;
    }

    private static string GetRequiredString(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(value.GetString()))
            throw new UnauthorizedAccessException($"Wompi no envio el campo {name}.");
        return value.GetString()!;
    }

    private static long GetRequiredInt64(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || !value.TryGetInt64(out var result))
            throw new UnauthorizedAccessException($"Wompi no envio el campo {name}.");
        return result;
    }

    private static string ComputeSha256Hex(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static bool FixedTimeHexEquals(string expected, string actual)
    {
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(expected),
                Convert.FromHexString(actual));
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
