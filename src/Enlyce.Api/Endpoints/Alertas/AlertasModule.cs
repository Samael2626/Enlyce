using Enlyce.Application.Queries.Lead;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Alertas;

public static class AlertasModule
{
    public static void MapAlertas(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alertas").WithTags("Alertas");

        group.MapGet("/", async (
            [AsParameters] ConsultarAlertasRequest query,
            ObtenerAlertasHandler handler) =>
        {
            var result = await handler.HandleAsync(
                new ObtenerAlertasQuery(query.DiasUmbral));
            return Results.Ok(result);
        })
        .WithName("ObtenerAlertas")
        .Produces<AlertasResponse>();
    }
}

public record ConsultarAlertasRequest(int DiasUmbral = 7);
