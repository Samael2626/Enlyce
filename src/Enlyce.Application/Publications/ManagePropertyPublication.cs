using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Publications;

public sealed record CreatePropertyPublicationCommand(
    Guid PropertyId,
    Guid AdvisorId,
    string Slug,
    string PublicTitle,
    string? PublicDescription,
    decimal PriceAmount,
    string PriceCurrency,
    string Municipality,
    string Neighborhood,
    decimal ApproximateLatitude,
    decimal ApproximateLongitude);

public sealed class CreatePropertyPublicationHandler
    : ICommandHandler<CreatePropertyPublicationCommand, PropertyPublicationMutationResponse>
{
    private readonly IPropertyPublicationRepository _publications;
    private readonly IInmuebleRepository _properties;
    private readonly IAsesorRepository _advisors;

    public CreatePropertyPublicationHandler(
        IPropertyPublicationRepository publications,
        IInmuebleRepository properties,
        IAsesorRepository advisors)
    {
        _publications = publications;
        _properties = properties;
        _advisors = advisors;
    }

    public async Task<PropertyPublicationMutationResponse> HandleAsync(
        CreatePropertyPublicationCommand command, CancellationToken ct = default)
    {
        var property = await _properties.GetByIdAsync(command.PropertyId)
            ?? throw new DomainError("El inmueble no existe.");
        if (!property.Activo)
            throw new DomainError("El inmueble esta inactivo.");

        if (await _publications.ExistsForPropertyAsync(command.PropertyId, ct))
            throw new DomainError("El inmueble ya tiene una publicacion.");

        var advisor = await _advisors.ObtenerPorIdAsync(command.AdvisorId)
            ?? throw new DomainError("El asesor no existe.");
        if (!advisor.Activo)
            throw new DomainError("El asesor esta inactivo.");

        var publication = PropertyPublication.Create(
            command.PropertyId,
            command.AdvisorId,
            command.Slug,
            command.PublicTitle,
            command.PublicDescription);
        if (await _publications.SlugExistsAsync(publication.Slug, ct: ct))
            throw new DomainError("El slug ya esta en uso.");

        publication.SetPublicPrice(command.PriceAmount, command.PriceCurrency);
        publication.SetPublicLocation(
            command.Municipality,
            command.Neighborhood,
            command.ApproximateLatitude,
            command.ApproximateLongitude);

        await _publications.SaveAsync(publication, ct);
        return new(publication.Id, publication.Status.ToString());
    }

}

public sealed record UpdatePropertyPublicationCommand(
    Guid Id,
    string Slug,
    string PublicTitle,
    string? PublicDescription,
    decimal PriceAmount,
    string PriceCurrency,
    string Municipality,
    string Neighborhood,
    decimal ApproximateLatitude,
    decimal ApproximateLongitude);

public sealed class UpdatePropertyPublicationHandler
    : ICommandHandler<UpdatePropertyPublicationCommand, PropertyPublicationMutationResponse>
{
    private readonly IPropertyPublicationRepository _publications;

    public UpdatePropertyPublicationHandler(IPropertyPublicationRepository publications) =>
        _publications = publications;

    public async Task<PropertyPublicationMutationResponse> HandleAsync(
        UpdatePropertyPublicationCommand command, CancellationToken ct = default)
    {
        var publication = await _publications.GetByIdAsync(command.Id, ct)
            ?? throw new DomainError("La publicacion no existe.");

        publication.UpdatePublicContent(command.Slug, command.PublicTitle, command.PublicDescription);
        if (await _publications.SlugExistsAsync(publication.Slug, publication.Id, ct))
            throw new DomainError("El slug ya esta en uso.");

        publication.SetPublicPrice(command.PriceAmount, command.PriceCurrency);
        publication.SetPublicLocation(
            command.Municipality,
            command.Neighborhood,
            command.ApproximateLatitude,
            command.ApproximateLongitude);

        await _publications.SaveAsync(publication, ct);
        return new(publication.Id, publication.Status.ToString());
    }
}

public sealed record ChangePropertyPublicationStatusCommand(Guid Id, string Action);

public sealed class ChangePropertyPublicationStatusHandler
    : ICommandHandler<ChangePropertyPublicationStatusCommand, PropertyPublicationMutationResponse>
{
    private readonly IPropertyPublicationRepository _publications;

    public ChangePropertyPublicationStatusHandler(IPropertyPublicationRepository publications) =>
        _publications = publications;

    public async Task<PropertyPublicationMutationResponse> HandleAsync(
        ChangePropertyPublicationStatusCommand command, CancellationToken ct = default)
    {
        var publication = await _publications.GetByIdAsync(command.Id, ct)
            ?? throw new DomainError("La publicacion no existe.");

        switch (command.Action.Trim().ToLowerInvariant())
        {
            case "publish": publication.Publish(); break;
            case "pause": publication.Pause(); break;
            case "withdraw": publication.Withdraw(); break;
            default: throw new DomainError("La accion de publicacion no es valida.");
        }

        await _publications.SaveAsync(publication, ct);
        return new(publication.Id, publication.Status.ToString());
    }
}
