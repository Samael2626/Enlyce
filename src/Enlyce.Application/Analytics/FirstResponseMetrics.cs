using Enlyce.Domain.Ports;
using Enlyce.Domain.Entities;

namespace Enlyce.Application.Analytics;

public sealed record GetFirstResponseMetricsQuery(DateOnly From, DateOnly To);

public sealed record FirstResponseMetric(string Name, int RespondedLeads, double AverageHours);

public sealed record FirstResponseMetricsResponse(
    DateOnly From,
    DateOnly To,
    int RespondedLeads,
    double? AverageHours,
    IReadOnlyList<FirstResponseMetric> ByAdvisor,
    IReadOnlyList<FirstResponseMetric> BySource);

public sealed class GetFirstResponseMetricsHandler(
    ILeadRepository leads,
    IAsesorRepository advisors,
    IInteraccionRepository interactions)
{
    public async Task<FirstResponseMetricsResponse> HandleAsync(
        GetFirstResponseMetricsQuery query,
        CancellationToken ct = default)
    {
        if (query.To < query.From)
            throw new ArgumentException("La fecha final debe ser igual o posterior a la inicial.");

        var from = query.From.ToDateTime(TimeOnly.MinValue);
        var until = query.To.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var periodLeads = (await leads.GetAllAsync())
            .Where(lead => lead.FechaCreacion >= from && lead.FechaCreacion < until)
            .ToList();
        var responded = periodLeads
            .Where(lead => lead.FechaPrimerContacto.HasValue)
            .Select(lead => new ResponseSample(
                lead,
                (lead.FechaPrimerContacto!.Value - lead.FechaCreacion).TotalHours))
            .ToList();
        var firstInteractions = (await interactions.ObtenerHastaPrimerContactoAsync(
                responded.Select(item => item.Lead.Id).ToArray(), ct))
            .GroupBy(interaction => interaction.LeadId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(interaction => interaction.Fecha)
                    .ThenBy(interaction => interaction.Id)
                    .First().AsesorId);
        var advisorNames = (await advisors.ObtenerTodosAsync())
            .ToDictionary(advisor => advisor.Id, advisor => advisor.Nombre);

        return new FirstResponseMetricsResponse(
            query.From,
            query.To,
            responded.Count,
            responded.Count == 0 ? null : responded.Average(item => item.Hours),
            Aggregate(responded, item => firstInteractions.TryGetValue(item.Lead.Id, out var id) && advisorNames.TryGetValue(id, out var name)
                ? name
                : "Sin dato de asesor"),
            Aggregate(responded, item => string.IsNullOrWhiteSpace(item.Lead.Fuente)
                ? "Sin origen"
                : item.Lead.Fuente.Trim()));
    }

    private static IReadOnlyList<FirstResponseMetric> Aggregate(
        IEnumerable<ResponseSample> items,
        Func<ResponseSample, string> keySelector) =>
        items.GroupBy(keySelector)
            .Select(group => new FirstResponseMetric(group.Key, group.Count(), group.Average(item => item.Hours)))
            .OrderByDescending(metric => metric.RespondedLeads)
            .ThenBy(metric => metric.Name)
            .ToList();

    private sealed record ResponseSample(Lead Lead, double Hours);
}
