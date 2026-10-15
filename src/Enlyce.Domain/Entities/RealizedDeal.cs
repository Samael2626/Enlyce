using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public enum RealizedDealOperation
{
    Sale,
    Rent
}

public sealed class RealizedDeal
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid ContactId { get; private set; }
    public Guid? LeadId { get; private set; }
    public Guid AdvisorId { get; private set; }
    public RealizedDealOperation Operation { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal CommissionAmount { get; private set; }
    public string Currency { get; private set; } = "COP";
    public DateOnly ClosedOn { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    private RealizedDeal() { }

    public static RealizedDeal Create(
        Guid propertyId,
        Guid contactId,
        Guid? leadId,
        Guid advisorId,
        RealizedDealOperation operation,
        decimal grossAmount,
        decimal commissionAmount,
        string currency,
        DateOnly closedOn,
        Guid recordedByUserId,
        DateTime recordedAtUtc)
    {
        if (propertyId == Guid.Empty || contactId == Guid.Empty || advisorId == Guid.Empty || recordedByUserId == Guid.Empty)
            throw new DomainError("Inmueble, cliente, asesor y autor del registro son obligatorios.");
        if (leadId == Guid.Empty)
            throw new DomainError("El identificador de la oportunidad no es valido.");
        if (!Enum.IsDefined(operation))
            throw new DomainError("El tipo de negocio no es valido.");
        if (grossAmount <= 0 || commissionAmount < 0)
            throw new DomainError("El valor bruto debe ser mayor a cero y la comision no puede ser negativa.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3 || currency.Any(character => !char.IsAsciiLetter(character)))
            throw new DomainError("La moneda debe ser un codigo de tres letras.");
        if (closedOn == default)
            throw new DomainError("La fecha de cierre es obligatoria.");

        return new RealizedDeal
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            ContactId = contactId,
            LeadId = leadId,
            AdvisorId = advisorId,
            Operation = operation,
            GrossAmount = decimal.Round(grossAmount, 2),
            CommissionAmount = decimal.Round(commissionAmount, 2),
            Currency = currency.Trim().ToUpperInvariant(),
            ClosedOn = closedOn,
            RecordedByUserId = recordedByUserId,
            RecordedAtUtc = DateTime.SpecifyKind(recordedAtUtc, DateTimeKind.Utc)
        };
    }
}
