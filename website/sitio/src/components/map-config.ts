const DEFAULT_PUBLIC_MAP_TILE_URL = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

export const PUBLIC_MAP_TILE_URL =
  process.env.NEXT_PUBLIC_MAP_TILE_URL?.trim() || DEFAULT_PUBLIC_MAP_TILE_URL;

// OSM standard tiles are best-effort and have no SLA; approve the privacy notice before production.
