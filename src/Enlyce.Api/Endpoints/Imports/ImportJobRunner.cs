using Enlyce.Application.Abstractions;
using Enlyce.Application.Imports;

namespace Enlyce.Api.Endpoints.Imports;

public enum ImportJobState
{
    Inactivo,
    EnCurso,
    Completado,
    Fallido
}

public sealed record ImportJobStatus(
    ImportJobState Estado,
    DateTimeOffset? Inicio,
    DateTimeOffset? Fin,
    bool Publish,
    ImportExternalListingsResponse? Resultado,
    string? Error);

// Corre la importacion fuera de la peticion HTTP: con cientos de fotos supera el
// timeout del proxy (300 s en Railway) y cancelar a mitad dejaba el lote a medias.
// Un solo trabajo a la vez; el estado vive en memoria y se pierde al reiniciar, lo
// cual es aceptable porque relanzar es idempotente.
public sealed class ImportJobRunner
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ImportJobRunner> _logger;
    private readonly Lock _gate = new();
    private ImportJobStatus _status = new(ImportJobState.Inactivo, null, null, false, null, null);

    public ImportJobRunner(
        IServiceScopeFactory scopes, IHostApplicationLifetime lifetime, ILogger<ImportJobRunner> logger)
    {
        _scopes = scopes;
        _lifetime = lifetime;
        _logger = logger;
    }

    public ImportJobStatus Status
    {
        get
        {
            lock (_gate)
                return _status;
        }
    }

    public bool TryStart(Guid advisorId, bool publish, out ImportJobStatus status)
    {
        lock (_gate)
        {
            if (_status.Estado == ImportJobState.EnCurso)
            {
                status = _status;
                return false;
            }

            _status = new(ImportJobState.EnCurso, DateTimeOffset.UtcNow, null, publish, null, null);
            status = _status;
        }

        _ = Task.Run(() => RunAsync(advisorId, publish));
        return true;
    }

    private async Task RunAsync(Guid advisorId, bool publish)
    {
        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<ImportExternalListingsCommand, ImportExternalListingsResponse>>();
            // Solo el apagado de la app cancela; la desconexion del cliente ya no.
            var result = await handler.HandleAsync(
                new ImportExternalListingsCommand(false, advisorId, publish), _lifetime.ApplicationStopping);

            _logger.LogInformation(
                "Importacion {Source} completada: {Creados} creados, {Actualizados} actualizados, {Omitidos} omitidos, {Publicados} publicados, {Fotos} fotos.",
                result.Source, result.Creados, result.Actualizados, result.Omitidos, result.Publicados, result.FotosImportadas);
            Finish(ImportJobState.Completado, result, null);
        }
        catch (Exception error)
        {
            // Fallo ruidoso: queda en logs como error y visible en el endpoint de estado.
            _logger.LogError(error, "Importacion Wasi fallida.");
            Finish(ImportJobState.Fallido, null, error.Message);
        }
    }

    private void Finish(ImportJobState state, ImportExternalListingsResponse? result, string? error)
    {
        lock (_gate)
            _status = _status with { Estado = state, Fin = DateTimeOffset.UtcNow, Resultado = result, Error = error };
    }
}
