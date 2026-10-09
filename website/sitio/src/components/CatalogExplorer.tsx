"use client";

import dynamic from "next/dynamic";
import { useEffect, useState, type FocusEvent, type PointerEvent, type ReactNode } from "react";
import { APPROXIMATE_RADIUS_METERS, toDisplayableLocation } from "@/lib/geo";
import type { CatalogMapItem } from "./CatalogMap";

const CatalogMap = dynamic(() => import("./CatalogMap"), {
  ssr: false,
  loading: () => <div className="catalog-map-loading" role="status">Cargando el mapa...</div>,
});

type Props = { properties: CatalogMapItem[]; children: ReactNode };

export function CatalogExplorer({ properties, children }: Props) {
  const [mapOpen, setMapOpen] = useState(false);
  const [selectedSlug, setSelectedSlug] = useState<string | null>(null);
  const mappedProperties = properties.flatMap((property) => {
    const location = toDisplayableLocation(property.latitude, property.longitude);
    return location ? [{ ...property, location }] : [];
  });
  const omittedCount = properties.length - mappedProperties.length;

  useEffect(() => {
    document.querySelectorAll<HTMLElement>("[data-catalog-slug]").forEach((card) => {
      if (card.dataset.catalogSlug === selectedSlug) card.dataset.active = "true";
      else delete card.dataset.active;
    });
  }, [selectedSlug]);

  function selectProperty(slug: string, focusCard = false) {
    setSelectedSlug(slug);
    if (!focusCard) return;

    requestAnimationFrame(() => {
      const card = document.getElementById(`catalog-property-${slug}`);
      card?.scrollIntoView({ behavior: "smooth", block: "center" });
      card?.querySelector<HTMLAnchorElement>("a[href]")?.focus({ preventScroll: true });
    });
  }

  function handleListPointer(event: PointerEvent<HTMLDivElement>) {
    if (!mapOpen) return;
    const target = event.target instanceof Element ? event.target.closest<HTMLElement>("[data-catalog-slug]") : null;
    const slug = target?.dataset.catalogSlug;
    if (slug && slug !== selectedSlug) setSelectedSlug(slug);
  }

  function handleListFocus(event: FocusEvent<HTMLDivElement>) {
    if (!mapOpen) return;
    const target = event.target instanceof Element ? event.target.closest<HTMLElement>("[data-catalog-slug]") : null;
    const slug = target?.dataset.catalogSlug;
    if (slug) setSelectedSlug(slug);
  }

  return (
    <section className="catalog-explorer" data-map-open={mapOpen} aria-label="Resultados de inmuebles">
      <div className="catalog-explorer-list" onPointerOver={handleListPointer} onFocusCapture={handleListFocus}>
        <div className="catalog-explorer-heading">
          <h2 className="sr-only">Resultados del catálogo</h2>
          <p>Lista completa en esta página. El mapa muestra solo zonas aproximadas.</p>
          {mappedProperties.length > 0 && (
            <button
              type="button"
              className="catalog-map-toggle"
              aria-expanded={mapOpen}
              aria-controls="catalog-map-panel"
              onClick={() => setMapOpen((open) => !open)}
            >
              {mapOpen ? "Ocultar mapa" : "Mostrar mapa"}
            </button>
          )}
        </div>

        {omittedCount > 0 && (
          <p className="catalog-map-note" role="status">
            {omittedCount} {omittedCount === 1 ? "inmueble no aparece" : "inmuebles no aparecen"} en el mapa porque no tienen coordenadas aproximadas válidas. La lista sigue completa.
          </p>
        )}

        {children}
        <p className="sr-only" aria-live="polite">
          {selectedSlug ? `Inmueble seleccionado: ${properties.find((item) => item.slug === selectedSlug)?.title ?? ""}` : ""}
        </p>
      </div>

      {mappedProperties.length > 0 && (
        <aside
          id="catalog-map-panel"
          className="catalog-map-panel"
          aria-label="Mapa de zonas aproximadas"
          hidden={!mapOpen}
        >
          <p className="catalog-map-note">
            Pines aproximados; el círculo cubre {APPROXIMATE_RADIUS_METERS} m. Nunca mostramos domicilios.
          </p>
          <p className="catalog-map-note">
            Mapa externo OpenStreetMap: al cargarlo, tu navegador solicita mosaicos y comparte el origen del sitio. No enviamos búsquedas ni datos de contacto.
          </p>
          {mapOpen && (
            <div className="catalog-map-frame">
              <CatalogMap
                properties={mappedProperties}
                selectedSlug={selectedSlug}
                onSelect={(slug) => selectProperty(slug, true)}
              />
            </div>
          )}
        </aside>
      )}
    </section>
  );
}
