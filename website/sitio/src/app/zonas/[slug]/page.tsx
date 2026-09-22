import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { PropertyCard } from "@/components/PropertyCard";
import { getProperties } from "@/lib/api/catalog";
import { NEIGHBORHOODS, findZone } from "@/lib/zones";

type PageProps = { params: Promise<{ slug: string }> };

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

  return (
    <div className="mx-auto max-w-6xl space-y-8 px-4 py-10">
      <header className="space-y-2">
        <nav aria-label="Ruta" className="text-sm text-muted">
          <Link href="/zonas" className="hover:text-accent">Zonas</Link>
          <span aria-hidden="true"> / </span>
          <span>{zone.name}</span>
        </nav>
        <h1 className="text-4xl">Inmuebles en {zone.name}</h1>
        <p className="max-w-prose text-muted">{zone.summary}</p>
      </header>

      {page.items.length === 0 ? (
        <div className="rounded-sheet border border-line p-8 text-center">
          <p className="mb-3 text-muted">
            Ahora mismo no hay publicaciones activas en {zone.name}.
          </p>
          <Link href="/inmuebles" className="text-accent underline">
            Ver todo el inventario
          </Link>
        </div>
      ) : (
        <ul className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {page.items.map((property) => (
            <li key={property.id}>
              <PropertyCard property={property} />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
