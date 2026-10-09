namespace Enlyce.Application.Commands.Lead;

public sealed record MoverEtapaCommand(Guid LeadId, string NuevaEtapa, Guid ActorId, string Reason);
