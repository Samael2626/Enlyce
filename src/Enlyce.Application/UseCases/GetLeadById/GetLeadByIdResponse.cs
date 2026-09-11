namespace Enlyce.Application.UseCases.GetLeadById;

public record GetLeadByIdResponse(
    Guid Id,
    string Nombre,
    string Email,
    string? Telefono,
    string Fuente,
    string Estado,
    string MotivoCierre,
    DateTime FechaCreacion,
    DateTime? FechaUltimoContacto,
    bool AutorizacionDatos,
    bool Activo,
    string EtapaPipeline,
    string TipoOperacion,
    string? OwnerService,
    Guid? PublicationId);
