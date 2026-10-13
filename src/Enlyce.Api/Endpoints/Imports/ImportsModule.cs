using Enlyce.Application.Abstractions;
using Enlyce.Application.Imports;

namespace Enlyce.Api.Endpoints.Imports;

public sealed record ImportStatusResponse(ImportJobStatus Trabajo, ImportedListingsCounts EnBase);

public static class ImportsModule
{
    public static void MapImports(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/import/wasi")
            .WithTags("Importaciones")
            .RequireAuthorization("Administrador");

        // dryRun=true por defecto y sincrono: no baja fotos, responde en segundos.
        // dryRun=false corre en segundo plano (202) y se sigue con GET /status.
        // publish=true publica en la web las que cumplan las reglas; sin alertas de busqueda.
        group.MapPost("/", async (
            HttpContext http,
            ICommandHandler<ImportExternalListingsCommand, ImportExternalListingsResponse> handler,
            ImportJobRunner runner,
            CancellationToken ct,
            bool dryRun = true,
            bool publish = false) =>
        {
            var actorId = EndpointAccess.ActorId(http.User);
            if (actorId is null)
                return Results.Forbid();

            if (dryRun)
                return Results.Ok(await handler.HandleAsync(
                    new ImportExternalListingsCommand(true, actorId.Value, publish), ct));

            return runner.TryStart(actorId.Value, publish, out var status)
                ? Results.Accepted("/api/import/wasi/status", status)
                : Results.Conflict(status);
        })
        .WithName("ImportWasiListings")
        .Produces<ImportExternalListingsResponse>()
        .Produces<ImportJobStatus>(StatusCodes.Status202Accepted)
        .Produces<ImportJobStatus>(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/status", async (
            ImportJobRunner runner,
            IImportedListingsReadRepository imported,
            CancellationToken ct) =>
            Results.Ok(new ImportStatusResponse(runner.Status, await imported.CountAsync("wasi", ct))))
        .WithName("GetWasiImportStatus")
        .Produces<ImportStatusResponse>();
    }
}
