using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Consentimiento;

public sealed class RegistrarConsentimientoHandler
{
    private readonly IConsentimientoRepository _consentimientoRepo;
    private readonly IPoliticaTratamientoRepository _politicaRepo;

    public RegistrarConsentimientoHandler(
        IConsentimientoRepository consentimientoRepo,
        IPoliticaTratamientoRepository politicaRepo)
    {
        _consentimientoRepo = consentimientoRepo;
        _politicaRepo = politicaRepo;
    }

    public async Task<RegistrarConsentimientoResponse> HandleAsync(
        RegistrarConsentimientoCommand command, CancellationToken ct = default)
    {
        var politica = await _politicaRepo.ObtenerActivaAsync()
            ?? throw new InvalidOperationException("No hay politica de tratamiento activa.");

        var consentimiento = Domain.Entities.Consentimiento.Registrar(
            command.LeadId,
            politica.TextoCompleto,
            politica.Version,
            command.Metodo,
            command.DireccionIp);

        await _consentimientoRepo.AgregarAsync(consentimiento);

        return new RegistrarConsentimientoResponse(
            consentimiento.Id, consentimiento.LeadId, consentimiento.VersionPolitica);
    }
}

public sealed record RegistrarConsentimientoResponse(
    Guid Id, Guid LeadId, string VersionPolitica);
