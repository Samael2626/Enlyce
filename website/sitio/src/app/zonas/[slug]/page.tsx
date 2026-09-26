import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { PropertyCard } from "@/components/PropertyCard";
import { getProperties } from "@/lib/api/catalog";
import { NEIGHBORHOODS, findNearbyZones, findZone } from "@/lib/zones";

type PageProps = { params: Promise<{ slug: string }> };

export const dynamic = "force-dynamic";

export function generateStaticParams() {
  return NEIGHBORHOODS.map((zone) => ({ slug: zone.slug }));
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const zone = findZone(slug);
  if (!zone) return { title: "Zona no encontrada" };

  return {
    title: `Inmuebles en ${zone.name}`,
    description: zone.summary,
    alternates: { canonical: `/zonas/${zone.slug}` },
  };
}

export default async function ZonaPage({ params }: PageProps) {
  const { slug } = await params;
  const zone = findZone(slug);
  if (!zone) notFound();

  const page = await getProperties({
    municipality: zone.municipality,
    neighborhood: zone.neighborhood || undefined,
    pageSize: 12,
  });
  const nearbyZones = findNearbyZones(zone);

  const catalogQuery = new URLSearchParams({ municipality: zone.municipality });
  if (zone.neighborhood) catalogQuery.set("neighborhood", zone.neighborhood);

  return (
    <main className="zone-detail-page">
      <header className="zone-detail-hero">
        <nav aria-label="Ruta">
          <Link href="/zonas">Zonas</Link>
          <span aria-hidden="true">/</span>
          <span>{zone.name}</span>
        </nav>
        <div className="zone-detail-heading">
          <div>
            <p className="section-kicker">{zone.kind} · {zone.municipality}</p>
            <h1>Vivir e invertir en {zone.name}.</h1>
          </div>
          <p>{zone.summary}</p>
        </div>
        <dl>
          <div>
            <dt>Ubicación</dt>
            <dd>{zone.municipality}</dd>
          </div>
          <div>
            <dt>Cobertura</dt>
            <dd>{zone.kind}</dd>
          </div>
          <div>
            <dt>Inventario activo</dt>
            <dd>{page.total}</dd>
          </div>
        </dl>
      </header>

      <section className="zone-inventory" aria-labelledby="zone-inventory-title">
        <header>
          <div>
            <p className="section-kicker">Selección disponible</p>
            <h2 id="zone-inventory-title">Propiedades en {zone.name}</h2>
          </div>
          <Link href={`/inmuebles?${catalogQuery.toString()}`}>Abrir búsqueda completa</Link>
        </header>

        {page.items.length === 0 ? (
          <div className="zone-inventory-empty">
            <span>Inventario en actualización</span>
            <h3>Ahora mismo no hay publicaciones activas en {zone.name}.</h3>
            <p>
              Déjanos tus datos y te avisamos cuando aparezca una propiedad que encaje con tu
              búsqueda.
            </p>
            <Link href={`/contacto?${catalogQuery.toString()}`}>Solicitar búsqueda</Link>
          </div>
        ) : (
          <ul>
            {page.items.map((property) => (
              <li key={property.id}>
                <PropertyCard property={property} />
              </li>
            ))}
          </ul>
        )}
      </section>

      {nearbyZones.length > 0 && (
        <section className="zone-nearby" aria-labelledby="zone-nearby-title">
          <header>
            <p className="section-kicker">Amplía el mapa</p>
            <h2 id="zone-nearby-title">Otros sectores de {zone.municipality}</h2>
          </header>
          <ul>
            {nearbyZones.map((nearbyZone) => (
              <li key={nearbyZone.slug}>
                <Link href={`/zonas/${nearbyZone.slug}`}>
                  <small>{nearbyZone.kind}</small>
                  <strong>{nearbyZone.name}</strong>
                  <span aria-hidden="true">→</span>
                </Link>
              </li>
            ))}
          </ul>
        </section>
      )}

      <section className="zone-owner-cta" aria-labelledby="zone-owner-title">
        <div>
          <p className="section-kicker">Propietarios en {zone.name}</p>
          <h2 id="zone-owner-title">¿Tienes una propiedad en esta zona?</h2>
        </div>
        <p>Conversemos sobre venta, arriendo, administración o valoración.</p>
        <Link href="/propietarios">Quiero hablar de mi inmueble</Link>
      </section>
    </main>
  );
}
