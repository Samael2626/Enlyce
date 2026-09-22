namespace Enlyce.Application.UseCases.CreateLead;

public record CreateLeadResponse(
    Guid Id,
    string Nombre,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    string? OwnerService,
    Guid? PublicationId,
    // True cuando el correo ya existia y la consulta se sumo como interaccion
    // sobre el lead existente en vez de crear uno nuevo.
    bool EsContactoRepetido = false);
