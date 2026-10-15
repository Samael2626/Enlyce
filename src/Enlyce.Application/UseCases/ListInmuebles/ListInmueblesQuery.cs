namespace Enlyce.Application.UseCases.ListInmuebles;

public sealed record ListInmueblesQuery(
    int Page = 1, int PageSize = 20, string? Search = null, string? Modalidad = null, string? Estado = null,
    string? Tipo = null, string? Pais = null, string? Departamento = null, string? Ciudad = null, string? Barrio = null,
    string? Moneda = null, decimal? PrecioDesde = null, decimal? PrecioHasta = null,
    int? AreaDesde = null, int? AreaHasta = null, int? Habitaciones = null, int? Banos = null, int? Parqueaderos = null,
    string Orden = "recent", bool Papelera = false);
public sealed record InmuebleListItem(Guid Id, string Nombre, string Tipo, string Modalidad, string Estado,
    bool TienePublicacion, string Ciudad, string? Barrio, string Moneda, decimal Precio,
    int MetrosCuadrados, int Habitaciones, int Banos, int Parqueaderos);
public sealed record ListInmueblesResponse(IReadOnlyList<InmuebleListItem> Items, int Page, int PageSize, int Total);
