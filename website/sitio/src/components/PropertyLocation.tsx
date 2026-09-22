"use client";

import dynamic from "next/dynamic";
import { toDisplayableLocation } from "@/lib/geo";

// Leaflet toca window al importarse, asi que el mapa nunca entra al render del
// servidor. next/dynamic con ssr:false solo se puede usar desde un componente
// cliente, por eso este envoltorio existe.
const PropertyMap = dynamic(() => import("./PropertyMap"), {
  ssr: false,
  loading: () => (
    <div className="flex h-full items-center justify-center text-sm text-muted">
      Cargando el mapa…
    </div>
  ),
});

type Props = {
  latitude?: number;
  longitude?: number;
  municipality: string;
  neighborhood: string;
  title: string;
};

export function PropertyLocation({
  latitude,
  longitude,
  municipality,
  neighborhood,
  title,
}: Props) {
  const location = toDisplayableLocation(latitude, longitude);

  return (
    <section aria-labelledby="ubicacion" className="space-y-3">
      <h2 id="ubicacion" className="text-2xl">Dónde queda</h2>

      <p className="text-muted">
        {neighborhood}, {municipality}
      </p>

      {location ? (
        <figure className="space-y-2">
          <div className="h-80 overflow-hidden rounded-sheet border border-line bg-surface sm:h-96">
            <PropertyMap location={location} label={title} />
          </div>
          <figcaption className="text-sm text-muted">
            El círculo marca la zona aproximada, no la dirección. La dirección exacta se
            comparte al agendar la visita.
          </figcaption>
        </figure>
      ) : (
        // Sin coordenada utilizable no se dibuja un mapa que mienta.
        <p className="rounded-sheet border border-line p-4 text-sm text-muted">
          Todavía no publicamos la ubicación en el mapa para este inmueble. Escríbenos y
          te contamos exactamente en qué parte de {neighborhood} queda.
        </p>
      )}
    </section>
  );
}
