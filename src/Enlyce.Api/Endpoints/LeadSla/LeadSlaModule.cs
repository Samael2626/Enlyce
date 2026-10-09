using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.LeadSla;

public static class LeadSlaModule
{
    public static void MapLeadSla(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/configuracion/sla-leads")
            .WithTags("Configuración")
            .RequireAuthorization("Administrador");

        group.MapGet("/", async (ILeadSlaRuleRepository rules, CancellationToken ct) =>
        {
            var items = await rules.GetAllAsync(ct);
            return Results.Ok(new LeadSlaRulesResponse(items.Select(ToDto).ToArray()));
        })
        .WithName("GetLeadSlaRules")
        .Produces<LeadSlaRulesResponse>();

        group.MapPut("/", async Task<IResult> (
            [FromBody] LeadSlaRuleRequest request,
            ILeadSlaRuleRepository rules,
            CancellationToken ct) =>
        {
            try
            {
                var rule = LeadSlaRule.Create(request.SourceKey, request.OperationType,
                    request.FirstResponseHours, request.InactivityDays, request.Enabled);
                return Results.Ok(ToDto(await rules.UpsertAsync(rule, ct)));
            }
            catch (DomainError error)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["rule"] = [error.Message]
                });
            }
        })
        .WithName("UpsertLeadSlaRule")
        .Produces<LeadSlaRuleDto>()
        .ProducesValidationProblem();
    }

    private static LeadSlaRuleDto ToDto(LeadSlaRule rule) => new(
        rule.SourceKey, rule.OperationType, rule.FirstResponseHours,
        rule.InactivityDays, rule.Enabled, rule.UpdatedAtUtc);
}

public sealed record LeadSlaRuleRequest(
    string SourceKey, string OperationType, int FirstResponseHours,
    int? InactivityDays, bool Enabled);
public sealed record LeadSlaRuleDto(
    string SourceKey, string OperationType, int FirstResponseHours,
    int? InactivityDays, bool Enabled, DateTime UpdatedAtUtc);
public sealed record LeadSlaRulesResponse(IReadOnlyList<LeadSlaRuleDto> Rules);
