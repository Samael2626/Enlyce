using Enlyce.Application.Abstractions;
using Enlyce.Application.Imports;

namespace Enlyce.Api.Endpoints.Imports;

public static class ImportsModule
{
    public static void MapImports(this IEndpointRouteBuilder app)
    {
        // dryRun=true por defecto: escribir en el CRM debe pedirse explicitamente.
        // publish=true publica en la web las que cumplan las reglas; sin alertas de busqueda.
        app.MapPost("/api/import/wasi", async (
            HttpContext http,
            ICommandHandler<ImportExternalListingsCommand, ImportExternalListingsResponse> handler,
            CancellationToken ct,
            bool dryRun = true,
            bool publish = false) =>
        {
            var actorId = EndpointAccess.ActorId(http.User);
            if (actorId is null)
                return Results.Forbid();

            var result = await handler.HandleAsync(new ImportExternalListingsCommand(dryRun, actorId.Value, publish), ct);
            return Results.Ok(result);
        })
        .WithTags("Importaciones")
        .WithName("ImportWasiListings")
        .RequireAuthorization("Administrador")
        .Produces<ImportExternalListingsResponse>()
        .ProducesProblem(StatusCodes.Status502BadGateway);
    }
}
