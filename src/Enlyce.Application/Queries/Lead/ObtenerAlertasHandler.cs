using Enlyce.Domain.Ports;

namespace Enlyce.Application.Queries.Lead;

public sealed class ObtenerAlertasHandler
{
    private readonly ILeadRepository _leadRepo;

    public ObtenerAlertasHandler(ILeadRepository leadRepo) => _leadRepo = leadRepo;

    public async Task<AlertasResponse> HandleAsync(
        ObtenerAlertasQuery query, CancellationToken ct = default)
    {
        var leads = query.AdvisorId is Guid advisorId
            ? await _leadRepo.GetByAsesorIdAsync(advisorId)
            : await _leadRepo.GetAllAsync();
        var ahora = DateTime.UtcNow;
        var umbral = ahora.AddDays(-query.DiasUmbral);

        var alertas = leads
            .Where(l => l.FechaUltimaInteraccion == null ||
                        l.FechaUltimaInteraccion < umbral)
            .Select(l => new AlertaLeadDto(
                l.Id, l.Nombre, l.Email.Value,
                l.EtapaPipeline,
                l.FechaUltimaInteraccion ?? l.FechaCreacion,
                (int)(ahora - (l.FechaUltimaInteraccion ?? l.FechaCreacion)).TotalDays))
            .OrderBy(a => a.DiasSinActividad)
            .ToList();

        return new AlertasResponse(alertas, alertas.Count);
    }
}
