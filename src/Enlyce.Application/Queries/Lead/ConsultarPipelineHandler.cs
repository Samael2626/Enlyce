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
        IReadOnlyList<Enlyce.Domain.Entities.Lead> accessibleLeads = query.AdvisorId is Guid advisorId
            ? await _leadRepo.GetByAsesorIdAsync(advisorId)
            : await _leadRepo.GetAllAsync();
        var leads = query.Etapa is null
            ? accessibleLeads
            : accessibleLeads.Where(lead => lead.EtapaPipeline == query.Etapa).ToList();

        var etapas = new List<EtapaPipelineDto>();
        foreach (var etapa in EtapasPipeline.Venta)
        {
            var count = leads.Count(l => l.EtapaPipeline == etapa);
            etapas.Add(new EtapaPipelineDto(etapa, EtapasPipeline.Etiquetas[etapa], count));
        }

        var leadDtos = leads.Select(l => new LeadDto(
            l.Id, l.Nombre, l.Email.Value,
            l.Telefono?.Value, l.Fuente,
            l.EtapaPipeline, l.TipoOperacion,
            l.InteraccionesCount, l.FechaUltimaInteraccion,
            l.FechaCreacion, null)).ToList();

        return new PipelineResponse(etapas, leadDtos, leads.Count);
    }
}
