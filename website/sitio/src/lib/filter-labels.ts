import { formatPrice } from "./format";

// Etiqueta legible de cada filtro activo para los chips del catalogo.
const labels: Record<string, (value: string) => string> = {
  neighborhood: (value) => `“${value}”`,
  minPrice: (value) => `Desde ${formatPrice(Number(value))}`,
  maxPrice: (value) => `Hasta ${formatPrice(Number(value))}`,
  minArea: (value) => `Desde ${value} m²`,
  maxArea: (value) => `Hasta ${value} m²`,
  bedrooms: (value) => `${value}+ habitaciones`,
  bathrooms: (value) => `${value}+ baños`,
  parkingSpaces: (value) => `${value}+ parqueaderos`,
};

export function filterLabel(key: string, value: string): string {
  return labels[key]?.(value) ?? value;
}
