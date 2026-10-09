using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.LeadDistribution;

public static class LeadDistributionModule
{
    public static void MapLeadDistribution(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/configuracion/reparto-leads")
            .WithTags("Configuración")
            .RequireAuthorization("Administrador");

        group.MapGet("/", async (ILeadDistributionSettingsRepository settings, CancellationToken ct) =>
        {
            var rule = await settings.GetRuleAsync(ct);
            return Results.Ok(new LeadDistributionRuleResponse(rule.ToString()));
        })
        .WithName("GetLeadDistributionRule")
        .Produces<LeadDistributionRuleResponse>();

        group.MapPut("/", async (
            LeadDistributionRuleRequest request,
            ILeadDistributionSettingsRepository settings,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<LeadDistributionRule>(request.Rule, true, out var rule) || !Enum.IsDefined(rule))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Rule)] = ["Selecciona una regla de reparto válida."]
                });

            await settings.SetRuleAsync(rule, ct);
            return Results.Ok(new LeadDistributionRuleResponse(rule.ToString()));
        })
        .WithName("SetLeadDistributionRule")
        .Produces<LeadDistributionRuleResponse>()
        .ProducesValidationProblem();
    }
}

public sealed record LeadDistributionRuleRequest(string Rule);
public sealed record LeadDistributionRuleResponse(string Rule);
