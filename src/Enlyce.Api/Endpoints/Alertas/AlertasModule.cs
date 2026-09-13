using Enlyce.Application.Queries.Lead;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Alertas;

public static class AlertasModule
{
    public static void MapAlertas(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alertas").WithTags("Alertas").RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [AsParameters] ConsultarAlertasRequest query,
            HttpContext http,
            ObtenerAlertasHandler handler) =>
        {
            var advisorId = http.User.IsInRole("Administrador")
                ? null
                : EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            var result = await handler.HandleAsync(
                new ObtenerAlertasQuery(query.DiasUmbral, advisorId));
            return Results.Ok(result);
        })
        .WithName("ObtenerAlertas")
        .Produces<AlertasResponse>();
    }
}

public record ConsultarAlertasRequest(int DiasUmbral = 7);
