"use client";

import { divIcon } from "leaflet";
import { Circle, MapContainer, Marker, TileLayer } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import {
  APPROXIMATE_RADIUS_METERS,
  DEFAULT_DETAIL_ZOOM,
  MAX_DETAIL_ZOOM,
  type ApproximateLocation,
} from "@/lib/geo";

type Props = {
  location: ApproximateLocation;
  label: string;
};

// Marcador propio: el pin azul de Leaflet no pega con la paleta editorial y
// ademas exige servir sus PNG desde el paquete.
const marker = divIcon({
  className: "",
  html: `<span class="block h-3 w-3 -translate-x-1/2 -translate-y-1/2 rounded-full bg-accent ring-4 ring-surface"></span>`,
  iconSize: [0, 0],
  iconAnchor: [0, 0],
});

export default function PropertyMap({ location, label }: Props) {
  const center: [number, number] = [location.latitude, location.longitude];

  return (
    <MapContainer
      center={center}
      zoom={DEFAULT_DETAIL_ZOOM}
      maxZoom={MAX_DETAIL_ZOOM}
      minZoom={11}
      scrollWheelZoom={false}
      className="h-full w-full"
      // El teclado sigue funcionando: se navega con tab y flechas.
      aria-label={`Mapa de la zona aproximada de ${label}`}
    >
      <TileLayer
        // Atribucion obligatoria por la licencia de OpenStreetMap.
        attribution='&copy; colaboradores de <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        url="https://tile.openstreetmap.org/{z}/{x}/{y}.png"
        maxZoom={MAX_DETAIL_ZOOM}
      />
      <Circle
        center={center}
        radius={APPROXIMATE_RADIUS_METERS}
        pathOptions={{
          color: "#ae6b48",
          weight: 1.5,
          fillColor: "#ae6b48",
          fillOpacity: 0.12,
        }}
      />
      <Marker position={center} icon={marker} />
    </MapContainer>
  );
}
