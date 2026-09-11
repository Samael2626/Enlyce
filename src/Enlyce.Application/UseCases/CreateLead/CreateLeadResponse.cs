namespace Enlyce.Application.UseCases.CreateLead;

public record CreateLeadResponse(
    Guid Id,
    string Nombre,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    string? OwnerService,
    Guid? PublicationId);
