using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Imports;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.Imports;

// AdvisorId: asesor que ejecuta la importacion; queda como responsable de las publicaciones.
// Publish: publica en la web las que cumplan las reglas del dominio; las demas quedan en borrador.
// No dispara alertas de busqueda: una carga masiva no debe inundar a los suscriptores.
public sealed record ImportExternalListingsCommand(bool DryRun, Guid AdvisorId, bool Publish = false);

public enum ImportOutcome
{
    Creado,
    Actualizado,
    Omitido
}

public sealed record ImportItemResult(
    string ExternalRef,
    string Nombre,
    string Modalidad,
    ImportOutcome Resultado,
    string? Motivo,
    int FotosImportadas = 0,
    IReadOnlyList<string>? FotosConError = null,
    bool Publicado = false,
    string? MotivoBorrador = null);

public sealed record ImportExternalListingsResponse(
    string Source,
    bool DryRun,
    int Leidos,
    int Creados,
    int Actualizados,
    int Omitidos,
    int FotosImportadas,
    int Publicados,
    IReadOnlyList<ImportItemResult> Items);

public sealed class ImportExternalListingsHandler
    : ICommandHandler<ImportExternalListingsCommand, ImportExternalListingsResponse>
{
    // Propietario temporal: Wasi no expone propietarios; los asesores lo reasignan despues.
    public const string PlaceholderOwnerName = "Pendiente asignar (Wasi)";
    public const string PlaceholderOwnerEmail = "pendiente.asignar.wasi@enlyce.invalid";
    private const int MaxDescriptionLength = 2000;
    private const int MaxTitleLength = 200;

    private readonly IExternalListingSource _source;
    private readonly IInmuebleRepository _properties;
    private readonly IPropietarioRepository _owners;
    private readonly IPropertyPublicationRepository _publications;
    private readonly IAsesorRepository _advisors;
    private readonly IMediaStorage _storage;

    public ImportExternalListingsHandler(
        IExternalListingSource source,
        IInmuebleRepository properties,
        IPropietarioRepository owners,
        IPropertyPublicationRepository publications,
        IAsesorRepository advisors,
        IMediaStorage storage)
    {
        _source = source;
        _properties = properties;
        _owners = owners;
        _publications = publications;
        _advisors = advisors;
        _storage = storage;
    }

    public async Task<ImportExternalListingsResponse> HandleAsync(
        ImportExternalListingsCommand command, CancellationToken ct = default)
    {
        var advisor = await _advisors.ObtenerPorIdAsync(command.AdvisorId)
            ?? throw new DomainError("El asesor que importa no existe.");
        if (!advisor.Activo)
            throw new DomainError("El asesor que importa esta inactivo.");

        var listings = await _source.GetListingsAsync(ct);

        // Venta y arriendo son negocios distintos: cada operacion es un inmueble propio.
        var units = listings.SelectMany(Split).ToList();
        var existing = (await _properties.GetByExternalRefsAsync(
                units.Select(unit => unit.ExternalRef).ToList()))
            .ToDictionary(property => property.ExternalRef!);

        var ownerId = command.DryRun ? Guid.NewGuid() : await ResolvePlaceholderOwnerAsync();
        var items = new List<ImportItemResult>(units.Count);

        foreach (var unit in units)
        {
            ct.ThrowIfCancellationRequested();
            items.Add(await ImportUnitAsync(unit, existing, ownerId, command, ct));
        }

        return new ImportExternalListingsResponse(
            _source.Source,
            command.DryRun,
            listings.Count,
            items.Count(item => item.Resultado == ImportOutcome.Creado),
            items.Count(item => item.Resultado == ImportOutcome.Actualizado),
            items.Count(item => item.Resultado == ImportOutcome.Omitido),
            items.Sum(item => item.FotosImportadas),
            items.Count(item => item.Publicado),
            items);
    }

    private async Task<ImportItemResult> ImportUnitAsync(
        ListingUnit unit,
        IReadOnlyDictionary<string, Inmueble> existing,
        Guid ownerId,
        ImportExternalListingsCommand command,
        CancellationToken ct)
    {
        var listing = unit.Listing;
        if (listing.Tipo is not { } tipo)
            return Skip(unit, $"Tipo '{listing.SourceTypeLabel}' sin equivalente en el CRM.");

        Inmueble property;
        ImportOutcome outcome;
        try
        {
            var nombre = Truncate(listing.Title.Trim(), MaxTitleLength);
            var descripcion = BuildDescription(listing);
            var direccion = Direccion.Crear(
                FirstNonEmpty(listing.Street, listing.Neighborhood, listing.City),
                listing.City,
                string.IsNullOrWhiteSpace(listing.Neighborhood) ? null : listing.Neighborhood);
            var precio = Dinero.Crear(unit.Price, listing.Currency);
            var metros = (int)Math.Round(listing.Area, MidpointRounding.AwayFromZero);

            if (existing.TryGetValue(unit.ExternalRef, out var found))
            {
                property = found;
                property.ActualizarDatos(nombre, descripcion, tipo, unit.Modalidad, direccion, precio,
                    metros, listing.Bedrooms, listing.Bathrooms, listing.Garages,
                    property.InternalFeatures, property.ExternalFeatures, property.AnioConstruccion,
                    property.EstadoFisico, property.Etiquetas);
                outcome = ImportOutcome.Actualizado;
            }
            else
            {
                property = Inmueble.Crear(nombre, descripcion, tipo, unit.Modalidad, direccion, precio,
                    metros, listing.Bedrooms, listing.Bathrooms, listing.Garages, ownerId);
                property.VincularOrigenExterno(unit.ExternalRef);
                outcome = ImportOutcome.Creado;
            }

            ApplyAvailability(property, listing.Availability);
        }
        catch (DomainError error)
        {
            // Un inmueble invalido no frena el lote, pero queda reportado con su motivo.
            return Skip(unit, error.Message);
        }

        var plannedPhotos = OrderedPhotos(listing).Count;
        if (command.DryRun)
        {
            var photos = outcome == ImportOutcome.Creado ? plannedPhotos : 0;
            return new(unit.ExternalRef, property.Nombre, unit.Modalidad.ToString(), outcome, null, photos);
        }

        await _properties.SaveAsync(property);

        // Solo se crea borrador si el inmueble aun no tiene publicacion: no se pisan
        // fotos ni textos que un asesor ya haya curado.
        if (await _publications.ExistsForPropertyAsync(property.Id, ct))
            return new(unit.ExternalRef, property.Nombre, unit.Modalidad.ToString(), outcome, null);

        var draft = await CreatePublicationAsync(unit, property, command, ct);
        return new(unit.ExternalRef, property.Nombre, unit.Modalidad.ToString(), outcome, null,
            draft.Imported, draft.Failures.Count == 0 ? null : draft.Failures, draft.Published, draft.DraftReason);
    }

    private async Task<PublicationResult> CreatePublicationAsync(
        ListingUnit unit, Inmueble property, ImportExternalListingsCommand command, CancellationToken ct)
    {
        var listing = unit.Listing;
        var publication = PropertyPublication.Create(
            property.Id,
            command.AdvisorId,
            $"{Truncate(listing.Title, 150)} {unit.Modalidad} {listing.SourceId}",
            property.Nombre,
            Truncate(property.Descripcion, 4_000));
        await EnsureUniqueSlugAsync(publication, ct);
        publication.SetPublicPrice(unit.Price, listing.Currency);

        if (listing.Latitude is { } latitude && listing.Longitude is { } longitude &&
            (latitude != 0 || longitude != 0))
        {
            publication.SetPublicLocation(
                listing.City,
                FirstNonEmpty(listing.Neighborhood, listing.City),
                latitude,
                longitude);
        }

        var stored = new List<string>();
        var failures = new List<string>();
        string? draftReason = null;
        try
        {
            foreach (var photo in OrderedPhotos(listing))
            {
                string? pending = null;
                try
                {
                    var content = await _source.DownloadPhotoAsync(photo, ct);
                    await using var stream = content.Content;
                    var upload = MediaUpload.Create(
                        publication.Id, content.FileName, content.ContentType, content.SizeBytes, stream);
                    pending = (await _storage.StoreAsync(upload, ct)).Url;
                    publication.AddPhoto(pending, AltText(photo, property), stored.Count, stored.Count == 0);
                    stored.Add(pending);
                    pending = null;
                }
                catch (Exception error) when (error is DomainError or ExternalSourceException)
                {
                    // Una foto rota o pequena no tumba el inmueble; queda en el reporte.
                    failures.Add($"Foto {photo.Position}: {error.Message}");
                    if (pending is not null)
                        await _storage.RemoveAsync(publication.Id, pending, CancellationToken.None);
                }
                catch when (pending is not null)
                {
                    await _storage.RemoveAsync(publication.Id, pending, CancellationToken.None);
                    throw;
                }
            }

            if (command.Publish)
            {
                try
                {
                    publication.Publish();
                }
                catch (DomainError error)
                {
                    // El dominio decide que es publicable; lo que no cumple queda en borrador.
                    draftReason = error.Message;
                }
            }

            await _publications.SaveAsync(publication, ct);
        }
        catch
        {
            // Sin publicacion guardada (fallo o cancelacion a mitad), las variantes
            // ya subidas quedarian huerfanas en el almacenamiento.
            foreach (var url in stored)
                await _storage.RemoveAsync(publication.Id, url, CancellationToken.None);
            throw;
        }

        return new(stored.Count, failures, publication.Status == PublicationStatus.Published, draftReason);
    }

    private sealed record PublicationResult(int Imported, List<string> Failures, bool Published, string? DraftReason);

    // El id de origen y la modalidad ya hacen unico el slug frente a otros importados;
    // el sufijo solo cubre choques con publicaciones creadas a mano.
    private async Task EnsureUniqueSlugAsync(PropertyPublication publication, CancellationToken ct)
    {
        var baseSlug = publication.Slug;
        for (var attempt = 2; await _publications.SlugExistsAsync(publication.Slug, ct: ct); attempt++)
            publication.UpdatePublicContent($"{baseSlug}-{attempt}", publication.PublicTitle, publication.PublicDescription);
    }

    private static IReadOnlyList<ExternalPhoto> OrderedPhotos(ExternalListing listing) =>
        listing.Photos
            .OrderBy(photo => photo.Position)
            .Take(MediaConstraints.MaxPhotosPerPublication)
            .ToList();

    private static string AltText(ExternalPhoto photo, Inmueble property) =>
        Truncate(string.IsNullOrWhiteSpace(photo.Description) ? property.Nombre : photo.Description.Trim(), 200);

    private IEnumerable<ListingUnit> Split(ExternalListing listing)
    {
        var baseRef = $"{_source.Source}:{listing.SourceId}";
        var emitted = false;

        if (listing.SalePrice is > 0)
        {
            emitted = true;
            yield return new(listing, $"{baseRef}:venta", ModalidadInmueble.Venta, listing.SalePrice.Value);
        }

        if (listing.RentPrice is > 0)
        {
            emitted = true;
            yield return new(listing, $"{baseRef}:arriendo", ModalidadInmueble.Arriendo, listing.RentPrice.Value);
        }

        // Sin precio no se crea nada, pero el reporte debe mostrarlo.
        if (!emitted)
            yield return new(listing, $"{baseRef}:sin-precio", ModalidadInmueble.Venta, 0m);
    }

    private static void ApplyAvailability(Inmueble property, ExternalAvailability availability)
    {
        if (property.Estado is not (EstadoInmueble.Disponible or EstadoInmueble.Reservado))
            return;

        if (availability == ExternalAvailability.Vendido)
            property.MarcarVendido();
        else if (availability == ExternalAvailability.Arrendado)
            property.MarcarArrendado();
    }

    private async Task<Guid> ResolvePlaceholderOwnerAsync()
    {
        var email = Email.Create(PlaceholderOwnerEmail);
        var owner = await _owners.GetByEmailAsync(email)
            ?? await _owners.SaveAsync(Propietario.Crear(PlaceholderOwnerName, email));
        return owner.Id;
    }

    private static string BuildDescription(ExternalListing listing)
    {
        var description = listing.Description.Trim();
        if (listing.Stratum is > 0)
            description = $"Estrato {listing.Stratum}.\n\n{description}".Trim();

        return Truncate(description, MaxDescriptionLength);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..(max - 3)].TrimEnd() + "...";

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static ImportItemResult Skip(ListingUnit unit, string reason) =>
        new(unit.ExternalRef, unit.Listing.Title.Trim(), unit.Modalidad.ToString(), ImportOutcome.Omitido, reason);

    private sealed record ListingUnit(
        ExternalListing Listing, string ExternalRef, ModalidadInmueble Modalidad, decimal Price);
}
