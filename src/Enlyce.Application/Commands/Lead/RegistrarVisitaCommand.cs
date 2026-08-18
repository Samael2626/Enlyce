namespace Enlyce.Application.Commands.Lead;

public sealed record RegistrarVisitaCommand(
    Guid LeadId, Guid InmuebleId, Guid AsesorId, DateTime FechaProgramada);
