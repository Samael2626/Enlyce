using System.Text.Json;
using Enlyce.Domain.Entities;
using Enlyce.Infrastructure.Billing;
using Microsoft.Extensions.Options;

namespace Enlyce.IntegrationTests.Billing;

public sealed class WompiPaymentGatewayTests
{
    [Fact]
    public void VerifyAndParse_SignedExample_ReturnsNotification()
    {
        var gateway = CreateGateway("prod", "events-secret-for-tests-only");
        using var document = JsonDocument.Parse(OfficialEvent);

        var result = gateway.VerifyAndParse(document.RootElement);

        Assert.Equal("1234-1610641025-49201", result.ProviderTransactionId);
        Assert.Equal("MZQ3X2DE2SMX", result.Reference);
        Assert.Equal(4_490_000, result.AmountInCents);
        Assert.Equal(PaymentOrderStatus.Approved, result.Status);
    }

    [Fact]
    public void VerifyAndParse_TamperedEvent_RejectsSignature()
    {
        var gateway = CreateGateway("prod", "events-secret-for-tests-only");
        using var document = JsonDocument.Parse(OfficialEvent.Replace("4490000", "4490001", StringComparison.Ordinal));

        Assert.Throws<UnauthorizedAccessException>(() => gateway.VerifyAndParse(document.RootElement));
    }

    private static WompiPaymentGateway CreateGateway(string environment, string eventsSecret) => new(
        Options.Create(new WompiOptions
        {
            Environment = environment,
            EventsSecret = eventsSecret
        }));

    private const string OfficialEvent = """
        {
          "event": "transaction.updated",
          "data": {
            "transaction": {
              "id": "1234-1610641025-49201",
              "amount_in_cents": 4490000,
              "reference": "MZQ3X2DE2SMX",
              "currency": "COP",
              "status": "APPROVED"
            }
          },
          "environment": "prod",
          "signature": {
            "properties": ["transaction.id", "transaction.status", "transaction.amount_in_cents"],
            "checksum": "92EBBD45B05B2D29DAF3A790E23666106A07A1040CD77773720BDA89C6B6D6B4"
          },
          "timestamp": 1530291411
        }
        """;
}
