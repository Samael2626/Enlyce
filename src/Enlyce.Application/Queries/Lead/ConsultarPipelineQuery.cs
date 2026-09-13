using Enlyce.Domain.Entities;

namespace Enlyce.Application.Queries.Lead;

public sealed record ConsultarPipelineQuery(string? Etapa = null, Guid? AdvisorId = null);

public sealed record PipelineResponse(
    List<EtapaPipelineDto> Etapas,
    List<LeadDto> Leads,
    int Total);

public sealed record EtapaPipelineDto(string Nombre, string Etiqueta, int LeadCount);

public sealed record LeadDto(
    Guid Id, string Nombre, string Email, string? Telefono,
    string Fuente, string EtapaPipeline, string TipoOperacion,
    int InteraccionesCount, DateTime? FechaUltimaInteraccion,
    DateTime FechaCreacion, string? AsesorNombre);
