import type { Metadata } from "next";
import Link from "next/link";
import { CatalogFilters } from "@/components/CatalogFilters";
import { CatalogExplorer } from "@/components/CatalogExplorer";
import { PropertyCard } from "@/components/PropertyCard";
import { SearchAlertForm } from "@/components/SearchAlertForm";
import { filterLabel } from "@/lib/filter-labels";
import { buildQuery, getProperties, parseFilters } from "@/lib/api/catalog";
import { whatsappLink } from "@/lib/contact";

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
  const criteria = Object.entries(filters)
    .filter(([key]) => !["page", "pageSize", "sort"].includes(key))
    .map(([key, value]) => filterLabel(key, String(value)));
  const alertQuery = buildQuery({ ...filters, page: undefined, pageSize: undefined, sort: undefined });
  const hasFilters = Object.keys(filters).some((key) => !["page", "pageSize", "sort"].includes(key));

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
            {hasFilters ? (
              <>
                <p>Ningún inmueble coincide con estos filtros.</p>
                <span>Prueba quitando algún filtro o cuéntanos qué buscas y te avisamos.</span>
              </>
            ) : (
              <>
                <p>Estamos actualizando el inventario.</p>
                <span>Cuéntanos qué buscas y un asesor te envía opciones que aún no están publicadas.</span>
              </>
            )}
            <div className="catalog-empty-actions">
              <a href={whatsappLink("Hola, busco un inmueble y no encontré opciones en la web. ¿Me ayudan?")} target="_blank" rel="noreferrer">
                Escribir por WhatsApp
              </a>
              <Link href="/contacto">Dejar mis datos</Link>
              {hasFilters && <Link href="/inmuebles">Limpiar la búsqueda</Link>}
            </div>
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

        <div className="catalog-alert">
          <SearchAlertForm
            query={alertQuery}
            summary={criteria.length > 0 ? criteria.join(", ") : "cualquier inmueble nuevo"}
            operation={filters.operation}
          />
        </div>

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
