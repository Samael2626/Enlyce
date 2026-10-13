using Enlyce.Application.Imports;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Imports;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Enlyce.Application.Tests.UseCases;

public sealed class ImportExternalListingsHandlerTests
{
    private readonly IExternalListingSource _source = Substitute.For<IExternalListingSource>();
    private readonly IInmuebleRepository _properties = Substitute.For<IInmuebleRepository>();
    private readonly IPropietarioRepository _owners = Substitute.For<IPropietarioRepository>();
    private readonly IPropertyPublicationRepository _publications = Substitute.For<IPropertyPublicationRepository>();
    private readonly IAsesorRepository _advisors = Substitute.For<IAsesorRepository>();
    private readonly IMediaStorage _storage = Substitute.For<IMediaStorage>();
    private readonly ImportExternalListingsHandler _handler;
    private readonly Asesor _admin = Asesor.Crear("Admin", Email.Create("admin@lyc.test"), "hash", "Administrador");
    private readonly List<PropertyPublication> _savedPublications = [];
    private int _storedCount;

    public ImportExternalListingsHandlerTests()
    {
        _handler = new ImportExternalListingsHandler(_source, _properties, _owners, _publications, _advisors, _storage);

        _source.Source.Returns("wasi");
        _advisors.ObtenerPorIdAsync(_admin.Id).Returns(_admin);
        _properties.GetByExternalRefsAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns([]);
        _properties.SaveAsync(Arg.Any<Inmueble>()).Returns(call => call.Arg<Inmueble>());
        _owners.SaveAsync(Arg.Any<Propietario>()).Returns(call => call.Arg<Propietario>());
        _publications.SaveAsync(Arg.Do<PropertyPublication>(_savedPublications.Add), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _source.DownloadPhotoAsync(Arg.Any<ExternalPhoto>(), Arg.Any<CancellationToken>())
            .Returns(call => new ExternalPhotoContent("f.jpg", "image/jpeg", 3, new MemoryStream([1, 2, 3])));
        _storage.StoreAsync(Arg.Any<MediaUpload>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                var url = $"https://cdn.enlyce.test/media/{++_storedCount}.webp";
                return new StoredMedia(url, 1600, 1200, [new MediaVariant(url, 1600)]);
            });
    }

    private static ExternalListing Listing(
        string id = "10472500",
        TipoInmueble? tipo = TipoInmueble.Finca,
        decimal? sale = 3_500_000_000m,
        decimal? rent = null,
        decimal area = 336,
        int photos = 3,
        ExternalAvailability availability = ExternalAvailability.Disponible) =>
        new("wasi", id, "Finca en venta San Vicente", "Descripcion", tipo, "7", sale, rent, "COP",
            "Vereda Las Frias", "San Vicente", "Las Frias", 6.2855m, -75.3342m, area, 3, 2, 10, 3,
            availability,
            Enumerable.Range(1, photos).Reverse()
                .Select(position => new ExternalPhoto($"https://image.wasi.co/{position}", position, ""))
                .ToList());

    private void GivenListings(params ExternalListing[] listings) =>
        _source.GetListingsAsync(Arg.Any<CancellationToken>()).Returns(listings);

    private Task<ImportExternalListingsResponse> Run(bool dryRun = false) =>
        _handler.HandleAsync(new ImportExternalListingsCommand(dryRun, _admin.Id));

    [Fact]
    public async Task VentaYArriendo_SeDividenEnDosInmuebles()
    {
        GivenListings(Listing(sale: 900_000_000m, rent: 4_500_000m));

        var result = await Run();

        Assert.Equal(2, result.Creados);
        Assert.Equal(["wasi:10472500:venta", "wasi:10472500:arriendo"], result.Items.Select(i => i.ExternalRef));
        await _properties.Received(1).SaveAsync(Arg.Is<Inmueble>(i =>
            i.Modalidad == ModalidadInmueble.Venta && i.Precio.Monto == 900_000_000m &&
            i.ExternalRef == "wasi:10472500:venta"));
        await _properties.Received(1).SaveAsync(Arg.Is<Inmueble>(i =>
            i.Modalidad == ModalidadInmueble.Arriendo && i.Precio.Monto == 4_500_000m));
    }

    [Fact]
    public async Task DryRun_NoEscribeNiDescarga()
    {
        GivenListings(Listing());

        var result = await Run(dryRun: true);

        Assert.True(result.DryRun);
        Assert.Equal(1, result.Creados);
        Assert.Equal(3, result.FotosImportadas);
        await _properties.DidNotReceive().SaveAsync(Arg.Any<Inmueble>());
        await _owners.DidNotReceive().SaveAsync(Arg.Any<Propietario>());
        await _source.DidNotReceive().DownloadPhotoAsync(Arg.Any<ExternalPhoto>(), Arg.Any<CancellationToken>());
        Assert.Empty(_savedPublications);
    }

    [Fact]
    public async Task ExistentePorExternalRef_SeActualizaSinDuplicar()
    {
        var existing = Inmueble.Crear("Viejo", "", TipoInmueble.Casa, ModalidadInmueble.Venta,
            Direccion.Crear("Calle 1", "Medellin"), Dinero.Crear(1m), 10, 1, 1, 0, Guid.NewGuid());
        existing.VincularOrigenExterno("wasi:10472500:venta");
        _properties.GetByExternalRefsAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns([existing]);
        _publications.ExistsForPropertyAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(true);
        GivenListings(Listing());

        var result = await Run();

        Assert.Equal(1, result.Actualizados);
        Assert.Equal(0, result.Creados);
        Assert.Equal("Finca en venta San Vicente", existing.Nombre);
        Assert.Equal(3_500_000_000m, existing.Precio.Monto);
        await _properties.Received(1).SaveAsync(existing);
        // Ya tiene publicacion: no se tocan sus fotos.
        Assert.Empty(_savedPublications);
    }

    [Fact]
    public async Task TipoSinEquivalente_SeOmiteConMotivo()
    {
        GivenListings(Listing(tipo: null));

        var result = await Run();

        var item = Assert.Single(result.Items);
        Assert.Equal(ImportOutcome.Omitido, item.Resultado);
        Assert.Contains("sin equivalente", item.Motivo);
        await _properties.DidNotReceive().SaveAsync(Arg.Any<Inmueble>());
    }

    [Fact]
    public async Task SinPrecio_SeOmiteConMotivo()
    {
        GivenListings(Listing(sale: null, rent: null));

        var result = await Run();

        var item = Assert.Single(result.Items);
        Assert.Equal(ImportOutcome.Omitido, item.Resultado);
        Assert.Equal("wasi:10472500:sin-precio", item.ExternalRef);
    }

    [Fact]
    public async Task InvalidoNoFrenaElLote()
    {
        GivenListings(Listing(id: "1", area: 0), Listing(id: "2"));

        var result = await Run();

        Assert.Equal(1, result.Omitidos);
        Assert.Equal(1, result.Creados);
    }

    [Fact]
    public async Task PropietarioGenerico_SeReutilizaSiExiste()
    {
        var placeholder = Propietario.Crear(
            ImportExternalListingsHandler.PlaceholderOwnerName,
            Email.Create(ImportExternalListingsHandler.PlaceholderOwnerEmail));
        _owners.GetByEmailAsync(Arg.Any<Email>()).Returns(placeholder);
        GivenListings(Listing());

        await Run();

        await _owners.DidNotReceive().SaveAsync(Arg.Any<Propietario>());
        await _properties.Received(1).SaveAsync(Arg.Is<Inmueble>(i => i.PropietarioId == placeholder.Id));
    }

    [Fact]
    public async Task PropietarioGenerico_SeCreaSiNoExiste()
    {
        GivenListings(Listing());

        await Run();

        await _owners.Received(1).SaveAsync(Arg.Is<Propietario>(p =>
            p.Nombre == ImportExternalListingsHandler.PlaceholderOwnerName));
    }

    [Fact]
    public async Task Vendido_EntraComoVendido()
    {
        GivenListings(Listing(availability: ExternalAvailability.Vendido));

        await Run();

        await _properties.Received(1).SaveAsync(Arg.Is<Inmueble>(i => i.Estado == EstadoInmueble.Vendido));
    }

    [Fact]
    public async Task Nuevo_CreaBorradorConFotosEnOrdenYPortada()
    {
        GivenListings(Listing(photos: 3));

        var result = await Run();

        Assert.Equal(3, result.FotosImportadas);
        var publication = Assert.Single(_savedPublications);
        Assert.Equal(PublicationStatus.Draft, publication.Status);
        Assert.Equal(_admin.Id, publication.AdvisorId);
        Assert.Equal(3_500_000_000m, publication.PublicPriceAmount);
        Assert.Equal("San Vicente", publication.Municipality);
        Assert.Equal(3, publication.Photos.Count);
        Assert.Single(publication.Photos, photo => photo.IsCover);
        Assert.Equal([0, 1, 2], publication.Photos.Select(photo => photo.Order));
        Received.InOrder(() =>
        {
            _source.DownloadPhotoAsync(Arg.Is<ExternalPhoto>(p => p.Position == 1), Arg.Any<CancellationToken>());
            _source.DownloadPhotoAsync(Arg.Is<ExternalPhoto>(p => p.Position == 2), Arg.Any<CancellationToken>());
            _source.DownloadPhotoAsync(Arg.Is<ExternalPhoto>(p => p.Position == 3), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task MasDe20Fotos_SoloImportaLas20Primeras()
    {
        GivenListings(Listing(photos: 37));

        var result = await Run();

        Assert.Equal(MediaConstraints.MaxPhotosPerPublication, result.FotosImportadas);
    }

    [Fact]
    public async Task FotoRota_SeReportaYSiguenLasDemas()
    {
        _storage.StoreAsync(Arg.Any<MediaUpload>(), Arg.Any<CancellationToken>())
            .Returns(
                _ => throw new DomainError("La imagen debe medir al menos 800x600 pixeles."),
                _ => new StoredMedia("https://cdn.enlyce.test/media/b.webp", 1600, 1200, []),
                _ => new StoredMedia("https://cdn.enlyce.test/media/c.webp", 1600, 1200, []));
        GivenListings(Listing(photos: 3));

        var result = await Run();

        var item = Assert.Single(result.Items);
        Assert.Equal(2, item.FotosImportadas);
        Assert.Contains("800x600", Assert.Single(item.FotosConError!));
        Assert.True(Assert.Single(_savedPublications).Photos.Single(p => p.Order == 0).IsCover);
    }

    [Fact]
    public async Task FalloAlGuardarPublicacion_LimpiaMediosGuardados()
    {
        _publications.SaveAsync(Arg.Any<PropertyPublication>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("db caida"));
        GivenListings(Listing(photos: 2));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Run());

        await _storage.Received(2).RemoveAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FalloDelOrigen_SePropagaRuidoso()
    {
        _source.GetListingsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new ExternalSourceException("Wasi rechazo property/search: codigo 0"));

        await Assert.ThrowsAsync<ExternalSourceException>(() => Run());
    }

    [Fact]
    public async Task AsesorInexistente_ThrowsDomainError()
    {
        await Assert.ThrowsAsync<DomainError>(() =>
            _handler.HandleAsync(new ImportExternalListingsCommand(false, Guid.NewGuid())));
    }

    [Fact]
    public async Task Publish_ConTresFotosYUbicacion_QuedaPublicado()
    {
        GivenListings(Listing(photos: 3));

        var result = await _handler.HandleAsync(new ImportExternalListingsCommand(false, _admin.Id, Publish: true));

        Assert.Equal(1, result.Publicados);
        Assert.Equal(PublicationStatus.Published, Assert.Single(_savedPublications).Status);
    }

    [Fact]
    public async Task Publish_ConPocasFotos_QuedaEnBorradorConMotivo()
    {
        GivenListings(Listing(photos: 2));

        var result = await _handler.HandleAsync(new ImportExternalListingsCommand(false, _admin.Id, Publish: true));

        var item = Assert.Single(result.Items);
        Assert.False(item.Publicado);
        Assert.Contains("3 fotos", item.MotivoBorrador);
        Assert.Equal(PublicationStatus.Draft, Assert.Single(_savedPublications).Status);
    }

    [Fact]
    public async Task SinPublish_QuedaEnBorrador()
    {
        GivenListings(Listing(photos: 3));

        var result = await Run();

        Assert.Equal(0, result.Publicados);
        Assert.Equal(PublicationStatus.Draft, Assert.Single(_savedPublications).Status);
    }

    [Fact]
    public async Task SlugEnUso_AgregaSufijo()
    {
        _publications.SlugExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true, false);
        GivenListings(Listing(photos: 0));

        await Run();

        Assert.EndsWith("-2", Assert.Single(_savedPublications).Slug);
    }
}
