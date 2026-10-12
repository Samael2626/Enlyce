import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { getPropertyBySlug } from "@/lib/api/catalog";
import type { PropertyDetail } from "@/lib/api/client";
import { formatArea, formatOperation, formatPrice, propertyPath } from "@/lib/format";

export const metadata: Metadata = {
  title: "Comparar inmuebles",
  robots: { index: false, follow: true },
};

type PageProps = { searchParams: Promise<Record<string, string | string[] | undefined>> };

type Row = { label: string; value: (property: PropertyDetail) => string; best?: "min" | "max"; numeric?: (property: PropertyDetail) => number };

const rows: Row[] = [
  { label: "Precio", value: (p) => formatPrice(p.price.amount, p.price.currency), best: "min", numeric: (p) => p.price.amount },
  { label: "Administración", value: (p) => (p.administrationFee ? formatPrice(p.administrationFee.amount, p.administrationFee.currency) : "—") },
  { label: "Área", value: (p) => formatArea(p.features.areaSquareMeters), best: "max", numeric: (p) => p.features.areaSquareMeters },
  { label: "Precio por m²", value: (p) => formatPrice(Math.round(p.price.amount / Math.max(1, p.features.areaSquareMeters))), best: "min", numeric: (p) => p.price.amount / Math.max(1, p.features.areaSquareMeters) },
  { label: "Habitaciones", value: (p) => String(p.features.bedrooms), best: "max", numeric: (p) => p.features.bedrooms },
  { label: "Baños", value: (p) => String(p.features.bathrooms), best: "max", numeric: (p) => p.features.bathrooms },
  { label: "Parqueaderos", value: (p) => String(p.features.parkingSpaces), best: "max", numeric: (p) => p.features.parkingSpaces },
  { label: "Estrato", value: (p) => (p.features.stratum ? String(p.features.stratum) : "—") },
  { label: "Ubicación", value: (p) => `${p.location.neighborhood}, ${p.location.municipality}` },
  { label: "Comodidades", value: (p) => (p.features.amenities.length > 0 ? p.features.amenities.join(", ") : "—") },
];

export default async function CompararPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const raw = typeof params.slugs === "string" ? params.slugs : "";
  const slugs = [...new Set(raw.split(",").map((slug) => slug.trim()).filter(Boolean))].slice(0, 3);
  const properties = (await Promise.all(slugs.map((slug) => getPropertyBySlug(slug))))
    .filter((property): property is PropertyDetail => property !== null);

  if (properties.length < 2) {
    return (
      <section className="mx-auto max-w-xl space-y-4 px-4 py-16 text-center">
        <h1 className="text-3xl">Elige al menos dos inmuebles</h1>
        <p className="text-muted">Marca entre dos y tres favoritos para compararlos. Algunos pueden haber dejado de estar publicados.</p>
        <Link href="/favoritos" className="text-accent-strong underline">Ir a mis favoritos</Link>
      </section>
    );
  }

  // Resalta el mejor valor de cada fila numerica cuando hay diferencia real.
  const bestIndex = (row: Row): number | null => {
    if (!row.best || !row.numeric) return null;
    const values = properties.map(row.numeric);
    const target = row.best === "min" ? Math.min(...values) : Math.max(...values);
    return values.filter((value) => value === target).length === 1 ? values.indexOf(target) : null;
  };

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-10">
      <header className="space-y-2">
        <Link href="/favoritos" className="text-sm text-muted hover:text-accent-strong">← Volver a favoritos</Link>
        <h1 className="text-3xl">Comparar inmuebles</h1>
      </header>

      <div className="compare-scroll">
        <table className="compare-table">
          <caption className="sr-only">Comparación de {properties.length} inmuebles</caption>
          <thead>
            <tr>
              <td />
              {properties.map((property) => {
                const cover = property.photos.find((photo) => photo.isCover) ?? property.photos[0];
                return (
                  <th key={property.slug} scope="col">
                    {cover && (
                      <span className="compare-photo">
                        <Image src={cover.url} alt={cover.altText} fill sizes="240px" className="object-cover" />
                      </span>
                    )}
                    <small>{formatOperation(property.operation)} · {property.propertyType}</small>
                    <Link href={propertyPath(property.slug)}>{property.publicTitle}</Link>
                  </th>
                );
              })}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => {
              const best = bestIndex(row);
              return (
                <tr key={row.label}>
                  <th scope="row">{row.label}</th>
                  {properties.map((property, index) => (
                    <td key={property.slug} data-best={index === best ? "true" : undefined}>
                      {row.value(property)}
                      {index === best && <span className="sr-only"> (mejor valor)</span>}
                    </td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
