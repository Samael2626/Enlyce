import type { Metadata } from "next";
import Link from "next/link";
import { CatalogFilters } from "@/components/CatalogFilters";
import { PropertyCard } from "@/components/PropertyCard";
import { buildQuery, getProperties, parseFilters } from "@/lib/api/catalog";

export const metadata: Metadata = {
  title: "Inmuebles en venta y arriendo",
  description:
    "Explora apartamentos, casas, oficinas y locales publicados por L&C en Medellín y el Valle de Aburrá.",
};

type PageProps = { searchParams: Promise<Record<string, string | string[] | undefined>> };

export default async function InmueblesPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const filters = parseFilters(params);
  const page = await getProperties(filters);
  const lastPage = Math.max(1, Math.ceil(page.total / Math.max(1, page.pageSize)));

  return (
    <div className="mx-auto max-w-6xl space-y-8 px-4 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl">Inmuebles disponibles</h1>
        <p className="text-muted">
          {page.total === 0
            ? "No hay publicaciones que coincidan con la búsqueda."
            : `${page.total} ${page.total === 1 ? "inmueble" : "inmuebles"} publicados.`}
        </p>
      </header>

      <CatalogFilters />

      {page.items.length === 0 ? (
        <div className="rounded-sheet border border-line p-8 text-center">
          <p className="mb-3">Ningún inmueble coincide con estos filtros.</p>
          <Link href="/inmuebles" className="text-accent underline">
            Limpiar la búsqueda
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

      {lastPage > 1 && (
        <nav aria-label="Paginación" className="flex items-center justify-between text-sm">
          {page.page > 1 ? (
            <Link
              className="text-accent underline"
              href={`/inmuebles${buildQuery({ ...filters, page: page.page - 1 })}`}
            >
              Anterior
            </Link>
          ) : (
            <span className="text-muted">Anterior</span>
          )}
          <span className="text-muted">
            Página {page.page} de {lastPage}
          </span>
          {page.page < lastPage ? (
            <Link
              className="text-accent underline"
              href={`/inmuebles${buildQuery({ ...filters, page: page.page + 1 })}`}
            >
              Siguiente
            </Link>
          ) : (
            <span className="text-muted">Siguiente</span>
          )}
        </nav>
      )}
    </div>
  );
}
