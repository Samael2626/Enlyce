using Enlyce.Application.UseCases.ListInmuebles;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class InmuebleListReadRepository(EnlyceDbContext context) : IInmuebleListReadRepository
{
    public async Task<ListInmueblesResponse> ListAsync(ListInmueblesQuery query, CancellationToken ct)
    {
        var items = context.Inmuebles.AsNoTracking().Where(x => x.Activo != query.Papelera);
        if (!string.IsNullOrWhiteSpace(query.CodigoWasi))
        {
            var wasiRefPrefix = $"wasi:{query.CodigoWasi.Trim()}:";
            items = items.Where(x => x.ExternalRef != null && x.ExternalRef.StartsWith(wasiRefPrefix));
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            items = items.Where(x => x.Nombre.ToLower().Contains(search) || x.Direccion.Ciudad.ToLower().Contains(search) ||
                x.Direccion.Calle.ToLower().Contains(search) || (x.Direccion.Barrio != null && x.Direccion.Barrio.ToLower().Contains(search)));
        }
        if (!string.IsNullOrWhiteSpace(query.Modalidad) && Enum.TryParse<Enlyce.Domain.Entities.ModalidadInmueble>(query.Modalidad, true, out var modalidad))
            items = items.Where(x => x.Modalidad == modalidad);
        if (!string.IsNullOrWhiteSpace(query.Estado) && Enum.TryParse<Enlyce.Domain.Entities.EstadoInmueble>(query.Estado, true, out var estado))
            items = items.Where(x => x.Estado == estado);
        if (!string.IsNullOrWhiteSpace(query.Tipo) && Enum.TryParse<Enlyce.Domain.Entities.TipoInmueble>(query.Tipo, true, out var tipo))
            items = items.Where(x => x.Tipo == tipo);
        if (!string.IsNullOrWhiteSpace(query.Pais)) items = items.Where(x => x.Direccion.Pais == query.Pais.Trim());
        if (!string.IsNullOrWhiteSpace(query.Departamento)) items = items.Where(x => x.Direccion.Departamento == query.Departamento.Trim());
        if (!string.IsNullOrWhiteSpace(query.Ciudad)) items = items.Where(x => x.Direccion.Ciudad == query.Ciudad.Trim());
        if (!string.IsNullOrWhiteSpace(query.Barrio)) items = items.Where(x => x.Direccion.Barrio == query.Barrio.Trim());
        if (!string.IsNullOrWhiteSpace(query.Moneda)) items = items.Where(x => x.Precio.Moneda == query.Moneda);
        if (query.PrecioDesde.HasValue) items = items.Where(x => x.Precio.Monto >= query.PrecioDesde.Value);
        if (query.PrecioHasta.HasValue) items = items.Where(x => x.Precio.Monto <= query.PrecioHasta.Value);
        if (query.AreaDesde.HasValue) items = items.Where(x => x.MetrosCuadrados >= query.AreaDesde.Value);
        if (query.AreaHasta.HasValue) items = items.Where(x => x.MetrosCuadrados <= query.AreaHasta.Value);
        if (query.Habitaciones.HasValue) items = items.Where(x => x.Habitaciones >= query.Habitaciones.Value);
        if (query.Banos.HasValue) items = items.Where(x => x.Banos >= query.Banos.Value);
        if (query.Parqueaderos.HasValue) items = items.Where(x => x.Parqueaderos >= query.Parqueaderos.Value);
        if (query.ConstruidoDesde.HasValue) items = items.Where(x => x.AnioConstruccion >= query.ConstruidoDesde.Value);
        if (!string.IsNullOrWhiteSpace(query.Etiqueta)) items = items.Where(x => x.Etiquetas.Contains(query.Etiqueta.Trim()));
        if (query.AsesorResponsableId.HasValue)
            items = query.IncluirSinResponsable
                ? items.Where(x => x.AsesorResponsableId == query.AsesorResponsableId || x.AsesorResponsableId == null)
                : items.Where(x => x.AsesorResponsableId == query.AsesorResponsableId);

        var total = await items.CountAsync(ct);
        var ordered = query.Orden switch
        {
            "oldest" => items.OrderBy(x => x.FechaCreacion),
            "priceAsc" => items.OrderBy(x => x.Precio.Monto),
            "priceDesc" => items.OrderByDescending(x => x.Precio.Monto),
            _ => items.OrderByDescending(x => x.FechaCreacion)
        };
        var rows = await ordered.ThenBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new InmuebleListItem(x.Id, x.Nombre, x.Tipo.ToString(), x.Modalidad.ToString(), x.Estado.ToString(),
                context.PropertyPublications.Any(p => p.PropertyId == x.Id), x.Direccion.Ciudad, x.Direccion.Barrio,
                x.ExternalRef, x.Precio.Moneda, x.Precio.Monto, x.MetrosCuadrados, x.Habitaciones, x.Banos, x.Parqueaderos,
                x.AnioConstruccion, x.Etiquetas, x.AsesorResponsableId,
                context.Asesores.Where(a => a.Id == x.AsesorResponsableId).Select(a => a.Nombre).FirstOrDefault()))
            .ToListAsync(ct);
        return new ListInmueblesResponse(rows, query.Page, query.PageSize, total);
    }
}
