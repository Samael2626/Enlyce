using Enlyce.Domain.Entities;

namespace Enlyce.Application.Queries.Lead;

public sealed record ConsultarPipelineQuery(
    string? Etapa = null,
    Guid? AdvisorId = null,
    string? Search = null,
    string? Operation = null,
    Guid? AssignedAdvisorId = null,
    DateOnly? CreatedFrom = null,
    DateOnly? CreatedTo = null,
    int Page = 1,
    int PageSize = 50);

public sealed record PipelineResponse(
    List<EtapaPipelineDto> Etapas,
    List<LeadDto> Leads,
    int Total,
    int Page,
    int PageSize);

public sealed record EtapaPipelineDto(string Nombre, string Etiqueta, int LeadCount);

public sealed record LeadDto(
    Guid Id, string Nombre, string Email, string? Telefono,
    string Fuente, string EtapaPipeline, string TipoOperacion,
    int InteraccionesCount, DateTime? FechaUltimaInteraccion,
    DateTime FechaCreacion, string? AsesorNombre,
    DateTime? FechaPrimerContacto);
