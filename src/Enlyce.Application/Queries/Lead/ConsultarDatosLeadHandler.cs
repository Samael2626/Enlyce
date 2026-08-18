using Enlyce.Domain.Ports;

namespace Enlyce.Application.Queries.Lead;

public sealed class ConsultarDatosLeadHandler
{
    private readonly ILeadRepository _leadRepo;
    private readonly IConsentimientoRepository _consentimientoRepo;

    public ConsultarDatosLeadHandler(
        ILeadRepository leadRepo,
        IConsentimientoRepository consentimientoRepo)
    {
        _leadRepo = leadRepo;
        _consentimientoRepo = consentimientoRepo;
    }

    public async Task<ConsultarDatosLeadResponse?> HandleAsync(
        ConsultarDatosLeadQuery query, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(query.LeadId);
        if (lead is null) return null;

        var consentimiento = await _consentimientoRepo.ObtenerPorLeadAsync(query.LeadId);

        return new ConsultarDatosLeadResponse(
            lead.Id,
            lead.Nombre,
            lead.Email,
            lead.Telefono,
            lead.AutorizacionDatos,
            consentimiento is not null
                ? new ConsentimientoInfo(
                    consentimiento.Id,
                    consentimiento.Fecha,
                    consentimiento.VersionPolitica,
                    consentimiento.Metodo)
                : null);
    }
}

public sealed record ConsultarDatosLeadResponse(
    Guid Id,
    string Nombre,
    Enlyce.Domain.ValueObjects.Email Email,
    Enlyce.Domain.ValueObjects.Telefono Telefono,
    bool AutorizacionDatos,
    ConsentimientoInfo? Consentimiento);

public sealed record ConsentimientoInfo(
    Guid Id, DateTime Fecha, string VersionPolitica, string Metodo);
