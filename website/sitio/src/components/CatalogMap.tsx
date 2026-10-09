"use client";

import { divIcon, latLngBounds } from "leaflet";
import { Circle, MapContainer, Marker, Popup, TileLayer, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { useEffect } from "react";
import Link from "next/link";
import { APPROXIMATE_RADIUS_METERS, MAX_DETAIL_ZOOM, type ApproximateLocation } from "@/lib/geo";
import { propertyPath } from "@/lib/format";
import { PUBLIC_MAP_TILE_URL } from "./map-config";

export type CatalogMapItem = {
  slug: string;
  title: string;
  municipality: string;
  neighborhood: string;
  latitude: number;
  longitude: number;
};
type MappedProperty = CatalogMapItem & { location: ApproximateLocation };
type Props = {
  properties: MappedProperty[];
  selectedSlug: string | null;
  onSelect: (slug: string) => void;
};

const markerIcon = divIcon({
  className: "catalog-map-marker",
  html: '<span aria-hidden="true"></span>',
  iconSize: [28, 28],
  iconAnchor: [14, 14],
});

function MapCamera({ properties, selectedSlug }: Pick<Props, "properties" | "selectedSlug">) {
  const map = useMap();

  useEffect(() => {
    const selected = properties.find(({ slug }) => slug === selectedSlug);
    if (selected) {
      map.panTo([selected.location.latitude, selected.location.longitude], { animate: true });
      return;
    }

    const bounds = latLngBounds(properties.map(({ location }) => [location.latitude, location.longitude]));
    map.fitBounds(bounds, { padding: [24, 24], maxZoom: 12, animate: false });
  }, [map, properties, selectedSlug]);

  return null;
}

export default function CatalogMap({ properties, selectedSlug, onSelect }: Props) {
  const center = properties[0]?.location ?? { latitude: 6.2442, longitude: -75.5937 };

  return (
    <div className="h-full w-full" role="region" aria-label="Mapa de áreas aproximadas de los inmuebles">
      <MapContainer
        center={[center.latitude, center.longitude]}
        zoom={12}
        minZoom={10}
        maxZoom={MAX_DETAIL_ZOOM}
        scrollWheelZoom={false}
        keyboard={true}
        className="h-full w-full"
      >
        <TileLayer
          attribution='&copy; colaboradores de <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url={PUBLIC_MAP_TILE_URL}
          maxZoom={MAX_DETAIL_ZOOM}
          updateWhenIdle={true}
          updateWhenZooming={false}
          keepBuffer={0}
        />
        <MapCamera properties={properties} selectedSlug={selectedSlug} />
        {properties.map((property) => (
          <Circle
            key={`area-${property.slug}`}
            center={[property.location.latitude, property.location.longitude]}
            radius={APPROXIMATE_RADIUS_METERS}
            interactive={false}
            pathOptions={{
              color: "#ae6b48",
              weight: property.slug === selectedSlug ? 1.5 : 1,
              fillColor: "#ae6b48",
              fillOpacity: property.slug === selectedSlug ? 0.12 : 0.035,
            }}
          />
        ))}
        {properties.map((property) => (
          <Marker
            key={property.slug}
            position={[property.location.latitude, property.location.longitude]}
            icon={markerIcon}
            title={`Zona aproximada: ${property.title}`}
            alt={`Ubicar ${property.title} en el mapa`}
            keyboard={true}
            eventHandlers={{
              click: () => onSelect(property.slug),
              keypress: (event) => {
                if (event.originalEvent.keyCode === 13) onSelect(property.slug);
              },
            }}
          >
            <Popup>
              <strong>{property.title}</strong>
              <p>{property.neighborhood}, {property.municipality}</p>
              <p>Zona aproximada: radio de {APPROXIMATE_RADIUS_METERS} m.</p>
              <Link href={propertyPath(property.slug)}>Ver inmueble</Link>
            </Popup>
          </Marker>
        ))}
      </MapContainer>
    </div>
  );
}
