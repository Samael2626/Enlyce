import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { FavoriteToggle } from "@/components/FavoriteToggle";
import { getPropertyBySlug } from "@/lib/api/catalog";
import { formatArea, formatOperation, formatPrice } from "@/lib/format";

type PageProps = { params: Promise<{ slug: string }> };

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
  const rest = photos.filter((photo) => photo !== cover);

  return (
    <article className="mx-auto max-w-5xl space-y-8 px-4 py-10">
      <nav aria-label="Ruta" className="text-sm text-muted">
        <Link href="/inmuebles" className="hover:text-accent">Inmuebles</Link>
        <span aria-hidden="true"> / </span>
        <span>{property.location.neighborhood}</span>
      </nav>

      <header className="space-y-3">
        <p className="text-sm uppercase tracking-wide text-accent">
          {formatOperation(property.operation)} · {property.propertyType}
        </p>
        <h1 className="text-4xl">{property.publicTitle}</h1>
        <p className="text-muted">
          {property.location.neighborhood}, {property.location.municipality}
        </p>
        <p className="font-display text-3xl text-accent">
          {formatPrice(property.price.amount, property.price.currency)}
        </p>
        {property.administrationFee && (
          <p className="text-sm text-muted">
            Administración: {formatPrice(property.administrationFee.amount, property.administrationFee.currency)}
          </p>
        )}
      </header>

      {cover && (
        <div className="space-y-3">
          <div className="relative aspect-16/9 overflow-hidden rounded-sheet bg-line">
            <Image
              src={cover.url}
              alt={cover.altText}
              fill
              priority
              sizes="(max-width: 1024px) 100vw, 1024px"
              className="object-cover"
            />
          </div>
          {rest.length > 0 && (
            <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3">
              {rest.map((photo) => (
                <li key={photo.url} className="relative aspect-4/3 overflow-hidden rounded bg-line">
                  <Image
                    src={photo.url}
                    alt={photo.altText}
                    fill
                    sizes="(max-width: 640px) 50vw, 33vw"
                    className="object-cover"
                  />
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

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

      <section aria-labelledby="contacto" className="rounded-sheet bg-surface p-6 shadow-card">
        <h2 id="contacto" className="mb-2 text-2xl">¿Quieres verlo?</h2>
        <p className="mb-4 text-muted">
          Te acompaña {property.advisor.displayName}. Agenda una visita sin costo.
        </p>
        <div className="flex flex-wrap items-center gap-3">
          <Link
            href={`/contacto?inmueble=${property.slug}`}
            className="rounded bg-accent px-5 py-2 text-surface hover:bg-accent-strong"
          >
            Solicitar visita
          </Link>
          <FavoriteToggle
            slug={property.slug}
            title={property.publicTitle}
            className="rounded border border-line px-5 py-2 hover:text-accent"
          />
        </div>
      </section>
    </article>
  );
}
