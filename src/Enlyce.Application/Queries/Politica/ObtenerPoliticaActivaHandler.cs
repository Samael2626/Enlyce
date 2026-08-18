using Enlyce.Domain.Ports;

namespace Enlyce.Application.Queries.Politica;

public sealed class ObtenerPoliticaActivaHandler
{
    private readonly IPoliticaTratamientoRepository _politicaRepo;

    public ObtenerPoliticaActivaHandler(IPoliticaTratamientoRepository politicaRepo)
    {
        _politicaRepo = politicaRepo;
    }

    public async Task<ObtenerPoliticaActivaResponse?> HandleAsync(
        ObtenerPoliticaActivaQuery query, CancellationToken ct = default)
    {
        var politica = await _politicaRepo.ObtenerActivaAsync();
        if (politica is null) return null;

        return new ObtenerPoliticaActivaResponse(
            politica.Id, politica.Version, politica.TextoCompleto, politica.FechaVigencia);
    }
}

public sealed record ObtenerPoliticaActivaResponse(
    Guid Id, string Version, string TextoCompleto, DateTime FechaVigencia);
