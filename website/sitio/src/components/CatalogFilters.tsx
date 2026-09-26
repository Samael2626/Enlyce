"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { type FormEvent, useTransition } from "react";

const operations = [
  { value: "", label: "Todos" },
  { value: "Venta", label: "Venta" },
  { value: "Arriendo", label: "Arriendo" },
] as const;

const propertyTypes = [
  { value: "", label: "Todos los tipos" },
  { value: "Apartamento", label: "Apartamento" },
  { value: "Casa", label: "Casa" },
  { value: "Oficina", label: "Oficina" },
  { value: "Local", label: "Local" },
  { value: "Bodega", label: "Bodega" },
  { value: "Lote", label: "Lote" },
  { value: "Finca", label: "Finca" },
] as const;

const sorts = [
  { value: "publishedAtDesc", label: "Más recientes" },
  { value: "priceAsc", label: "Menor precio" },
  { value: "priceDesc", label: "Mayor precio" },
] as const;

const filterKeys = [
  "operation", "propertyType", "municipality", "neighborhood", "minPrice",
  "maxPrice", "minArea", "maxArea", "bedrooms", "bathrooms", "parkingSpaces",
] as const;

const fieldClass = "catalog-filter-control";

export function CatalogFilters({ total }: { total: number }) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [pending, startTransition] = useTransition();
  const current = (key: string) => searchParams.get(key) ?? "";

  function navigate(params: URLSearchParams) {
    const query = params.toString();
    startTransition(() => router.push(query ? `${pathname}?${query}` : pathname));
  }

  function update(key: string, value: string) {
    const params = new URLSearchParams(searchParams.toString());
    if (value) params.set(key, value);
    else params.delete(key);
    params.delete("page");
    navigate(params);
  }

  function handleFilterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const params = new URLSearchParams(searchParams.toString());
    filterKeys.forEach((key) => params.delete(key));
    for (const key of filterKeys) {
      const value = data.get(key);
      if (typeof value === "string" && value.trim()) params.set(key, value.trim());
    }
    params.delete("page");
    navigate(params);
  }

  function handleLocationSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = new FormData(event.currentTarget).get("neighborhood");
    update("neighborhood", typeof value === "string" ? value.trim() : "");
  }

  function clearFilters() {
    const params = new URLSearchParams(searchParams.toString());
    filterKeys.forEach((key) => params.delete(key));
    params.delete("sort");
    params.delete("page");
    navigate(params);
  }

  const activeFilters = filterKeys
    .map((key) => [key, current(key)] as [string, string])
    .filter((entry) => Boolean(entry[1]));

  return (
    <>
      <details className="catalog-filter-panel" data-pending={pending ? "" : undefined}>
        <summary>
          <span>Filtrar inventario</span>
          <small>{activeFilters.length ? `${activeFilters.length} activos` : "Ver opciones"}</small>
        </summary>
        <form key={searchParams.toString()} onSubmit={handleFilterSubmit} className="catalog-filter-form">
          <fieldset>
            <legend>Operación</legend>
            <div className="catalog-filter-options">
              {operations.map((option) => (
                <label key={option.label}>
                  <input type="radio" name="operation" value={option.value} defaultChecked={current("operation") === option.value} />
                  <span>{option.label}</span>
                </label>
              ))}
            </div>
          </fieldset>

          <label>
            <span>Tipo de inmueble</span>
            <select className={fieldClass} name="propertyType" defaultValue={current("propertyType")}>
              {propertyTypes.map((item) => <option key={item.label} value={item.value}>{item.label}</option>)}
            </select>
          </label>

          <label>
            <span>Municipio</span>
            <input className={fieldClass} name="municipality" type="search" placeholder="Medellín, Envigado…" defaultValue={current("municipality")} />
          </label>

          <fieldset>
            <legend>Rango de precio</legend>
            <div className="catalog-filter-pair">
              <input className={fieldClass} name="minPrice" type="number" min="0" placeholder="Desde $" defaultValue={current("minPrice")} />
              <input className={fieldClass} name="maxPrice" type="number" min="0" placeholder="Hasta $" defaultValue={current("maxPrice")} />
            </div>
          </fieldset>

          <fieldset>
            <legend>Área</legend>
            <div className="catalog-filter-pair">
              <input className={fieldClass} name="minArea" type="number" min="0" placeholder="Mín. m²" defaultValue={current("minArea")} />
              <input className={fieldClass} name="maxArea" type="number" min="0" placeholder="Máx. m²" defaultValue={current("maxArea")} />
            </div>
          </fieldset>

          <div className="catalog-filter-pair catalog-filter-features">
            <label><span>Habitaciones</span><input className={fieldClass} name="bedrooms" type="number" min="0" placeholder="Mín." defaultValue={current("bedrooms")} /></label>
            <label><span>Baños</span><input className={fieldClass} name="bathrooms" type="number" min="0" placeholder="Mín." defaultValue={current("bathrooms")} /></label>
          </div>

          <label>
            <span>Parqueaderos</span>
            <input className={fieldClass} name="parkingSpaces" type="number" min="0" placeholder="Mínimo" defaultValue={current("parkingSpaces")} />
          </label>

          <div className="catalog-filter-actions">
            <button type="button" onClick={clearFilters}>Limpiar</button>
            <button type="submit">Aplicar filtros</button>
          </div>
        </form>
      </details>

      <section className="catalog-toolbar" aria-label="Controles del catálogo">
        <form onSubmit={handleLocationSubmit} className="catalog-location-search">
          <SearchIcon />
          <input key={current("neighborhood")} name="neighborhood" type="search" placeholder="Buscar por barrio o sector…" defaultValue={current("neighborhood")} aria-label="Barrio o sector" />
          <button type="submit">Buscar</button>
        </form>

        <div className="catalog-toolbar-meta">
          <strong>{total} {total === 1 ? "resultado" : "resultados"}</strong>
          <label>
            <span className="sr-only">Ordenar resultados</span>
            <select value={current("sort") || "publishedAtDesc"} onChange={(event) => update("sort", event.target.value)}>
              {sorts.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
            </select>
          </label>
        </div>

        {activeFilters.length > 0 && (
          <div className="catalog-active-filters" aria-label="Filtros activos">
            {activeFilters.map(([key, value]) => (
              <button key={key} type="button" onClick={() => update(key, "")}>{value}<span aria-hidden="true">×</span></button>
            ))}
          </div>
        )}
      </section>
    </>
  );
}

function SearchIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" focusable="false">
      <circle cx="10.5" cy="10.5" r="6.5" fill="none" stroke="currentColor" strokeWidth="2" />
      <path d="m15.5 15.5 4.2 4.2" fill="none" stroke="currentColor" strokeLinecap="round" strokeWidth="2" />
    </svg>
  );
}
