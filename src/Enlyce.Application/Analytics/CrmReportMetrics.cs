using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Analytics;

public sealed record GetCrmReportMetricsQuery(DateOnly From, DateOnly To);

public sealed record CrmReportMetric(
    string Name,
    int Leads,
    int RespondedLeads,
    double? AverageResponseHours,
    int Visits,
    int WonLeads,
    int LostLeads);

public sealed record CrmReportMetricsResponse(
    DateOnly From,
    DateOnly To,
    int Leads,
    int RespondedLeads,
    int Visits,
    int WonLeads,
    int LostLeads,
    IReadOnlyList<CrmReportMetric> ByAdvisor,
    IReadOnlyList<CrmReportMetric> ByStage,
    IReadOnlyList<CrmReportMetric> BySource,
    IReadOnlyList<CrmReportMetric> ByCampaign,
    IReadOnlyList<CrmReportMetric> ByOperation);

public sealed class GetCrmReportMetricsHandler(
    ILeadRepository leads,
    IAsesorRepository advisors,
    IVisitaRepository visits)
{
    public async Task<CrmReportMetricsResponse> HandleAsync(
        GetCrmReportMetricsQuery query,
        CancellationToken ct = default)
    {
        if (query.To < query.From)
            throw new ArgumentException("La fecha final debe ser igual o posterior a la inicial.");

        var from = query.From.ToDateTime(TimeOnly.MinValue);
        var through = query.To.ToDateTime(TimeOnly.MaxValue);
        var cohort = (await leads.GetAllAsync())
            .Where(lead => lead.FechaCreacion >= from && lead.FechaCreacion <= through)
            .ToList();
        var leadIds = cohort.Select(lead => lead.Id).ToHashSet();
        var periodVisits = (await visits.ObtenerVisitasAsync(null))
            .Where(visit => leadIds.Contains(visit.LeadId)
                && visit.FechaProgramada >= from && visit.FechaProgramada <= through)
            .ToList();
        var visitCounts = periodVisits.GroupBy(visit => visit.LeadId)
            .ToDictionary(group => group.Key, group => group.Count());
        var advisorNames = (await advisors.ObtenerTodosAsync())
            .ToDictionary(advisor => advisor.Id, advisor => advisor.Nombre);
        var samples = cohort.Select(lead => new ReportSample(
            lead,
            lead.FechaPrimerContacto.HasValue
                ? (lead.FechaPrimerContacto.Value - lead.FechaCreacion).TotalHours
                : null,
            visitCounts.GetValueOrDefault(lead.Id),
            lead.AsesorAsignadoId is Guid advisorId && advisorNames.TryGetValue(advisorId, out var name)
                ? name
                : "Sin asesor"))
            .ToList();

        return new CrmReportMetricsResponse(
            query.From,
            query.To,
            samples.Count,
            samples.Count(sample => sample.ResponseHours.HasValue),
            samples.Sum(sample => sample.Visits),
            samples.Count(sample => sample.Lead.Estado == EstadoLead.CerradoGanado),
            samples.Count(sample => sample.Lead.Estado == EstadoLead.CerradoPerdido),
            Aggregate(samples, sample => sample.Advisor),
            Aggregate(samples, sample => sample.Lead.EtapaPipeline),
            Aggregate(samples, sample => ParseSource(sample.Lead.Fuente)),
            Aggregate(samples, sample => ParseCampaign(sample.Lead.Fuente)),
            Aggregate(samples, sample => string.IsNullOrWhiteSpace(sample.Lead.TipoOperacion) ? "Sin operación" : sample.Lead.TipoOperacion.Trim()));
    }

    private static string ParseSource(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Sin origen";

        var marker = value.IndexOf("|utm=", StringComparison.Ordinal);
        var source = (marker < 0 ? value : value[..marker]).Trim();
        return string.IsNullOrWhiteSpace(source) ? "Sin origen" : source;
    }

    private static string ParseCampaign(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Sin campaña";

        var marker = value.IndexOf("|utm=", StringComparison.Ordinal);
        if (marker < 0)
            return "Sin campaña";

        var utmParts = value[(marker + "|utm=".Length)..].Split('/');
        return utmParts.Length > 2 && !string.IsNullOrWhiteSpace(utmParts[2])
            ? utmParts[2].Trim()
            : "Sin campaña";
    }

    private static IReadOnlyList<CrmReportMetric> Aggregate(
        IEnumerable<ReportSample> samples,
        Func<ReportSample, string> keySelector) =>
        samples.GroupBy(keySelector)
            .Select(CreateMetric)
            .OrderByDescending(metric => metric.Leads)
            .ThenBy(metric => metric.Name)
            .ToList();

    private static CrmReportMetric CreateMetric(IGrouping<string, ReportSample> group)
    {
        var responseHours = group
            .Where(sample => sample.ResponseHours.HasValue)
            .Select(sample => sample.ResponseHours!.Value)
            .ToArray();
        return new CrmReportMetric(
            group.Key,
            group.Count(),
            responseHours.Length,
            responseHours.Length == 0 ? null : responseHours.Average(),
            group.Sum(sample => sample.Visits),
            group.Count(sample => sample.Lead.Estado == EstadoLead.CerradoGanado),
            group.Count(sample => sample.Lead.Estado == EstadoLead.CerradoPerdido));
    }

    private sealed record ReportSample(Lead Lead, double? ResponseHours, int Visits, string Advisor);
}
