import { useState } from "react"
import { Navigate } from "react-router-dom"
import {
  TrendingUp,
  Building2,
  Calendar,
  CheckCircle2,
  Percent,
  Calculator,
  Search,
  RotateCcw,
  Printer,
  Home,
  Tag,
  MapPin,
  Layers,
} from "lucide-react"
import { useMarketComparativeAnalysis } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
import { TIPOS_INMUEBLE } from "@/lib/constants"
import type { MarketAnalysisFilter } from "@/lib/types"

function formatMoney(amount: number, currency: string = "COP") {
  if (amount === 0) return "$0"
  return new Intl.NumberFormat("es-CO", {
    style: "currency",
    currency: currency || "COP",
    maximumFractionDigits: 0,
  }).format(amount)
}

function formatNumber(value: number) {
  return new Intl.NumberFormat("es-CO").format(value)
}

export function MercadoPage() {
  const user = useAuthStore((state) => state.user)

  // Filters state
  const [city, setCity] = useState("Medellin")
  const [neighborhood, setNeighborhood] = useState("")
  const [propertyType, setPropertyType] = useState("")
  const [operation, setOperation] = useState("Venta")
  const [targetAreaM2, setTargetAreaM2] = useState<number | "">("")
  const [bedrooms, setBedrooms] = useState<number | "">("")
  const [bathrooms, setBathrooms] = useState<number | "">("")

  // Active view tab for comparables
  const [activeTab, setActiveTab] = useState<"oferta" | "cierres">("oferta")

  const queryFilter: MarketAnalysisFilter = {
    city: city.trim() || undefined,
    neighborhood: neighborhood.trim() || undefined,
    propertyType: propertyType || undefined,
    operation: operation || undefined,
    targetAreaM2: typeof targetAreaM2 === "number" && targetAreaM2 > 0 ? targetAreaM2 : undefined,
    bedrooms: typeof bedrooms === "number" && bedrooms > 0 ? bedrooms : undefined,
    bathrooms: typeof bathrooms === "number" && bathrooms > 0 ? bathrooms : undefined,
  }

  const { data: analysis, isLoading, isError } = useMarketComparativeAnalysis(queryFilter)

  if (user?.rol !== "Administrador") {
    return <Navigate to="/dashboard" replace />
  }

  const handleResetFilters = () => {
    setCity("Medellin")
    setNeighborhood("")
    setPropertyType("")
    setOperation("Venta")
    setTargetAreaM2("")
    setBedrooms("")
    setBathrooms("")
  }

  const activeMetrics = analysis?.activeMetrics
  const closedMetrics = analysis?.closedMetrics
  const valuation = analysis?.valuationEstimate

  return (
    <div className="space-y-8 pb-12">
      {/* Header */}
      <header className="analytics-hero rounded-lg p-7 md:p-10">
        <div className="relative z-10 flex flex-wrap items-end justify-between gap-6">
          <div className="max-w-3xl">
            <span className="text-[.68rem] font-bold uppercase tracking-[.18em] text-[#e3be65]">
              Inteligencia de Mercado & ACM
            </span>
            <h1 className="mt-4 font-display text-[clamp(2.4rem,5vw,4.5rem)] leading-[.95] tracking-[-.04em] text-white">
              Análisis Comparativo de Mercado.
            </h1>
            <p className="mt-4 max-w-2xl text-sm leading-6 text-white/75">
              Avalúos algorítmicos basados en la oferta activa de la agencia y los negocios cerrados en el sector.
              Criterio cuantitativo para fijar precios de captación y asesorar a propietarios.
            </p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => window.print()}
              className="flex items-center gap-2 rounded-md border border-white/20 bg-white/10 px-4 py-2.5 text-xs font-semibold uppercase tracking-wider text-white transition hover:bg-white/20"
            >
              <Printer className="h-4 w-4" /> Imprimir informe
            </button>
          </div>
        </div>
      </header>

      {/* Filter Bar */}
      <section className="crm-panel p-6" aria-label="Filtros de análisis de mercado">
        <div className="mb-4 flex flex-wrap items-center justify-between gap-3 border-b border-border pb-3">
          <div className="flex items-center gap-2">
            <Search className="h-4 w-4 text-accent" />
            <h2 className="text-sm font-bold uppercase tracking-wider text-foreground">
              Parámetros de comparación
            </h2>
          </div>
          <button
            onClick={handleResetFilters}
            className="flex items-center gap-1.5 text-xs text-muted-foreground transition hover:text-foreground"
          >
            <RotateCcw className="h-3.5 w-3.5" /> Reestablecer filtros
          </button>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-7">
          <label className="text-xs font-medium text-muted-foreground">
            Ciudad
            <input
              type="text"
              value={city}
              onChange={(e) => setCity(e.target.value)}
              placeholder="Ej. Medellín"
              className="mt-1 block w-full rounded-md border border-input bg-card px-3 py-2 text-sm text-foreground focus:border-accent"
            />
          </label>

          <label className="text-xs font-medium text-muted-foreground">
            Barrio / Sector
            <input
              type="text"
              value={neighborhood}
              onChange={(e) => setNeighborhood(e.target.value)}
              placeholder="Ej. El Poblado, Laureles"
              className="mt-1 block w-full rounded-md border border-input bg-card px-3 py-2 text-sm text-foreground focus:border-accent"
            />
          </label>

          <label className="text-xs font-medium text-muted-foreground">
            Tipo de Inmueble
            <select
              value={propertyType}
              onChange={(e) => setPropertyType(e.target.value)}
              className="mt-1 block w-full rounded-md border border-input bg-card px-3 py-2 text-sm text-foreground"
            >
              <option value="">Todos los tipos</option>
              {TIPOS_INMUEBLE.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          </label>

          <label className="text-xs font-medium text-muted-foreground">
            Operación
            <select
              value={operation}
              onChange={(e) => setOperation(e.target.value)}
              className="mt-1 block w-full rounded-md border border-input bg-card px-3 py-2 text-sm text-foreground"
            >
              <option value="Venta">Venta</option>
              <option value="Arriendo">Arriendo</option>
            </select>
          </label>

          <label className="text-xs font-medium text-muted-foreground">
            Área objetivo (m²)
            <input
              type="number"
              min={1}
              value={targetAreaM2}
              onChange={(e) => setTargetAreaM2(e.target.value ? Number(e.target.value) : "")}
              placeholder="Ej. 85"
              className="mt-1 block w-full rounded-md border border-input bg-card px-3 py-2 text-sm text-foreground focus:border-accent"
            />
          </label>

          <label className="text-xs font-medium text-muted-foreground">
            Habitaciones mín.
            <select
              value={bedrooms}
              onChange={(e) => setBedrooms(e.target.value ? Number(e.target.value) : "")}
              className="mt-1 block w-full rounded-md border border-input bg-card px-3 py-2 text-sm text-foreground"
            >
              <option value="">Cualquiera</option>
              <option value="1">1+</option>
              <option value="2">2+</option>
              <option value="3">3+</option>
              <option value="4">4+</option>
            </select>
          </label>

          <label className="text-xs font-medium text-muted-foreground">
            Baños mín.
            <select
              value={bathrooms}
              onChange={(e) => setBathrooms(e.target.value ? Number(e.target.value) : "")}
              className="mt-1 block w-full rounded-md border border-input bg-card px-3 py-2 text-sm text-foreground"
            >
              <option value="">Cualquiera</option>
              <option value="1">1+</option>
              <option value="2">2+</option>
              <option value="3">3+</option>
            </select>
          </label>
        </div>
      </section>

      {/* Loading & Error States */}
      {isLoading && (
        <div className="crm-panel p-12 text-center text-sm text-muted-foreground">
          Calculando análisis de mercado y comparables…
        </div>
      )}

      {isError && (
        <div role="alert" className="crm-panel border-l-4 border-l-destructive p-8 text-sm text-destructive">
          Error al obtener los datos de mercado. Verifique la conexión con el servidor.
        </div>
      )}

      {analysis && !isLoading && (
        <>
          {/* Valuation Estimator Card (if target area provided) */}
          {valuation ? (
            <section
              className="relative overflow-hidden rounded-lg border-2 border-accent/40 bg-gradient-to-br from-card via-card to-accent/5 p-7 shadow-sm"
              aria-label="Estimación de avalúo comercial"
            >
              <div className="flex flex-wrap items-start justify-between gap-4">
                <div>
                  <span className="flex items-center gap-1.5 text-[.68rem] font-bold uppercase tracking-[.18em] text-accent">
                    <Calculator className="h-4 w-4" /> Estimación de Avalúo Comercial (ACM)
                  </span>
                  <h3 className="mt-2 font-display text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
                    {formatMoney(valuation.targetEstimate)}
                  </h3>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Valor sugerido de mercado para {valuation.targetAreaM2} m² a {formatMoney(valuation.referencePricePerM2)}/m²
                  </p>
                </div>

                <div className="flex items-center gap-2 rounded-full border border-border bg-background px-3.5 py-1.5 text-xs font-semibold">
                  <span>Confianza estadística:</span>
                  <span
                    className={`font-bold ${
                      valuation.confidenceLevel === "Alta"
                        ? "text-success"
                        : valuation.confidenceLevel === "Media"
                        ? "text-accent"
                        : "text-warning"
                    }`}
                  >
                    {valuation.confidenceLevel}
                  </span>
                </div>
              </div>

              {/* Range bar */}
              <div className="mt-6 grid gap-4 border-t border-border/60 pt-5 sm:grid-cols-3">
                <div className="rounded-md border border-border/80 bg-background/60 p-4">
                  <span className="text-[.68rem] font-bold uppercase tracking-wider text-muted-foreground">
                    Rango Bajo (-5%)
                  </span>
                  <p className="mt-1 font-display text-xl font-semibold text-foreground">
                    {formatMoney(valuation.lowEstimate)}
                  </p>
                  <p className="text-[.7rem] text-muted-foreground">Cierre rápido / Venta ágil</p>
                </div>

                <div className="rounded-md border-2 border-accent bg-accent/10 p-4">
                  <span className="text-[.68rem] font-bold uppercase tracking-wider text-accent">
                    Precio Sugerido (Punto Medio)
                  </span>
                  <p className="mt-1 font-display text-xl font-bold text-foreground">
                    {formatMoney(valuation.targetEstimate)}
                  </p>
                  <p className="text-[.7rem] text-muted-foreground">Valor óptimo de captación</p>
                </div>

                <div className="rounded-md border border-border/80 bg-background/60 p-4">
                  <span className="text-[.68rem] font-bold uppercase tracking-wider text-muted-foreground">
                    Rango Alto (+5%)
                  </span>
                  <p className="mt-1 font-display text-xl font-semibold text-foreground">
                    {formatMoney(valuation.highEstimate)}
                  </p>
                  <p className="text-[.7rem] text-muted-foreground">Margen de negociación inicial</p>
                </div>
              </div>

              <p className="mt-4 text-xs italic text-muted-foreground">{valuation.explanation}</p>
            </section>
          ) : (
            <div className="crm-panel flex items-center justify-between gap-4 border-l-4 border-l-accent p-5">
              <div className="flex items-center gap-3">
                <Calculator className="h-5 w-5 text-accent flex-shrink-0" />
                <div>
                  <h4 className="text-sm font-semibold text-foreground">
                    ¿Deseas calcular el avalúo de un inmueble específico?
                  </h4>
                  <p className="text-xs text-muted-foreground">
                    Ingresa los metros cuadrados en el campo «Área objetivo (m²)» para generar la estimación comercial de precio y rangos recomendados.
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Key Metrics Grid */}
          <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4" aria-label="Métricas clave de mercado">
            {/* Active Price / m2 */}
            <div className="crm-panel p-5">
              <div className="flex items-center justify-between text-muted-foreground">
                <span className="text-xs font-bold uppercase tracking-wider">Oferta Activa / m²</span>
                <Building2 className="h-4 w-4 text-accent" />
              </div>
              <p className="mt-3 font-display text-2xl font-bold text-foreground">
                {activeMetrics ? formatMoney(activeMetrics.medianPricePerM2) : "$0"}
              </p>
              <div className="mt-2 flex items-center justify-between text-xs text-muted-foreground">
                <span>Promedio: {activeMetrics ? formatMoney(activeMetrics.averagePricePerM2) : "$0"}</span>
                <span>Muestra: {activeMetrics?.sampleSize ?? 0}</span>
              </div>
            </div>

            {/* Closed Price / m2 */}
            <div className="crm-panel p-5">
              <div className="flex items-center justify-between text-muted-foreground">
                <span className="text-xs font-bold uppercase tracking-wider">Cierre Real / m²</span>
                <CheckCircle2 className="h-4 w-4 text-success" />
              </div>
              <p className="mt-3 font-display text-2xl font-bold text-foreground">
                {closedMetrics ? formatMoney(closedMetrics.medianPricePerM2) : "$0"}
              </p>
              <div className="mt-2 flex items-center justify-between text-xs text-muted-foreground">
                <span>Promedio: {closedMetrics ? formatMoney(closedMetrics.averagePricePerM2) : "$0"}</span>
                <span>Cierres: {closedMetrics?.sampleSize ?? 0}</span>
              </div>
            </div>

            {/* Days in market */}
            <div className="crm-panel p-5">
              <div className="flex items-center justify-between text-muted-foreground">
                <span className="text-xs font-bold uppercase tracking-wider">Tiempo de colocación</span>
                <Calendar className="h-4 w-4 text-info" />
              </div>
              <p className="mt-3 font-display text-2xl font-bold text-foreground">
                {closedMetrics && closedMetrics.sampleSize > 0
                  ? `${closedMetrics.averageDaysToClose} días`
                  : activeMetrics && activeMetrics.sampleSize > 0
                  ? `${activeMetrics.averageDaysOnMarket} días`
                  : "N/D"}
              </p>
              <div className="mt-2 flex items-center justify-between text-xs text-muted-foreground">
                <span>Activos: {activeMetrics?.averageDaysOnMarket ?? 0}d prom.</span>
                <span>Cierre: {closedMetrics?.averageDaysToClose ?? 0}d</span>
              </div>
            </div>

            {/* Discount margin */}
            <div className="crm-panel p-5">
              <div className="flex items-center justify-between text-muted-foreground">
                <span className="text-xs font-bold uppercase tracking-wider">Margen de Descuento</span>
                <Percent className="h-4 w-4 text-warning" />
              </div>
              <p className="mt-3 font-display text-2xl font-bold text-foreground">
                {closedMetrics?.averageDiscountPercentage !== null && closedMetrics?.averageDiscountPercentage !== undefined
                  ? `${closedMetrics.averageDiscountPercentage}%`
                  : "0.0%"}
              </p>
              <p className="mt-2 text-xs text-muted-foreground">
                Ajuste medio entre precio de lista y valor acordado en cierre
              </p>
            </div>
          </section>

          {/* Comparables Section with Tabs */}
          <section className="crm-panel overflow-hidden" aria-labelledby="comparables-heading">
            <header className="flex flex-wrap items-center justify-between gap-4 border-b border-border p-5">
              <div>
                <span className="crm-eyebrow">Comparables de Mercado</span>
                <h3 id="comparables-heading" className="mt-1 font-display text-xl font-bold">
                  Inmuebles y Negocios de Referencia
                </h3>
              </div>

              {/* Tabs */}
              <div className="flex rounded-md border border-border bg-muted/40 p-1">
                <button
                  onClick={() => setActiveTab("oferta")}
                  className={`rounded px-3.5 py-1.5 text-xs font-semibold transition ${
                    activeTab === "oferta"
                      ? "bg-card text-foreground shadow-sm"
                      : "text-muted-foreground hover:text-foreground"
                  }`}
                >
                  Oferta Activa ({analysis.activeComparables.length})
                </button>
                <button
                  onClick={() => setActiveTab("cierres")}
                  className={`rounded px-3.5 py-1.5 text-xs font-semibold transition ${
                    activeTab === "cierres"
                      ? "bg-card text-foreground shadow-sm"
                      : "text-muted-foreground hover:text-foreground"
                  }`}
                >
                  Cierres Históricos ({analysis.closedComparables.length})
                </button>
              </div>
            </header>

            {/* Table: Active Comparables */}
            {activeTab === "oferta" && (
              <div className="overflow-x-auto">
                {analysis.activeComparables.length === 0 ? (
                  <div className="p-8 text-center text-sm text-muted-foreground">
                    No se encontraron inmuebles activos con los criterios seleccionados.
                  </div>
                ) : (
                  <table className="w-full text-left text-sm">
                    <thead className="border-b border-border bg-muted/30 text-[.7rem] uppercase tracking-wider text-muted-foreground">
                      <tr>
                        <th className="px-5 py-3">Inmueble</th>
                        <th className="px-4 py-3">Tipo / Operación</th>
                        <th className="px-4 py-3">Ubicación</th>
                        <th className="px-4 py-3 text-right">Área</th>
                        <th className="px-4 py-3 text-center">Distribución</th>
                        <th className="px-4 py-3 text-right">Precio Lista</th>
                        <th className="px-4 py-3 text-right">Precio / m²</th>
                        <th className="px-4 py-3 text-right">Días</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {analysis.activeComparables.map((item) => (
                        <tr key={item.id} className="transition hover:bg-muted/20">
                          <td className="px-5 py-3 font-medium text-foreground">
                            {item.title}
                          </td>
                          <td className="px-4 py-3">
                            <span className="inline-flex items-center gap-1 text-xs">
                              <span className="font-semibold text-foreground">{item.propertyType}</span>
                              <span className="text-muted-foreground">• {item.operation}</span>
                            </span>
                          </td>
                          <td className="px-4 py-3 text-xs text-muted-foreground">
                            <span className="flex items-center gap-1">
                              <MapPin className="h-3.5 w-3.5 text-muted-foreground/70" />
                              {item.neighborhood ? `${item.neighborhood}, ${item.city}` : item.city}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-right font-mono text-xs font-medium">
                            {item.areaM2 > 0 ? `${item.areaM2} m²` : "N/D"}
                          </td>
                          <td className="px-4 py-3 text-center text-xs text-muted-foreground">
                            {item.bedrooms}H / {item.bathrooms}B
                          </td>
                          <td className="px-4 py-3 text-right font-mono text-xs font-bold text-foreground">
                            {formatMoney(item.askingPrice, item.currency)}
                          </td>
                          <td className="px-4 py-3 text-right font-mono text-xs text-accent font-semibold">
                            {formatMoney(item.pricePerM2, item.currency)}
                          </td>
                          <td className="px-4 py-3 text-right text-xs text-muted-foreground">
                            {item.daysOnMarket} d
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            )}

            {/* Table: Closed Deals Comparables */}
            {activeTab === "cierres" && (
              <div className="overflow-x-auto">
                {analysis.closedComparables.length === 0 ? (
                  <div className="p-8 text-center text-sm text-muted-foreground">
                    No se encontraron negocios cerrados con los criterios seleccionados.
                  </div>
                ) : (
                  <table className="w-full text-left text-sm">
                    <thead className="border-b border-border bg-muted/30 text-[.7rem] uppercase tracking-wider text-muted-foreground">
                      <tr>
                        <th className="px-5 py-3">Inmueble Negociado</th>
                        <th className="px-4 py-3">Tipo / Negocio</th>
                        <th className="px-4 py-3">Ubicación</th>
                        <th className="px-4 py-3 text-right">Área</th>
                        <th className="px-4 py-3 text-center">Distribución</th>
                        <th className="px-4 py-3 text-right">Precio Cierre</th>
                        <th className="px-4 py-3 text-right">Precio / m²</th>
                        <th className="px-4 py-3 text-right">Días Cierre</th>
                        <th className="px-4 py-3 text-right">Fecha Cierre</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {analysis.closedComparables.map((item) => (
                        <tr key={item.dealId} className="transition hover:bg-muted/20">
                          <td className="px-5 py-3 font-medium text-foreground">
                            {item.title}
                          </td>
                          <td className="px-4 py-3">
                            <span className="inline-flex items-center gap-1 text-xs">
                              <span className="font-semibold text-foreground">{item.propertyType}</span>
                              <span className="text-muted-foreground">• {item.operation}</span>
                            </span>
                          </td>
                          <td className="px-4 py-3 text-xs text-muted-foreground">
                            <span className="flex items-center gap-1">
                              <MapPin className="h-3.5 w-3.5 text-muted-foreground/70" />
                              {item.neighborhood ? `${item.neighborhood}, ${item.city}` : item.city}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-right font-mono text-xs font-medium">
                            {item.areaM2 > 0 ? `${item.areaM2} m²` : "N/D"}
                          </td>
                          <td className="px-4 py-3 text-center text-xs text-muted-foreground">
                            {item.bedrooms}H / {item.bathrooms}B
                          </td>
                          <td className="px-4 py-3 text-right font-mono text-xs font-bold text-success">
                            {formatMoney(item.closedPrice, item.currency)}
                          </td>
                          <td className="px-4 py-3 text-right font-mono text-xs text-accent font-semibold">
                            {formatMoney(item.pricePerM2, item.currency)}
                          </td>
                          <td className="px-4 py-3 text-right text-xs text-muted-foreground">
                            {item.daysToClose} d
                          </td>
                          <td className="px-4 py-3 text-right text-xs text-muted-foreground font-mono">
                            {item.closedOn}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            )}
          </section>
        </>
      )}
    </div>
  )
}
