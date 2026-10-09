import type { Metadata } from "next";
import Link from "next/link";
import { CatalogFilters } from "@/components/CatalogFilters";
import { CatalogExplorer } from "@/components/CatalogExplorer";
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
    <div className="catalog-page">
      <header className="catalog-page-heading">
        <p className="section-kicker">Inventario L&amp;C</p>
        <h1>Propiedades para vivir e invertir</h1>
        <p>Explora el inventario publicado y afina la búsqueda sin perder tus filtros.</p>
      </header>

      <div className="catalog-shop">
        <CatalogFilters total={page.total} />

        {page.items.length === 0 ? (
          <div className="catalog-empty">
            <p>Ningún inmueble coincide con estos filtros.</p>
            <span>Prueba ampliando el precio, la zona o el tipo de inmueble.</span>
            <Link href="/inmuebles">Limpiar la búsqueda</Link>
          </div>
        ) : (
          <CatalogExplorer
            properties={page.items.map((property) => ({
              slug: property.slug,
              title: property.publicTitle,
              municipality: property.location.municipality,
              neighborhood: property.location.neighborhood,
              latitude: property.location.approximateLatitude,
              longitude: property.location.approximateLongitude,
            }))}
          >
            <ul className="catalog-results" aria-label="Publicaciones de esta página">
              {page.items.map((property) => (
                <li
                  key={property.id}
                  id={`catalog-property-${property.slug}`}
                  data-catalog-slug={property.slug}
                >
                  <PropertyCard property={property} />
                </li>
              ))}
            </ul>
          </CatalogExplorer>
        )}

        {lastPage > 1 && (
          <nav aria-label="Paginación" className="catalog-pagination">
            {page.page > 1 ? (
              <Link href={`/inmuebles${buildQuery({ ...filters, page: page.page - 1 })}`}>← Anterior</Link>
            ) : <span>← Anterior</span>}
            <small>Página {page.page} de {lastPage}</small>
            {page.page < lastPage ? (
              <Link href={`/inmuebles${buildQuery({ ...filters, page: page.page + 1 })}`}>Siguiente →</Link>
            ) : <span>Siguiente →</span>}
          </nav>
        )}
      </div>
    </div>
  );
}
