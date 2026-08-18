namespace Enlyce.Application.Commands.Lead;

public sealed record ReasignarLeadCommand(Guid LeadId, Guid NuevoAsesorId);
