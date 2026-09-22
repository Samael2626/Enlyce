"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useTransition } from "react";

const operations = [
  { value: "", label: "Venta y arriendo" },
  { value: "Venta", label: "Venta" },
  { value: "Arriendo", label: "Arriendo" },
];

const propertyTypes = [
  { value: "", label: "Todos los tipos" },
  { value: "Apartamento", label: "Apartamento" },
  { value: "Casa", label: "Casa" },
  { value: "Oficina", label: "Oficina" },
  { value: "Local", label: "Local" },
  { value: "Bodega", label: "Bodega" },
  { value: "Lote", label: "Lote" },
  { value: "Finca", label: "Finca" },
];

const sorts = [
  { value: "publishedAtDesc", label: "Más recientes" },
  { value: "priceAsc", label: "Menor precio" },
  { value: "priceDesc", label: "Mayor precio" },
];

const fieldClass =
  "w-full rounded border border-line bg-surface px-3 py-2 text-sm focus:border-accent";

export function CatalogFilters() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [pending, startTransition] = useTransition();

  // Cada cambio reescribe la URL: la busqueda es compartible y navegable atras.
  function update(key: string, value: string) {
    const params = new URLSearchParams(searchParams.toString());
    if (value) params.set(key, value);
    else params.delete(key);
    params.delete("page");
    startTransition(() => router.push(`${pathname}?${params.toString()}`));
  }

  const current = (key: string) => searchParams.get(key) ?? "";

  return (
    <form
      aria-label="Filtros de búsqueda"
      className="grid gap-3 rounded-sheet bg-surface p-4 shadow-card sm:grid-cols-2 lg:grid-cols-4"
      data-pending={pending ? "" : undefined}
    >
      <label className="text-sm">
        <span className="mb-1 block text-muted">Operación</span>
        <select
          className={fieldClass}
          value={current("operation")}
          onChange={(event) => update("operation", event.target.value)}
        >
          {operations.map((item) => (
            <option key={item.label} value={item.value}>{item.label}</option>
          ))}
        </select>
      </label>

      <label className="text-sm">
        <span className="mb-1 block text-muted">Tipo</span>
        <select
          className={fieldClass}
          value={current("propertyType")}
          onChange={(event) => update("propertyType", event.target.value)}
        >
          {propertyTypes.map((item) => (
            <option key={item.label} value={item.value}>{item.label}</option>
          ))}
        </select>
      </label>

      <label className="text-sm">
        <span className="mb-1 block text-muted">Barrio</span>
        <input
          className={fieldClass}
          type="search"
          placeholder="Laureles, El Poblado…"
          defaultValue={current("neighborhood")}
          onBlur={(event) => update("neighborhood", event.target.value.trim())}
        />
      </label>

      <label className="text-sm">
        <span className="mb-1 block text-muted">Orden</span>
        <select
          className={fieldClass}
          value={current("sort") || "publishedAtDesc"}
          onChange={(event) => update("sort", event.target.value)}
        >
          {sorts.map((item) => (
            <option key={item.value} value={item.value}>{item.label}</option>
          ))}
        </select>
      </label>
    </form>
  );
}
