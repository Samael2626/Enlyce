using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.IntegrationTests;

internal static class PublicCatalogTestData
{
    public const string PublishedApartmentSlug = "apartamento-publico-laureles";
    public const string PublishedHouseSlug = "casa-publica-envigado";
    public const string PausedSlug = "apartamento-pausado-belen";
    public const string PrivateStreet = "Carrera privada 66 #10-20";
    public const string OwnerName = "Propietario Secreto Catalogo";

    public static async Task SeedAsync(EnlyceDbContext db)
    {
        if (await db.PropertyPublications.AnyAsync(
                publication => publication.Slug == PublishedApartmentSlug))
            return;

        var owner = Propietario.Crear(
            OwnerName,
            Email.Create("owner-catalog@example.test"),
            Telefono.Create("3105550101"));
        var advisor = Asesor.Crear(
            "Asesor L&C",
            Email.Create("advisor-catalog@example.test"),
            "test-password-hash");

        var apartment = CreateProperty(
            "Apartamento Laureles",
            TipoInmueble.Apartamento,
            ModalidadInmueble.Venta,
            620_000_000m,
            92,
            3,
            2,
            1,
            owner.Id,
            "Medellín",
            "Laureles");
        var house = CreateProperty(
            "Casa Envigado",
            TipoInmueble.Casa,
            ModalidadInmueble.Arriendo,
            5_000_000m,
            140,
            4,
            3,
            2,
            owner.Id,
            "Envigado",
            "El Dorado");
        var pausedProperty = CreateProperty(
            "Apartamento Belén",
            TipoInmueble.Apartamento,
            ModalidadInmueble.Venta,
            450_000_000m,
            75,
            2,
            2,
            1,
            owner.Id,
            "Medellín",
            "Belén");

        var apartmentPublication = CreatePublication(
            apartment,
            advisor,
            PublishedApartmentSlug,
            "Apartamento iluminado en Laureles",
            620_000_000m,
            "Medellín",
            "Laureles");
        var housePublication = CreatePublication(
            house,
            advisor,
            PublishedHouseSlug,
            "Casa familiar en Envigado",
            5_000_000m,
            "Envigado",
            "El Dorado");
        var pausedPublication = CreatePublication(
            pausedProperty,
            advisor,
            PausedSlug,
            "Apartamento pausado",
            450_000_000m,
            "Medellín",
            "Belén");
        pausedPublication.Pause();

        db.Propietarios.Add(owner);
        db.Asesores.Add(advisor);
        db.Inmuebles.AddRange(apartment, house, pausedProperty);
        db.PropertyPublications.AddRange(
            apartmentPublication,
            housePublication,
            pausedPublication);
        await db.SaveChangesAsync();
    }

    private static Inmueble CreateProperty(
        string name,
        TipoInmueble type,
        ModalidadInmueble operation,
        decimal price,
        int area,
        int bedrooms,
        int bathrooms,
        int parkingSpaces,
        Guid ownerId,
        string city,
        string neighborhood) =>
        Inmueble.Crear(
            name,
            "Descripción interna que no debe filtrarse.",
            type,
            operation,
            Direccion.Crear(PrivateStreet, city, neighborhood),
            Dinero.Crear(price),
            area,
            bedrooms,
            bathrooms,
            parkingSpaces,
            ownerId);

    private static PropertyPublication CreatePublication(
        Inmueble property,
        Asesor advisor,
        string slug,
        string title,
        decimal price,
        string municipality,
        string neighborhood)
    {
        var publication = PropertyPublication.Create(
            property.Id,
            advisor.Id,
            slug,
            title,
            "Descripción pública del inmueble.");
        publication.SetPublicPrice(price);
        publication.SetPublicLocation(municipality, neighborhood, 6.2443m, -75.5934m);
        publication.AddPhoto($"https://media.example.test/{slug}/cover.webp", "Portada", 0, true);
        publication.AddPhoto($"https://media.example.test/{slug}/two.webp", "Cocina", 1);
        publication.AddPhoto($"https://media.example.test/{slug}/three.webp", "Habitación", 2);
        publication.Publish();
        return publication;
    }
}
