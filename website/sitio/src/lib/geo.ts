export type ApproximateLocation = {
  latitude: number;
  longitude: number;
};

// El dominio valida el rango de la coordenada pero no la redondea, y 0,0 pasa
// esa validacion. Un pin en el golfo de Guinea es peor que no mostrar mapa.
export function toDisplayableLocation(
  latitude: number | undefined,
  longitude: number | undefined,
): ApproximateLocation | null {
  if (typeof latitude !== "number" || typeof longitude !== "number") return null;
  if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) return null;
  if (latitude === 0 && longitude === 0) return null;
  if (Math.abs(latitude) > 90 || Math.abs(longitude) > 180) return null;

  return { latitude, longitude };
}

// Radio del area publicada, en metros. La coordenada que guarda el catalogo
// tiene cuatro decimales (unos 11 m), asi que sin este circulo el pin senalaria
// practicamente el portal del inmueble. El circulo comunica "por aqui" y es la
// proteccion real; el tope de zoom solo evita que se lea como direccion.
export const APPROXIMATE_RADIUS_METERS = 350;

// A 6 grados de latitud, el zoom 16 da ~2,4 m por pixel: el circulo de 350 m
// se dibuja con unos 146 px de radio, imposible de confundir con una casa.
// Mas cerca empiezan a leerse tejados y el pin aparentaria precision que el
// dato no tiene.
export const MAX_DETAIL_ZOOM = 16;
export const DEFAULT_DETAIL_ZOOM = 14;
