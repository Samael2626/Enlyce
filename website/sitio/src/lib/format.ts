const priceFormatter = new Intl.NumberFormat("es-CO", {
  style: "currency",
  currency: "COP",
  maximumFractionDigits: 0,
});

const numberFormatter = new Intl.NumberFormat("es-CO");

export function formatPrice(amount: number, currency = "COP"): string {
  if (currency !== "COP") return `${currency} ${numberFormatter.format(amount)}`;
  return priceFormatter.format(amount);
}

export function formatArea(squareMeters: number): string {
  return `${numberFormatter.format(squareMeters)} m²`;
}

export function formatOperation(operation: string): string {
  return operation === "Arriendo" ? "En arriendo" : "En venta";
}

export function propertyPath(slug: string): string {
  return `/inmuebles/${slug}`;
}
