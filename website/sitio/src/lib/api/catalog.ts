import {
  apiGet,
  apiGetOrNull,
  resolveMediaUrl,
  type PropertyDetail,
  type PropertyPage,
} from "./client";

// Valores que acepta la API, no los nombres del enum de C#.
export const SORT_VALUES = ["publishedAtDesc", "priceAsc", "priceDesc"] as const;
export const MAX_PAGE_SIZE = 100;
export type SortValue = (typeof SORT_VALUES)[number];

export type CatalogFilters = {
  page?: number;
  pageSize?: number;
  operation?: string;
  propertyType?: string;
  municipality?: string;
  neighborhood?: string;
  minPrice?: number;
  maxPrice?: number;
  minArea?: number;
  maxArea?: number;
  bedrooms?: number;
  bathrooms?: number;
  parkingSpaces?: number;
  sort?: SortValue;
};

const FILTER_KEYS: (keyof CatalogFilters)[] = [
  "page", "pageSize", "operation", "propertyType", "municipality", "neighborhood",
  "minPrice", "maxPrice", "minArea", "maxArea", "bedrooms", "bathrooms", "parkingSpaces", "sort",
];

// Los filtros viven en la URL: una busqueda siempre se puede compartir.
export function parseFilters(params: Record<string, string | string[] | undefined>): CatalogFilters {
  const filters: CatalogFilters = {};

  for (const key of FILTER_KEYS) {
    const raw = params[key];
    const value = Array.isArray(raw) ? raw[0] : raw;
    if (value === undefined || value === "") continue;

    if (key === "sort") {
      if ((SORT_VALUES as readonly string[]).includes(value)) filters.sort = value as SortValue;
      continue;
    }

    if (["operation", "propertyType", "municipality", "neighborhood"].includes(key)) {
      Object.assign(filters, { [key]: value });
      continue;
    }

    const numeric = Number(value);
    if (!Number.isFinite(numeric) || numeric < 0) continue;

    // Se acotan contra los limites de la API: una URL compartida o editada a
    // mano no puede tumbar la pagina con un 400.
    if (key === "page") {
      filters.page = Math.max(1, Math.floor(numeric));
      continue;
    }

    if (key === "pageSize") {
      filters.pageSize = Math.min(MAX_PAGE_SIZE, Math.max(1, Math.floor(numeric)));
      continue;
    }

    Object.assign(filters, { [key]: numeric });
  }

  return filters;
}

export function buildQuery(filters: CatalogFilters): string {
  const search = new URLSearchParams();
  for (const key of FILTER_KEYS) {
    const value = filters[key];
    if (value !== undefined && value !== "") search.set(key, String(value));
  }
  const query = search.toString();
  return query ? `?${query}` : "";
}

export async function getProperties(filters: CatalogFilters): Promise<PropertyPage> {
  const page = await apiGet<PropertyPage>(`/api/public/inmuebles${buildQuery(filters)}`);
  return {
    ...page,
    items: page.items.map((property) => ({
      ...property,
      coverPhoto: property.coverPhoto
        ? { ...property.coverPhoto, url: resolveMediaUrl(property.coverPhoto.url) }
        : property.coverPhoto,
    })),
  };
}

export async function getPropertyBySlug(slug: string): Promise<PropertyDetail | null> {
  const property = await apiGetOrNull<PropertyDetail>(
    `/api/public/inmuebles/${encodeURIComponent(slug)}`,
  );
  return property
    ? {
        ...property,
        photos: property.photos.map((photo) => ({
          ...photo,
          url: resolveMediaUrl(photo.url),
        })),
      }
    : null;
}
