namespace Enlyce.Application.UseCases.ListInmuebles;

public sealed record ListInmueblesQuery(int Page = 1, int PageSize = 20, string? Search = null, string? Modalidad = null, string? Estado = null);
public sealed record InmuebleListItem(Guid Id, string Nombre, string Tipo, string Modalidad, string Estado, bool TienePublicacion);
public sealed record ListInmueblesResponse(IReadOnlyList<InmuebleListItem> Items, int Page, int PageSize, int Total);
