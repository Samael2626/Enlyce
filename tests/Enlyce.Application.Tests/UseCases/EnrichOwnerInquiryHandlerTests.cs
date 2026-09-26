using Enlyce.Application.UseCases.EnrichOwnerInquiry;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using NSubstitute;

namespace Enlyce.Application.Tests.UseCases;

public sealed class EnrichOwnerInquiryHandlerTests
{
    private readonly ILeadRepository _leads = Substitute.For<ILeadRepository>();

    [Fact]
    public async Task Handle_WithMatchingEmail_EnrichesTheExistingOpportunity()
    {
        var lead = CreateOwnerLead();
        _leads.GetByIdAsync(lead.Id).Returns(lead);
        _leads.SaveAsync(lead).Returns(lead);

        var result = await new EnrichOwnerInquiryHandler(_leads).HandleAsync(new(
            lead.Id,
            "owner@test.com",
            "Apartment",
            "Medellin",
            "Laureles",
            650_000_000m,
            "Piso alto",
            "WhatsApp"));

        Assert.NotNull(result);
        Assert.Equal("Apartment", result!.PropertyType);
        Assert.Equal("Medellin", result.City);
        await _leads.Received(1).SaveAsync(lead);
    }

    [Fact]
    public async Task Handle_WithDifferentEmail_DoesNotRevealOrModifyTheOpportunity()
    {
        var lead = CreateOwnerLead();
        _leads.GetByIdAsync(lead.Id).Returns(lead);

        var result = await new EnrichOwnerInquiryHandler(_leads).HandleAsync(new(
            lead.Id,
            "attacker@test.com",
            "House",
            "Medellin",
            null,
            null,
            null,
            "Email"));

        Assert.Null(result);
        await _leads.DidNotReceiveWithAnyArgs().SaveAsync(default!);
    }

    private static Lead CreateOwnerLead() => Lead.Crear(
        "Owner",
        Email.Create("owner@test.com"),
        null,
        "PropietarioWeb:Vender",
        true,
        "Venta",
        OwnerInquiryService.Sell);
}
