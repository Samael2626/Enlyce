const currencyFormatter = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0,
});

const numberFormatter = new Intl.NumberFormat('es-CO');

export function formatCurrency(money) {
  if (!money || !Number.isFinite(Number(money.amount))) return 'Precio por consultar';

  if (money.currency === 'COP') return currencyFormatter.format(Number(money.amount));
  return `${numberFormatter.format(Number(money.amount))} ${money.currency || ''}`.trim();
}

export function formatPropertyFacts(property) {
  return [
    `${numberFormatter.format(property.areaSquareMeters)} m²`,
    `${property.bedrooms} hab.`,
    `${property.bathrooms} baños`,
    `${property.parkingSpaces} parq.`,
  ].join(' · ');
}

export function formatLocation(location) {
  return [location?.neighborhood, location?.municipality].filter(Boolean).join(', ');
}

export function formatPublishedAt(value) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  return new Intl.DateTimeFormat('es-CO', { dateStyle: 'medium' }).format(date);
}

export function safeMediaUrl(value) {
  if (typeof value !== 'string') return '';

  try {
    const url = new URL(value, globalThis.location?.href || 'http://localhost');
    return ['http:', 'https:'].includes(url.protocol) ? url.href : '';
  } catch {
    return '';
  }
}
