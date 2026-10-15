import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { CostCalculator } from "@/components/CostCalculator";
import { FavoriteToggle } from "@/components/FavoriteToggle";
import { PropertyCard } from "@/components/PropertyCard";
import { PropertyGallery } from "@/components/PropertyGallery";
import { ShareButton } from "@/components/ShareButton";
import { PropertyLocation } from "@/components/PropertyLocation";
import { getProperties, getPropertyBySlug } from "@/lib/api/catalog";
import type { PropertyListItem } from "@/lib/api/client";
import { formatArea, formatOperation, formatPrice, formatPropertyPrice } from "@/lib/format";
import { serializeJsonLd } from "@/lib/seo/json-ld";
import { whatsappLink } from "@/lib/contact";

type PageProps = { params: Promise<{ slug: string }> };
const siteUrl = (process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000").replace(/\/$/, "");

// Metadatos por inmueble: la razon principal de traer Next a la web publica.
export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const property = await getPropertyBySlug(slug);
  if (!property) return { title: "Inmueble no encontrado" };

  const title = `${property.publicTitle} — ${property.location.neighborhood}`;
  const description = property.publicDescription.slice(0, 160);
  const cover = property.photos.find((photo) => photo.isCover) ?? property.photos[0];

  return {
    title,
    description,
    alternates: { canonical: `/inmuebles/${property.slug}` },
    openGraph: {
      type: "article",
      title,
      description,
      url: `/inmuebles/${property.slug}`,
      images: cover ? [{ url: cover.url, alt: cover.altText }] : undefined,
    },
  };
}

export default async function InmueblePage({ params }: PageProps) {
  const { slug } = await params;
  const property = await getPropertyBySlug(slug);
  if (!property) notFound();

  const photos = [...property.photos].sort((a, b) => a.order - b.order);
  const cover = photos.find((photo) => photo.isCover) ?? photos[0];
  const gallery = cover ? [cover, ...photos.filter((photo) => photo !== cover)] : [];
  const similar = await getSimilar(property.slug, property.operation, property.propertyType, property.location.municipality);
  const canonicalUrl = `${siteUrl}/inmuebles/${encodeURIComponent(property.slug)}`;
  const propertyType = property.propertyType === "Apartamento"
    ? "Apartment"
    : property.propertyType === "Casa"
      ? "House"
      : "Place";
  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "RealEstateListing",
    name: property.publicTitle,
    description: property.publicDescription,
    url: canonicalUrl,
    datePosted: property.publishedAt,
    image: photos.map((photo) => photo.url),
    mainEntity: {
      "@type": propertyType,
      name: property.publicTitle,
      containedInPlace: {
        "@type": "Place",
        name: property.location.neighborhood,
        containedInPlace: { "@type": "City", name: property.location.municipality },
      },
      geo: Number.isFinite(property.location.approximateLatitude) && Number.isFinite(property.location.approximateLongitude)
        ? {
            "@type": "GeoCoordinates",
            latitude: property.location.approximateLatitude,
            longitude: property.location.approximateLongitude,
          }
        : undefined,
      additionalProperty: [
        { "@type": "PropertyValue", name: "Área", value: property.features.areaSquareMeters, unitCode: "MTK" },
        { "@type": "PropertyValue", name: "Habitaciones", value: property.features.bedrooms },
        { "@type": "PropertyValue", name: "Baños", value: property.features.bathrooms },
        { "@type": "PropertyValue", name: "Parqueaderos", value: property.features.parkingSpaces },
      ],
    },
    offers: Number.isFinite(property.price.amount) && property.price.amount > 0 && /^[A-Z]{3}$/.test(property.price.currency)
      ? {
          "@type": "Offer",
          price: property.price.amount,
          priceCurrency: property.price.currency,
          category: property.operation,
          url: canonicalUrl,
        }
      : undefined,
  };

  return (
    <article className="mx-auto max-w-5xl space-y-8 px-4 py-10">
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: serializeJsonLd(jsonLd) }}
      />
      <nav aria-label="Ruta" className="text-sm text-muted">
        <Link href="/inmuebles" className="hover:text-accent-strong">Inmuebles</Link>
        <span aria-hidden="true"> / </span>
        <span>{property.location.neighborhood}</span>
      </nav>

      <header className="space-y-3">
        <p className="text-sm uppercase tracking-wide text-accent-strong">
          {formatOperation(property.operation)} · {property.propertyType}
        </p>
        <h1 className="text-4xl">{property.publicTitle}</h1>
        <p className="text-muted">
          {property.location.neighborhood}, {property.location.municipality}
        </p>
        <p className="font-display text-3xl text-accent-strong">
          {formatPropertyPrice(property.price.amount, property.price.currency, property.operation)}
        </p>
        {property.administrationFee && (
          <p className="text-sm text-muted">
            Administración: {formatPrice(property.administrationFee.amount, property.administrationFee.currency)}
          </p>
        )}
      </header>

      <PropertyGallery photos={gallery} />

      <section aria-labelledby="caracteristicas" className="space-y-3">
        <h2 id="caracteristicas" className="text-2xl">Características</h2>
        <dl className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {[
            { label: "Área", value: formatArea(property.features.areaSquareMeters) },
            { label: "Habitaciones", value: String(property.features.bedrooms) },
            { label: "Baños", value: String(property.features.bathrooms) },
            { label: "Parqueaderos", value: String(property.features.parkingSpaces) },
          ].map((item) => (
            <div key={item.label} className="rounded-sheet bg-surface p-4 shadow-card">
              <dt className="text-sm text-muted">{item.label}</dt>
              <dd className="font-display text-xl">{item.value}</dd>
            </div>
          ))}
        </dl>
        {property.features.amenities.length > 0 && (
          <ul className="flex flex-wrap gap-2 text-sm">
            {property.features.amenities.map((amenity) => (
              <li key={amenity} className="rounded-full border border-line px-3 py-1">
                {amenity}
              </li>
            ))}
          </ul>
        )}
      </section>

      <section aria-labelledby="descripcion" className="space-y-3">
        <h2 id="descripcion" className="text-2xl">Sobre el inmueble</h2>
        <p className="max-w-prose leading-relaxed">{property.publicDescription}</p>
      </section>

      <CostCalculator
        operation={property.operation}
        price={property.price.amount}
        administration={property.administrationFee?.amount}
      />

      <PropertyLocation
        latitude={property.location.approximateLatitude}
        longitude={property.location.approximateLongitude}
        municipality={property.location.municipality}
        neighborhood={property.location.neighborhood}
        title={property.publicTitle}
      />

      <section aria-labelledby="contacto" className="rounded-sheet bg-surface p-6 shadow-card">
        <h2 id="contacto" className="mb-2 text-2xl">¿Quieres verlo?</h2>
        <p className="mb-4 text-muted">
          Te acompaña {property.advisor.displayName}. Agenda una visita sin costo.
        </p>
        <div className="flex flex-wrap items-center gap-3">
          <Link
            href={`/contacto?inmueble=${property.slug}`}
            className="rounded bg-accent-strong px-5 py-2 text-surface hover:bg-ink"
          >
            Solicitar visita
          </Link>
          <FavoriteToggle
            slug={property.slug}
            title={property.publicTitle}
          className="rounded border border-line px-5 py-2 hover:text-accent-strong"
          />
          <ShareButton
            title={property.publicTitle}
            className="rounded border border-line px-5 py-2 hover:text-accent-strong"
          />
        </div>
      </section>

      {similar.length > 0 && (
        <section aria-labelledby="similares" className="space-y-4">
          <h2 id="similares" className="text-2xl">Inmuebles parecidos</h2>
          <ul className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {similar.map((item) => (
              <li key={item.id}><PropertyCard property={item} /></li>
            ))}
          </ul>
        </section>
      )}

      {/* Barra fija en movil: la accion siempre a un toque, sin volver al final. */}
      <div className="property-sticky-cta">
        <div>
          <strong>{formatPropertyPrice(property.price.amount, property.price.currency, property.operation)}</strong>
          <span>{property.location.neighborhood}</span>
        </div>
        <a
          href={whatsappLink(`Hola, me interesa "${property.publicTitle}" (${property.slug}). ¿Sigue disponible?`)}
          target="_blank"
          rel="noreferrer"
          className="property-sticky-whatsapp"
        >
          WhatsApp
        </a>
        <Link href={`/contacto?inmueble=${property.slug}`}>Agendar visita</Link>
      </div>
    </article>
  );
}

// Primero misma operacion, tipo y municipio; si no alcanza, relaja el tipo.
async function getSimilar(slug: string, operation: string, propertyType: string, municipality: string): Promise<PropertyListItem[]> {
  try {
    const strict = await getProperties({ operation, propertyType, municipality, pageSize: 4 });
    let items = strict.items.filter((item) => item.slug !== slug);
    if (items.length < 3) {
      const loose = await getProperties({ operation, pageSize: 6 });
      const seen = new Set(items.map((item) => item.slug));
      items = [...items, ...loose.items.filter((item) => item.slug !== slug && !seen.has(item.slug))];
    }
    return items.slice(0, 3);
  } catch (error) {
    // Los similares son opcionales: la ficha no cae si fallan, pero queda registrado.
    console.error("No se pudieron cargar inmuebles similares", error);
    return [];
  }
}
