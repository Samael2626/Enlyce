using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.Media;

public static class MediaCleanupModule
{
    public static void MapMediaCleanup(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media/cleanup-orphans").WithTags("Media").RequireAuthorization("Administrador");

        group.MapPost("/", (bool? dryRun, IServiceProvider services, MediaCleanupJobRunner runner) =>
        {
            if (services.GetService<IOrphanMediaStorage>() is null)
                return Results.Problem("La limpieza de objetos requiere MediaStorage:Provider=s3.", statusCode: StatusCodes.Status503ServiceUnavailable);
            var isDryRun = dryRun ?? true;
            if (!runner.TryStart(isDryRun, out var job))
                return Results.Conflict(new { message = "Ya existe una limpieza en curso.", jobId = job.Id });
            return Results.Accepted($"/api/media/cleanup-orphans/{job.Id}", job);
        })
        .WithName("StartOrphanMediaCleanup")
        .Produces<MediaCleanupJobStatus>(StatusCodes.Status202Accepted);

        group.MapGet("/{jobId:guid}", (Guid jobId, MediaCleanupJobRunner runner) =>
            runner.Get(jobId) is { } status ? Results.Ok(status) : Results.NotFound())
            .WithName("GetOrphanMediaCleanupStatus");
    }
}
