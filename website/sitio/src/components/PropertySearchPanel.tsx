"use client";

import { useState } from "react";
import { trackFunnelEvent } from "@/lib/analytics";
import { NeighborhoodOptions } from "./NeighborhoodOptions";

const operations = [
  { value: "", label: "Venta y arriendo" },
  { value: "Venta", label: "Comprar" },
  { value: "Arriendo", label: "Arrendar" },
] as const;

const propertyTypes = [
  { value: "", label: "Todos los inmuebles" },
  { value: "Apartamento", label: "Apartamento" },
  { value: "Casa", label: "Casa" },
  { value: "Oficina", label: "Oficina" },
  { value: "Local", label: "Local" },
  { value: "Bodega", label: "Bodega" },
  { value: "Lote", label: "Lote" },
  { value: "Finca", label: "Finca" },
] as const;

// Los topes de arriendo y venta viven en escalas distintas: mezclarlos confunde.
const rentPrices = [
  { value: "1500000", label: "Hasta $1,5 millones" },
  { value: "3000000", label: "Hasta $3 millones" },
  { value: "5000000", label: "Hasta $5 millones" },
  { value: "10000000", label: "Hasta $10 millones" },
] as const;

const salePrices = [
  { value: "300000000", label: "Hasta $300 millones" },
  { value: "500000000", label: "Hasta $500 millones" },
  { value: "1000000000", label: "Hasta $1.000 millones" },
  { value: "2000000000", label: "Hasta $2.000 millones" },
] as const;

function priceOptions(operation: string) {
  if (operation === "Arriendo") return rentPrices;
  if (operation === "Venta") return salePrices;
  return [...rentPrices, ...salePrices];
}

const bedroomOptions = [
  { value: "", label: "Cualquier distribución" },
  { value: "1", label: "1+ habitaciones" },
  { value: "2", label: "2+ habitaciones" },
  { value: "3", label: "3+ habitaciones" },
  { value: "4", label: "4+ habitaciones" },
] as const;

export function PropertySearchPanel() {
  const [operation, setOperation] = useState("");
  const [maxPrice, setMaxPrice] = useState("");

  return (
    <form
      className="property-search-panel"
      action="/inmuebles"
      method="get"
      aria-label="Buscar inmuebles"
      onSubmit={() => trackFunnelEvent("search")}
    >
      <div className="property-search-grid">
        <label className="property-search-field property-search-location">
          <span>Ubicación</span>
          <span className="property-search-control">
            <LocationIcon />
            <input name="neighborhood" type="search" placeholder="Barrio o sector" autoComplete="off" list="home-neighborhoods" />
            <NeighborhoodOptions id="home-neighborhoods" />
          </span>
        </label>

        <label className="property-search-field">
          <span>Operación</span>
          <select
            name="operation"
            value={operation}
            onChange={(event) => {
              setOperation(event.target.value);
              setMaxPrice("");
            }}
          >
            {operations.map((option) => <option key={option.label} value={option.value}>{option.label}</option>)}
          </select>
        </label>

        <label className="property-search-field">
          <span>Tipo de inmueble</span>
          <select name="propertyType" defaultValue="">
            {propertyTypes.map((option) => <option key={option.label} value={option.value}>{option.label}</option>)}
          </select>
        </label>

        <label className="property-search-field">
          <span>Presupuesto máximo</span>
          <select name="maxPrice" value={maxPrice} onChange={(event) => setMaxPrice(event.target.value)}>
            <option value="">Sin límite</option>
            {priceOptions(operation).map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
          </select>
        </label>

        <label className="property-search-field">
          <span>Habitaciones</span>
          <select name="bedrooms" defaultValue="">
            {bedroomOptions.map((option) => <option key={option.label} value={option.value}>{option.label}</option>)}
          </select>
        </label>

        <button className="property-search-submit" type="submit">
          <SearchIcon />
          <span>Buscar</span>
        </button>
      </div>
    </form>
  );
}

function LocationIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" focusable="false">
      <path fill="none" stroke="currentColor" strokeWidth="1.8" d="M12 21s6-5.2 6-11a6 6 0 1 0-12 0c0 5.8 6 11 6 11Z" />
      <circle cx="12" cy="10" r="2.2" fill="none" stroke="currentColor" strokeWidth="1.8" />
    </svg>
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
