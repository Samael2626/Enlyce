namespace Enlyce.Application.UseCases.CreateLead;

public record CreateLeadCommand(
    string Nombre,
    string Email,
    string? Telefono,
    string? Fuente,
    bool AutorizacionDatos,
    string TipoOperacion = "Venta",
    string? OwnerService = null,
    string? PublicationId = null);
