using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Api.Development;

public static class DemoCatalogSeeder
{
    private const string OwnerEmail = "catalogo.demo@enlyce.test";
    private const string AdvisorEmail = "asesor.demo@enlyce.test";

    private static readonly DemoProperty[] Properties =
    [
        new("apartamento-laureles-estadio", "Apartamento amplio en Laureles", "Espacios iluminados, balcón y ubicación residencial cerca de servicios.", TipoInmueble.Apartamento, ModalidadInmueble.Venta, "Medellín", "Laureles", 780_000_000m, 118, 3, 3, 2, 6.2443m, -75.5934m),
        new("apartamento-el-poblado", "Apartamento contemporáneo en El Poblado", "Vista urbana, distribución abierta y acceso rápido a la zona empresarial.", TipoInmueble.Apartamento, ModalidadInmueble.Venta, "Medellín", "El Poblado", 1_180_000_000m, 142, 3, 3, 2, 6.2088m, -75.5652m),
        new("apartamento-belen", "Apartamento familiar en Belén", "Vivienda funcional con zonas generosas y rutas de transporte cercanas.", TipoInmueble.Apartamento, ModalidadInmueble.Arriendo, "Medellín", "Belén", 3_400_000m, 91, 3, 2, 1, 6.2307m, -75.6028m),
        new("casa-envigado-loma-esmeraldal", "Casa tranquila en Loma del Esmeraldal", "Casa de varios niveles con terraza, estudio y entorno verde.", TipoInmueble.Casa, ModalidadInmueble.Venta, "Envigado", "Loma del Esmeraldal", 1_650_000_000m, 248, 4, 4, 3, 6.1707m, -75.5767m),
        new("apartamento-envigado-zuniga", "Apartamento moderno en Zúñiga", "Acabados sobrios, buena iluminación y cercanía a comercio local.", TipoInmueble.Apartamento, ModalidadInmueble.Arriendo, "Envigado", "Zúñiga", 5_200_000m, 126, 3, 3, 2, 6.1777m, -75.5839m),
        new("apartamento-sabaneta-asdesillas", "Apartamento con vista en Asdesillas", "Ambiente residencial, balcón panorámico y espacios bien aprovechados.", TipoInmueble.Apartamento, ModalidadInmueble.Venta, "Sabaneta", "Asdesillas", 590_000_000m, 86, 3, 2, 1, 6.1448m, -75.6045m),
        new("casa-sabaneta-canaveralejo", "Casa familiar en Cañaveralejo", "Patio, estudio y áreas independientes para una familia en crecimiento.", TipoInmueble.Casa, ModalidadInmueble.Venta, "Sabaneta", "Cañaveralejo", 920_000_000m, 176, 4, 3, 2, 6.1510m, -75.6161m),
        new("oficina-ciudad-del-rio", "Oficina flexible en Ciudad del Río", "Planta adaptable para equipos pequeños cerca de servicios empresariales.", TipoInmueble.Oficina, ModalidadInmueble.Arriendo, "Medellín", "Ciudad del Río", 4_800_000m, 74, 0, 2, 2, 6.2231m, -75.5734m),
        new("local-comercial-la-america", "Local comercial en La América", "Frente visible y espacio versátil para comercio o servicios.", TipoInmueble.Local, ModalidadInmueble.Arriendo, "Medellín", "La América", 3_100_000m, 62, 0, 1, 0, 6.2515m, -75.6092m),
        new("bodega-itagui-santa-maria", "Bodega operativa en Santa María", "Área abierta, acceso vehicular y ubicación industrial estratégica.", TipoInmueble.Bodega, ModalidadInmueble.Arriendo, "Itagüí", "Santa María", 12_500_000m, 510, 0, 3, 4, 6.1849m, -75.6078m),
        new("lote-las-palmas", "Lote con entorno natural en Las Palmas", "Terreno inclinado con visuales abiertas y acceso desde vía secundaria.", TipoInmueble.Lote, ModalidadInmueble.Venta, "Medellín", "Las Palmas", 1_350_000_000m, 1_850, 0, 0, 0, 6.1868m, -75.5368m),
        new("finca-copacabana-el-cabuyal", "Finca de recreo en El Cabuyal", "Casa principal, árboles maduros y espacio exterior para descanso.", TipoInmueble.Finca, ModalidadInmueble.Venta, "Copacabana", "El Cabuyal", 1_090_000_000m, 2_600, 4, 3, 5, 6.3372m, -75.5084m),
    ];

    public static async Task SeedAsync(EnlyceDbContext db, CancellationToken cancellationToken = default)
    {
        var demoSlugs = Properties.Select(item => item.Slug).ToArray();
        var existingSlugs = await db.PropertyPublications
            .Where(item => demoSlugs.Contains(item.Slug))
            .Select(item => item.Slug)
            .ToListAsync(cancellationToken);

        if (existingSlugs.Count == Properties.Length)
            return;

        var owner = await db.Propietarios
            .SingleOrDefaultAsync(item => item.Email.Value == OwnerEmail, cancellationToken);
        if (owner is null)
        {
            owner = Propietario.Crear("Propietario sintético del catálogo", Email.Create(OwnerEmail));
            db.Propietarios.Add(owner);
        }

        var advisor = await db.Asesores
            .SingleOrDefaultAsync(item => item.Correo.Value == AdvisorEmail, cancellationToken);
        if (advisor is null)
        {
            advisor = Asesor.Crear("Equipo L&C", Email.Create(AdvisorEmail), "demo-account-without-login");
            db.Asesores.Add(advisor);
        }

        var existing = existingSlugs.ToHashSet(StringComparer.Ordinal);
        foreach (var spec in Properties.Where(item => !existing.Contains(item.Slug)))
        {
            var property = Inmueble.Crear(
                spec.Title,
                spec.Description,
                spec.Type,
                spec.Operation,
                Direccion.Crear($"Dirección sintética {spec.Slug}", spec.Municipality, spec.Neighborhood),
                Dinero.Crear(spec.Price),
                spec.Area,
                spec.Bedrooms,
                spec.Bathrooms,
                spec.ParkingSpaces,
                owner.Id);

            var publication = PropertyPublication.Create(
                property.Id,
                advisor.Id,
                spec.Slug,
                spec.Title,
                spec.Description);
            publication.SetPublicPrice(spec.Price);
            publication.SetPublicLocation(spec.Municipality, spec.Neighborhood, spec.Latitude, spec.Longitude);
            AddPhotos(publication, spec);
            publication.Publish();

            db.Inmuebles.Add(property);
            db.PropertyPublications.Add(publication);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddPhotos(PropertyPublication publication, DemoProperty spec)
    {
        publication.AddPhoto($"http://localhost:4173/assets/property-triptych.png?property={spec.Slug}&photo=1", $"Vista principal de {spec.Title}", 0, true);
        publication.AddPhoto($"http://localhost:4173/assets/medellin-hero.png?property={spec.Slug}&photo=2", $"Entorno de {spec.Neighborhood}", 1);
        publication.AddPhoto($"http://localhost:4173/assets/property-triptych.png?property={spec.Slug}&photo=3", $"Detalle interior de {spec.Title}", 2);
    }

    private sealed record DemoProperty(
        string Slug,
        string Title,
        string Description,
        TipoInmueble Type,
        ModalidadInmueble Operation,
        string Municipality,
        string Neighborhood,
        decimal Price,
        int Area,
        int Bedrooms,
        int Bathrooms,
        int ParkingSpaces,
        decimal Latitude,
        decimal Longitude);
}
