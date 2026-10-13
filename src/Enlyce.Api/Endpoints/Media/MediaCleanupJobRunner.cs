using Enlyce.Application.Media;

namespace Enlyce.Api.Endpoints.Media;

public enum MediaCleanupJobState { Queued, Running, Completed, Failed }
public sealed record MediaCleanupJobStatus(Guid Id, bool DryRun, MediaCleanupJobState State, DateTimeOffset CreatedAt,
    DateTimeOffset? FinishedAt, OrphanMediaCleanupResult? Result, string? Error);

public sealed class MediaCleanupJobRunner(IServiceScopeFactory scopes, IHostApplicationLifetime lifetime, ILogger<MediaCleanupJobRunner> logger)
{
    private readonly Lock _gate = new();
    private readonly Dictionary<Guid, MediaCleanupJobStatus> _jobs = [];

    public bool TryStart(bool dryRun, out MediaCleanupJobStatus status)
    {
        lock (_gate)
        {
            if (_jobs.Values.Any(job => job.State is MediaCleanupJobState.Queued or MediaCleanupJobState.Running))
            {
                status = _jobs.Values.Last(job => job.State is MediaCleanupJobState.Queued or MediaCleanupJobState.Running);
                return false;
            }
            status = new(Guid.NewGuid(), dryRun, MediaCleanupJobState.Queued, DateTimeOffset.UtcNow, null, null, null);
            _jobs[status.Id] = status;
        }
        _ = RunAsync(status);
        return true;
    }

    public MediaCleanupJobStatus? Get(Guid id)
    {
        lock (_gate) return _jobs.GetValueOrDefault(id);
    }

    private async Task RunAsync(MediaCleanupJobStatus job)
    {
        Update(job with { State = MediaCleanupJobState.Running });
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICleanupOrphanMediaHandler>();
            // La desconexion HTTP no cancela el trabajo; solo el cierre del proceso.
            var result = await handler.ExecuteAsync(job.DryRun, lifetime.ApplicationStopping);
            logger.LogInformation("Limpieza R2 {JobId} completada ({Mode}): {ObjectCount} objetos, {TotalBytes} bytes",
                job.Id, job.DryRun ? "dry-run" : "borrado", result.ObjectCount, result.TotalBytes);
            Update(job with { State = MediaCleanupJobState.Completed, FinishedAt = DateTimeOffset.UtcNow, Result = result });
        }
        catch (Exception error)
        {
            logger.LogError(error, "Limpieza de medios fallida para el trabajo {JobId}", job.Id);
            Update(job with { State = MediaCleanupJobState.Failed, FinishedAt = DateTimeOffset.UtcNow, Error = error.Message });
        }
    }

    private void Update(MediaCleanupJobStatus status) { lock (_gate) _jobs[status.Id] = status; }
}
