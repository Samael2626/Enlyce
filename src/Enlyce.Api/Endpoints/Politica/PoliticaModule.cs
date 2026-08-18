using Enlyce.Application.Queries.Politica;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Politica;

public static class PoliticaModule
{
    public static void MapPolitica(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/politica").WithTags("PoliticaTratamiento");

        group.MapGet("/activa", async (
            ObtenerPoliticaActivaHandler handler) =>
        {
            var result = await handler.HandleAsync(new ObtenerPoliticaActivaQuery());
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetPoliticaActiva")
        .Produces<ObtenerPoliticaActivaResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
