using Enlyce.Application.Abstractions;

namespace Enlyce.Application.Publications;

public sealed record GetPropertyPublicationsQuery(Guid? AdvisorId);

public sealed class GetPropertyPublicationsHandler
    : IQueryHandler<GetPropertyPublicationsQuery, IReadOnlyList<PropertyPublicationAdminData>>
{
    private readonly IPropertyPublicationAdminReadRepository _repository;

    public GetPropertyPublicationsHandler(IPropertyPublicationAdminReadRepository repository) =>
        _repository = repository;

    public Task<IReadOnlyList<PropertyPublicationAdminData>> HandleAsync(
        GetPropertyPublicationsQuery query, CancellationToken ct = default) =>
        _repository.GetAllAsync(query.AdvisorId, ct);
}

public sealed record GetPropertyPublicationOptionsQuery(Guid? AdvisorId);

public sealed class GetPropertyPublicationOptionsHandler
    : IQueryHandler<GetPropertyPublicationOptionsQuery, PropertyPublicationOptions>
{
    private readonly IPropertyPublicationAdminReadRepository _repository;

    public GetPropertyPublicationOptionsHandler(IPropertyPublicationAdminReadRepository repository) =>
        _repository = repository;

    public Task<PropertyPublicationOptions> HandleAsync(
        GetPropertyPublicationOptionsQuery query, CancellationToken ct = default) =>
        _repository.GetOptionsAsync(query.AdvisorId, ct);
}
