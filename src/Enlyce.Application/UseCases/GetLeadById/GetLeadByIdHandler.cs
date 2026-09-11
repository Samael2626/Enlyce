using Enlyce.Application.Abstractions;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.UseCases.GetLeadById;

public class GetLeadByIdHandler : IQueryHandler<GetLeadByIdQuery, GetLeadByIdResponse?>
{
    private readonly ILeadRepository _leadRepo;

    public GetLeadByIdHandler(ILeadRepository leadRepo)
    {
        _leadRepo = leadRepo;
    }

    public async Task<GetLeadByIdResponse?> HandleAsync(GetLeadByIdQuery query, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(query.Id);
        if (lead is null) return null;

        return new GetLeadByIdResponse(
            lead.Id,
            lead.Nombre,
            lead.Email.Value,
            lead.Telefono?.Value,
            lead.Fuente,
            lead.Estado.ToString(),
            lead.MotivoCierre.ToString(),
            lead.FechaCreacion,
            lead.FechaUltimoContacto,
            lead.AutorizacionDatos,
            lead.Activo,
            lead.EtapaPipeline,
            lead.TipoOperacion,
            lead.OwnerService?.ToString(),
            lead.PublicationId);
    }
}
