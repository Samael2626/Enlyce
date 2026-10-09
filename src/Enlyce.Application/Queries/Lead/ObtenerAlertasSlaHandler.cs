using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Queries.Lead;

public sealed class ObtenerAlertasSlaHandler(
    ILeadSlaRuleRepository rules,
    ILeadSlaAlertsRepository alerts,
    TimeProvider timeProvider)
{
    public async Task<AlertasSlaResponse> HandleAsync(
        ObtenerAlertasSlaQuery query, CancellationToken ct = default)
    {
        var configuredRules = await rules.GetAllAsync(ct);
        if (configuredRules.Count == 0)
            return new AlertasSlaResponse([], 0);

        var candidates = await alerts.GetCandidatesAsync(query.AdvisorId, ct);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var results = new List<AlertaSlaLeadDto>();

        foreach (var lead in candidates)
        {
            if (lead.FechaPrimeraAsignacion is not DateTime assignedAt)
                continue;

            var sourceKey = NormalizeLeadSource(lead.Fuente);
            var rule = ResolveRule(configuredRules, sourceKey, lead.TipoOperacion);
            if (rule is not { Enabled: true })
                continue;

            DateTime startedAt;
            DateTime dueAt;
            string kind;

            if (lead.FechaPrimerContacto is null)
            {
                startedAt = assignedAt;
                dueAt = assignedAt.AddHours(rule.FirstResponseHours);
                kind = "FirstResponse";
            }
            else
            {
                if (rule.InactivityDays is not int inactivityDays)
                    continue;

                startedAt = lead.FechaUltimaInteraccion ?? lead.FechaPrimerContacto.Value;
                dueAt = startedAt.AddDays(inactivityDays);
                kind = "Inactivity";
            }

            if (now < dueAt)
                continue;

            results.Add(new AlertaSlaLeadDto(
                lead.LeadId,
                lead.Nombre,
                lead.Email,
                lead.TipoOperacion,
                sourceKey ?? string.Empty,
                lead.EtapaPipeline,
                kind,
                startedAt,
                dueAt,
                (int)Math.Floor((now - dueAt).TotalHours)));
        }

        var ordered = results
            .OrderByDescending(alert => alert.OverdueHours)
            .ThenBy(alert => alert.DueAtUtc)
            .ThenBy(alert => alert.LeadId)
            .ToArray();
        return new AlertasSlaResponse(ordered.Take(100).ToArray(), ordered.Length);
    }

    private static LeadSlaRule? ResolveRule(
        IReadOnlyList<LeadSlaRule> rules, string? sourceKey, string operationType)
    {
        if (sourceKey is not null)
        {
            var exact = rules.FirstOrDefault(rule =>
                rule.SourceKey == sourceKey && rule.OperationType == operationType);
            if (exact is not null)
                return exact;

            var sourceWildcard = rules.FirstOrDefault(rule =>
                rule.SourceKey == sourceKey && rule.OperationType == "*");
            if (sourceWildcard is not null)
                return sourceWildcard;
        }

        var operationWildcard = rules.FirstOrDefault(rule =>
            rule.SourceKey == "*" && rule.OperationType == operationType);
        if (operationWildcard is not null)
            return operationWildcard;

        return rules.FirstOrDefault(rule => rule.SourceKey == "*" && rule.OperationType == "*");
    }

    private static string? NormalizeLeadSource(string source)
    {
        try
        {
            return LeadSlaRule.NormalizeSourceKey(source);
        }
        catch (DomainError)
        {
            return null;
        }
    }
}

public sealed record ObtenerAlertasSlaQuery(Guid? AdvisorId = null);
public sealed record AlertasSlaResponse(IReadOnlyList<AlertaSlaLeadDto> Alerts, int Total);
public sealed record AlertaSlaLeadDto(
    Guid LeadId,
    string Nombre,
    string Email,
    string OperationType,
    string SourceKey,
    string Stage,
    string Kind,
    DateTime StartedAtUtc,
    DateTime DueAtUtc,
    int OverdueHours);
