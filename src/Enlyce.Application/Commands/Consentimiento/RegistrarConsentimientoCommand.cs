namespace Enlyce.Application.Commands.Consentimiento;

public sealed record RegistrarConsentimientoCommand(
    Guid LeadId,
    string Metodo,
    string? DireccionIp = null);
