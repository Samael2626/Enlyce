using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.CustomerFunnels;

public static class CustomerFunnelsModule
{
    public static void MapCustomerFunnels(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/configuracion/embudos-clientes")
            .WithTags("Embudos de clientes").RequireAuthorization();

        group.MapGet("/etapas", async (ICustomerFunnelStageRepository stages, CancellationToken ct) =>
        {
            var all = await stages.GetAllAsync(ct);
            return Results.Ok(new CustomerFunnelStagesResponse(
                all.Select(stage => new CustomerFunnelStageResponse(stage.Id, stage.FunnelName,
                    stage.Name, stage.SortOrder, stage.IsDefaultForInbound)).ToArray()));
        }).WithName("GetCustomerFunnelStages");

        group.MapPost("/etapas", async Task<IResult> (
            [FromBody] CreateCustomerFunnelStageRequest request,
            ICustomerCatalogRepository catalogs,
            ICustomerFunnelStageRepository stages,
            CancellationToken ct) =>
        {
            var funnelName = request.FunnelName?.Trim() ?? string.Empty;
            var existingStages = await stages.GetAllAsync(ct);
            var knownFunnel = string.Equals(funnelName, "Clientes", StringComparison.OrdinalIgnoreCase) ||
                (await catalogs.GetAllAsync(ct)).Any(item => item.Catalog == "CustomerFunnel" &&
                    string.Equals(item.Name, funnelName, StringComparison.OrdinalIgnoreCase)) ||
                existingStages.Any(stage => string.Equals(stage.FunnelName, funnelName, StringComparison.OrdinalIgnoreCase));
            if (!knownFunnel)
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["funnelName"] = ["El embudo no existe."] });

            try
            {
                var stage = CustomerFunnelStage.Create(funnelName, request.Name, request.SortOrder ?? 0);
                if (!await stages.AddAsync(stage, ct))
                    return Results.Conflict(new { error = "La etapa ya existe en ese embudo." });
                return Results.Created("/api/configuracion/embudos-clientes/etapas",
                    new CustomerFunnelStageResponse(stage.Id, stage.FunnelName, stage.Name,
                        stage.SortOrder, stage.IsDefaultForInbound));
            }
            catch (DomainError error)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = [error.Message] });
            }
        }).RequireAuthorization("Administrador").WithName("CreateCustomerFunnelStage");

        group.MapPut("/etapas/{id:guid}/predeterminada", async Task<IResult> (
            Guid id, ICustomerFunnelStageRepository stages, CancellationToken ct) =>
        {
            if (!await stages.SetDefaultForInboundAsync(id, ct))
                return Results.NotFound();
            return Results.NoContent();
        }).RequireAuthorization("Administrador").WithName("SetDefaultCustomerFunnelStage");
    }
}

public sealed record CreateCustomerFunnelStageRequest(string FunnelName, string Name, int? SortOrder = null);
public sealed record CustomerFunnelStageResponse(Guid Id, string FunnelName, string Name,
    int SortOrder, bool IsDefaultForInbound);
public sealed record CustomerFunnelStagesResponse(IReadOnlyList<CustomerFunnelStageResponse> Stages);
