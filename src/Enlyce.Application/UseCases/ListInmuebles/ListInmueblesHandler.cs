using Enlyce.Application.Abstractions;

namespace Enlyce.Application.UseCases.ListInmuebles;

public interface IInmuebleListReadRepository
{
    Task<ListInmueblesResponse> ListAsync(ListInmueblesQuery query, CancellationToken ct);
}

public sealed class ListInmueblesHandler(IInmuebleListReadRepository repository) : IQueryHandler<ListInmueblesQuery, ListInmueblesResponse>
{
    public Task<ListInmueblesResponse> HandleAsync(ListInmueblesQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        return repository.ListAsync(query with { Page = page, PageSize = pageSize }, ct);
    }
}
