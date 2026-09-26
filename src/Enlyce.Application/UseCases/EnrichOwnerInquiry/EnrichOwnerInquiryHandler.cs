using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.UseCases.EnrichOwnerInquiry;

public sealed record EnrichOwnerInquiryCommand(
    Guid LeadId,
    string Email,
    string PropertyType,
    string City,
    string? Neighborhood,
    decimal? ExpectedPrice,
    string? Message,
    string PreferredContactChannel);

public sealed record OwnerInquiryDetailsResponse(
    Guid LeadId,
    string PropertyType,
    string City,
    string? Neighborhood,
    decimal? ExpectedPrice,
    string? Message,
    string PreferredContactChannel);

public sealed class EnrichOwnerInquiryHandler(ILeadRepository leads)
    : ICommandHandler<EnrichOwnerInquiryCommand, OwnerInquiryDetailsResponse?>
{
    public async Task<OwnerInquiryDetailsResponse?> HandleAsync(
        EnrichOwnerInquiryCommand command,
        CancellationToken ct = default)
    {
        var lead = await leads.GetByIdAsync(command.LeadId);
        var email = Email.Create(command.Email);

        // Mismo resultado para id inexistente y correo distinto: el endpoint
        // publico no confirma si una oportunidad ajena existe.
        if (lead is null || lead.Email != email)
            return null;

        var propertyType = ParseEnum<OwnerPropertyType>(command.PropertyType, "tipo de inmueble");
        var preferredChannel = ParseEnum<PreferredContactChannel>(
            command.PreferredContactChannel,
            "canal de contacto");

        lead.EnrichOwnerInquiry(
            propertyType,
            command.City,
            command.Neighborhood,
            command.ExpectedPrice,
            command.Message,
            preferredChannel);

        var saved = await leads.SaveAsync(lead);
        return new OwnerInquiryDetailsResponse(
            saved.Id,
            saved.OwnerPropertyType!.Value.ToString(),
            saved.OwnerPropertyCity!,
            saved.OwnerPropertyNeighborhood,
            saved.OwnerExpectedPrice,
            saved.OwnerPropertyMessage,
            saved.OwnerPreferredContactChannel!.Value.ToString());
    }

    private static TEnum ParseEnum<TEnum>(string value, string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.TryParse<TEnum>(value, false, out var parsed) || !Enum.IsDefined(parsed))
            throw new DomainError($"Valor no valido para {fieldName}: {value}");

        return parsed;
    }
}
