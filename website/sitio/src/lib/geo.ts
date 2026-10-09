export type ApproximateLocation = {
  latitude: number;
  longitude: number;
};

// Se valida y vuelve a redondear en el cliente: numeric(9,6) aun puede traer
// coordenadas mas precisas que la zona publica. 0,0 no es ubicacion util.
export function toDisplayableLocation(
  latitude: number | undefined,
  longitude: number | undefined,
): ApproximateLocation | null {
  if (typeof latitude !== "number" || typeof longitude !== "number") return null;
  if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) return null;
  if (latitude === 0 && longitude === 0) return null;
  if (Math.abs(latitude) > 90 || Math.abs(longitude) > 180) return null;

  const roundedLatitude = Number(latitude.toFixed(3));
  const roundedLongitude = Number(longitude.toFixed(3));
  if (roundedLatitude === 0 && roundedLongitude === 0) return null;

  return { latitude: roundedLatitude, longitude: roundedLongitude };
}

// Radio del area publicada. El redondeo a 3 decimales da ~111 m norte-sur en
// Medellin; el circulo refuerza que el pin indica zona, nunca domicilio.
export const APPROXIMATE_RADIUS_METERS = 350;

// El maximo de zoom limita la lectura de tejados y precision falsa del punto.
export const MAX_DETAIL_ZOOM = 16;
export const DEFAULT_DETAIL_ZOOM = 14;
