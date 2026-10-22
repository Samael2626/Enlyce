"use client";

import { divIcon } from "leaflet";
import { useEffect } from "react";
import { Circle, MapContainer, Marker, TileLayer, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import {
  APPROXIMATE_RADIUS_METERS,
  DEFAULT_DETAIL_ZOOM,
  MAX_DETAIL_ZOOM,
  type ApproximateLocation,
} from "@/lib/geo";
import { PUBLIC_MAP_TILE_URL } from "./map-config";

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

function ResizeMapToContainer() {
  const map = useMap();

  useEffect(() => {
    let frame = 0;
    let previousWidth = 0;
    let previousHeight = 0;
    const refreshMap = () => {
      const { width, height } = map.getContainer().getBoundingClientRect();
      if (width === previousWidth && height === previousHeight) return;
      previousWidth = width;
      previousHeight = height;
      cancelAnimationFrame(frame);
      frame = requestAnimationFrame(() => {
        map.invalidateSize({ pan: false, debounceMoveend: true });
      });
    };
    const observer = new ResizeObserver(refreshMap);
    observer.observe(map.getContainer());
    observer.observe(map.getContainer().parentElement ?? map.getContainer());
    refreshMap();
    window.addEventListener("resize", refreshMap);

    return () => {
      cancelAnimationFrame(frame);
      observer.disconnect();
      window.removeEventListener("resize", refreshMap);
    };
  }, [map]);

  return null;
}

function SyncMapCenter({ latitude, longitude }: { latitude: number; longitude: number }) {
  const map = useMap();

  useEffect(() => {
    map.setView([latitude, longitude], DEFAULT_DETAIL_ZOOM, { animate: false });
  }, [latitude, longitude, map]);

  return null;
}

export default function PropertyMap({ location, label }: Props) {
  const center: [number, number] = [location.latitude, location.longitude];

  return (
    <div className="h-full w-full" role="region" aria-label={`Mapa de la zona aproximada de ${label}`}>
      <MapContainer
        center={center}
        zoom={DEFAULT_DETAIL_ZOOM}
        maxZoom={MAX_DETAIL_ZOOM}
        minZoom={11}
        preferCanvas={true}
        scrollWheelZoom={false}
        className="h-full w-full"
      >
        <SyncMapCenter latitude={location.latitude} longitude={location.longitude} />
        <ResizeMapToContainer />
        <TileLayer
          // Atribucion obligatoria por la licencia de OpenStreetMap.
          attribution='&copy; colaboradores de <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url={PUBLIC_MAP_TILE_URL}
          maxZoom={MAX_DETAIL_ZOOM}
          updateWhenIdle={true}
          updateWhenZooming={false}
          keepBuffer={1}
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
    </div>
  );
}
