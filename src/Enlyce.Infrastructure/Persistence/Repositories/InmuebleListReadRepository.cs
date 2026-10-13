using Enlyce.Application.UseCases.ListInmuebles;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class InmuebleListReadRepository(EnlyceDbContext context) : IInmuebleListReadRepository
{
    public async Task<ListInmueblesResponse> ListAsync(ListInmueblesQuery query, CancellationToken ct)
    {
        var items = context.Inmuebles.AsNoTracking().Where(x => x.Activo);
        if (!string.IsNullOrWhiteSpace(query.Search))
            items = items.Where(x => x.Nombre.Contains(query.Search) || x.Direccion.Ciudad.Contains(query.Search));
        if (!string.IsNullOrWhiteSpace(query.Modalidad) && Enum.TryParse<Enlyce.Domain.Entities.ModalidadInmueble>(query.Modalidad, true, out var modalidad))
            items = items.Where(x => x.Modalidad == modalidad);
        if (!string.IsNullOrWhiteSpace(query.Estado) && Enum.TryParse<Enlyce.Domain.Entities.EstadoInmueble>(query.Estado, true, out var estado))
            items = items.Where(x => x.Estado == estado);

        var total = await items.CountAsync(ct);
        var rows = await items.OrderByDescending(x => x.FechaCreacion)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new InmuebleListItem(x.Id, x.Nombre, x.Tipo.ToString(), x.Modalidad.ToString(), x.Estado.ToString(),
                context.PropertyPublications.Any(p => p.PropertyId == x.Id)))
            .ToListAsync(ct);
        return new ListInmueblesResponse(rows, query.Page, query.PageSize, total);
    }
}
