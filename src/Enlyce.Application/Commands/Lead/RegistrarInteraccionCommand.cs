namespace Enlyce.Application.Commands.Lead;

public sealed record RegistrarInteraccionCommand(
    Guid LeadId, Guid AsesorId, string Tipo, string? Resumen);
