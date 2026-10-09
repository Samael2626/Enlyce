using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Queries.Lead;

public sealed class ConsultarPipelineHandler
{
    private readonly ILeadRepository _leadRepo;

    public ConsultarPipelineHandler(ILeadRepository leadRepo) => _leadRepo = leadRepo;

    public async Task<PipelineResponse> HandleAsync(
        ConsultarPipelineQuery query, CancellationToken ct = default)
    {
        var result = await _leadRepo.SearchPipelineAsync(
            query.AdvisorId,
            query.Etapa,
            query.Search,
            query.Operation,
            query.AssignedAdvisorId,
            query.CreatedFrom,
            query.CreatedTo,
            query.Page,
            query.PageSize,
            ct);

        var etapas = new List<EtapaPipelineDto>();
        foreach (var etapa in EtapasPipeline.Venta.Concat(EtapasPipeline.Arriendo).Distinct(StringComparer.Ordinal))
        {
            var count = result.StageCounts.GetValueOrDefault(etapa);
            etapas.Add(new EtapaPipelineDto(etapa, EtapasPipeline.Etiquetas[etapa], count));
        }

        var leadDtos = result.Items.Select(l => new LeadDto(
            l.Id, l.Nombre, l.Email.Value,
            l.Telefono?.Value, l.Fuente,
            l.EtapaPipeline, l.TipoOperacion,
            l.InteraccionesCount, l.FechaUltimaInteraccion,
            l.FechaCreacion, null, l.FechaPrimerContacto)).ToList();

        return new PipelineResponse(etapas, leadDtos, result.Total, query.Page, query.PageSize);
    }
}
