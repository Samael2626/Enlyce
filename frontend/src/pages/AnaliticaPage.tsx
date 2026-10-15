import { useState } from "react"
import { Navigate } from "react-router-dom"
import { ArrowDownRight, BarChart3, MousePointerClick } from "lucide-react"
import { api } from "@/api/client"
import { useAdvisors, useAnalyticsFunnel, useContacts, useCreateRealizedDeal, useCrmReportMetrics, useFirstResponseMetrics, useInmuebles, useInventoryAnalytics, useRealizedDealsReport } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"

const labels: Record<string, string> = {
  page_view: "Visitas",
  search: "Búsquedas",
  favorite: "Favoritos",
  form_started: "Formularios iniciados",
  conversion: "Oportunidades creadas",
}

export function AnaliticaPage() {
  const [days, setDays] = useState(30)
  const [crmFrom, setCrmFrom] = useState(() => dateOffset(-29))
  const [crmTo, setCrmTo] = useState(() => dateOffset(0))
  const [dealFrom, setDealFrom] = useState(() => dateOffset(-29))
  const [dealTo, setDealTo] = useState(() => dateOffset(0))
  const [dealAdvisorId, setDealAdvisorId] = useState("")
  const [dealOperation, setDealOperation] = useState("")
  const [dealPropertyType, setDealPropertyType] = useState("")
  const [dealPage, setDealPage] = useState(1)
  const user = useAuthStore((state) => state.user)
  const funnel = useAnalyticsFunnel(days)
  const crmReport = useCrmReportMetrics(crmFrom, crmTo)
  const responseMetrics = useFirstResponseMetrics(crmFrom, crmTo)
  const inventory = useInventoryAnalytics()
  const advisors = useAdvisors()
  const dealReport = useRealizedDealsReport({
    from: dealFrom, to: dealTo, advisorId: dealAdvisorId,
    operation: dealOperation, propertyType: dealPropertyType, page: dealPage, pageSize: 10,
  })

  if (user?.rol !== "Administrador") return <Navigate to="/dashboard" replace />

  const steps = funnel.data?.steps ?? []
  const visits = steps.find((step) => step.event === "page_view")?.uniqueSessions ?? 0
  const conversions = steps.find((step) => step.event === "conversion")?.uniqueSessions ?? 0
  const conversionRate = visits === 0 ? 0 : Math.round((conversions * 1000) / visits) / 10
  return (
    <div className="space-y-8">
      <header className="analytics-hero rounded-lg p-7 md:p-10">
        <div className="relative z-10 max-w-3xl">
          <span className="text-[.68rem] font-bold uppercase tracking-[.18em] text-[#e3be65]">Lectura comercial</span>
          <h1 className="mt-4 font-display text-[clamp(2.7rem,6vw,5.4rem)] leading-[.9] tracking-[-.05em]">Del interés a la oportunidad.</h1>
          <p className="mt-5 max-w-xl text-sm leading-6 text-white/72">Sesiones anónimas de la web pública. Sin nombres, correos, teléfonos ni vigilancia decorativa.</p>
        </div>
      </header>

      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Embudo web</span>
          <h2 className="mt-2 font-display text-3xl tracking-tight">Últimos {days} días</h2>
        </div>
        <label className="text-xs font-bold uppercase tracking-[.1em] text-muted-foreground">
          Periodo
          <select className="ml-3 rounded-sm border border-input bg-card px-3 py-2 text-foreground" value={days} onChange={(event) => setDays(Number(event.target.value))}>
            <option value={7}>7 días</option>
            <option value={30}>30 días</option>
            <option value={90}>90 días</option>
          </select>
        </label>
      </div>

      {funnel.isLoading ? (
        <div className="crm-panel p-8 text-sm text-muted-foreground">Calculando el embudo…</div>
      ) : funnel.isError ? (
        <div role="alert" className="crm-panel border-l-4 border-l-destructive p-8 text-sm text-destructive">No se pudo cargar la analítica.</div>
      ) : (
        <>
          <section className="grid gap-4 sm:grid-cols-3" aria-label="Indicadores del embudo">
            <Metric label="Sesiones con visita" value={visits} icon={<MousePointerClick />} />
            <Metric label="Oportunidades creadas" value={conversions} icon={<BarChart3 />} />
            <Metric label="Conversión visita → oportunidad" value={`${conversionRate}%`} icon={<ArrowDownRight />} />
          </section>

          <section className="crm-panel overflow-hidden" aria-labelledby="funnel-title">
            <header className="border-b border-border p-6">
              <span className="crm-eyebrow">Recorrido</span>
              <h2 id="funnel-title" className="mt-2 font-display text-2xl">Sesiones únicas por acción</h2>
            </header>
            <ol className="analytics-funnel-list">
              {steps.map((step, index) => (
                <li key={step.event}>
                  <span>{String(index + 1).padStart(2, "0")}</span>
                  <div>
                    <strong>{labels[step.event] ?? step.event}</strong>
                    <small>{step.totalEvents} eventos totales</small>
                  </div>
                  <div className="analytics-funnel-bar" aria-hidden="true"><i style={{ width: `${Math.min(100, step.rateFromVisits)}%` }} /></div>
                  <b>{step.uniqueSessions}</b>
                  <em>{step.rateFromVisits}%</em>
                </li>
              ))}
            </ol>
          </section>
        </>
      )}

      <section className="space-y-4" aria-labelledby="inventory-report-title">
        <div>
          <span className="crm-eyebrow">Inventario</span>
          <h2 id="inventory-report-title" className="mt-2 font-display text-3xl tracking-tight">Propiedades disponibles</h2>
          <p className="mt-2 text-sm text-muted-foreground">Cuenta propiedades activas en estado disponible o reservado. Una propiedad en venta y arriendo cuenta en ambas modalidades.</p>
        </div>
        {inventory.isLoading ? (
          <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando inventario…</div>
        ) : inventory.isError ? (
          <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el inventario.</div>
        ) : (
          <div className="grid gap-4 lg:grid-cols-3">
            <div className="crm-panel p-5"><span className="text-xs text-muted-foreground">Total disponible</span><b className="mt-1 block font-display text-3xl">{inventory.data?.totalProperties ?? 0}</b></div>
            <InventoryBreakdown title="Por tipo de inmueble" rows={inventory.data?.byPropertyType ?? []} />
            <InventoryBreakdown title="Por modalidad" rows={inventory.data?.byOperation ?? []} />
          </div>
        )}
      </section>

      <RealizedDealsSection
        report={dealReport.data}
        isLoading={dealReport.isLoading}
        isError={dealReport.isError}
        advisors={advisors.data ?? []}
        filters={{
          from: dealFrom, to: dealTo, advisorId: dealAdvisorId,
          operation: dealOperation, propertyType: dealPropertyType,
        }}
        onFilterChange={(key, value) => {
          setDealPage(1)
          if (key === "from") setDealFrom(value)
          if (key === "to") setDealTo(value)
          if (key === "advisorId") setDealAdvisorId(value)
          if (key === "operation") setDealOperation(value)
          if (key === "propertyType") setDealPropertyType(value)
        }}
        page={dealPage}
        onPageChange={setDealPage}
      />

      <section className="space-y-4" aria-labelledby="crm-report-title">
        <div className="flex flex-wrap items-end justify-between gap-4">
          <div>
          <span className="crm-eyebrow">Actividad del CRM</span>
          <h2 id="crm-report-title" className="mt-2 font-display text-3xl tracking-tight">Rendimiento comercial</h2>
          <p className="mt-2 text-sm text-muted-foreground">Leads creados en el rango (inclusive); asesor y etapa actuales. Respondidas: primer contacto registrado / leads. Tiempo: creación a primer contacto. Visitas: citas programadas en el rango para esos leads. Cierres: leads de la cohorte que hoy están cerrados.</p>
          </div>
          <div className="flex flex-wrap gap-3 text-xs font-bold text-muted-foreground">
            <label>Desde <input aria-label="Desde" type="date" value={crmFrom} max={crmTo} onChange={(event) => setCrmFrom(event.target.value)} className="ml-2 rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
            <label>Hasta <input aria-label="Hasta" type="date" value={crmTo} min={crmFrom} onChange={(event) => setCrmTo(event.target.value)} className="ml-2 rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
          </div>
        </div>
        {crmReport.isLoading ? (
          <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando actividad comercial…</div>
        ) : crmReport.isError ? (
          <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el reporte del CRM.</div>
        ) : (
          <div className="grid gap-4 lg:grid-cols-2">
            {responseMetrics.isError && <p role="alert" className="text-sm text-destructive lg:col-span-2">No se pudieron cargar las métricas de primera respuesta.</p>}
            <div className="crm-panel grid grid-cols-2 gap-4 p-5 sm:grid-cols-3 lg:col-span-2">
              <ReportTotal label="Oportunidades" value={crmReport.data?.leads ?? 0} />
              <ReportTotal label="Respondidas" value={`${crmReport.data?.respondedLeads ?? 0} / ${crmReport.data?.leads ?? 0}`} />
              <ReportTotal label="Visitas programadas" value={crmReport.data?.visits ?? 0} />
              <ReportTotal label="Cierres ganados" value={crmReport.data?.wonLeads ?? 0} />
              <ReportTotal label="Cierres perdidos" value={crmReport.data?.lostLeads ?? 0} />
            </div>
            <CrmBreakdown title="Asesor actual" rows={crmReport.data?.byAdvisor ?? []} />
            <CrmBreakdown title="Etapa actual" rows={crmReport.data?.byStage ?? []} />
            <CrmBreakdown title="Origen" rows={crmReport.data?.bySource ?? []} />
            <CrmBreakdown title="Campaña" rows={crmReport.data?.byCampaign ?? []} />
            <CrmBreakdown title="Tipo de operación" rows={crmReport.data?.byOperation ?? []} />
            <div className="crm-panel p-5">
              <span className="text-[.68rem] font-bold uppercase tracking-[.1em] text-muted-foreground">Primera respuesta del asesor</span>
              <p className="mt-5 font-display text-3xl">{responseMetrics.isLoading ? "Cargando…" : responseMetrics.isError ? "No disponible" : responseMetrics.data?.averageHours == null ? "Sin datos" : `${responseMetrics.data.averageHours.toFixed(1)} h`}</p>
              <p className="mt-2 text-sm text-muted-foreground">Promedio de {responseMetrics.data?.respondedLeads ?? 0} oportunidades con respuesta registrada.</p>
            </div>
            <ResponseBreakdown title="Primera respuesta por asesor" rows={responseMetrics.data?.byAdvisor ?? []} isLoading={responseMetrics.isLoading} />
            <ResponseBreakdown title="Primera respuesta por origen" rows={responseMetrics.data?.bySource ?? []} isLoading={responseMetrics.isLoading} />
          </div>
        )}
      </section>
    </div>
  )
}

function RealizedDealsSection({
  report,
  isLoading,
  isError,
  advisors,
  filters,
  onFilterChange,
  page,
  onPageChange,
}: {
  report?: import("@/lib/types").RealizedDealReportPage
  isLoading: boolean
  isError: boolean
  advisors: { id: string; name: string }[]
  filters: { from: string; to: string; advisorId: string; operation: string; propertyType: string }
  onFilterChange: (key: "from" | "to" | "advisorId" | "operation" | "propertyType", value: string) => void
  page: number
  onPageChange: (page: number) => void
}) {
  const [showForm, setShowForm] = useState(false)
  const [exportError, setExportError] = useState("")
  const [exporting, setExporting] = useState(false)
  const maxPage = Math.max(1, Math.ceil((report?.total ?? 0) / (report?.pageSize ?? 10)))

  async function exportXlsx() {
    setExporting(true)
    setExportError("")
    try {
      const blob = await api.downloadRealizedDealsXlsx(filters)
      const url = URL.createObjectURL(blob)
      const link = document.createElement("a")
      link.href = url
      link.download = "negocios-realizados.xlsx"
      link.click()
      window.setTimeout(() => URL.revokeObjectURL(url), 1_000)
    } catch (error) {
      setExportError(error instanceof Error ? error.message : "No se pudo exportar el reporte.")
    } finally {
      setExporting(false)
    }
  }

  async function exportPdf() {
    const popup = window.open("", "_blank")
    if (!popup) {
      setExportError("El navegador bloqueó la ventana de impresión.")
      return
    }
    setExporting(true)
    setExportError("")
    popup.document.write("<p style='font:16px sans-serif;padding:24px'>Preparando reporte para imprimir…</p>")
    try {
      const exportFilters = { ...filters, pageSize: 5_000 }
      const firstPage = await api.getRealizedDealsReport({ ...exportFilters, page: 1 })
      if (firstPage.total > 50_000) throw new Error("El reporte supera el límite de 50.000 negocios para imprimir.")
      const items = [...firstPage.items]
      const pageCount = Math.ceil(firstPage.total / firstPage.pageSize)
      for (let pageNumber = 2; pageNumber <= pageCount; pageNumber++) {
        const result = await api.getRealizedDealsReport({ ...exportFilters, page: pageNumber })
        items.push(...result.items)
      }
      popup.document.open()
      popup.document.write(buildPrintableDealsReport(items, firstPage.totalsByCurrency, filters, advisors))
      popup.document.close()
      popup.focus()
      window.setTimeout(() => popup.print(), 300)
    } catch (error) {
      popup.close()
      setExportError(error instanceof Error ? error.message : "No se pudo preparar el reporte PDF.")
    } finally {
      setExporting(false)
    }
  }

  return (
    <section className="space-y-4" aria-labelledby="realized-deals-title">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Negocios realizados</span>
          <h2 id="realized-deals-title" className="mt-2 font-display text-3xl tracking-tight">Cierres, valor y comisión</h2>
        </div>
        <div className="flex flex-wrap gap-2">
          <button type="button" disabled={exporting} className="rounded-sm border border-border px-4 py-2 text-sm font-semibold disabled:opacity-50" onClick={exportXlsx}>{exporting ? "Preparando…" : "Excel"}</button>
          <button type="button" disabled={exporting} className="rounded-sm border border-border px-4 py-2 text-sm font-semibold disabled:opacity-50" onClick={exportPdf}>PDF</button>
          <button type="button" className="rounded-sm bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground" onClick={() => setShowForm((value) => !value)}>
            {showForm ? "Cerrar formulario" : "Registrar negocio"}
          </button>
        </div>
      </div>
      {exportError && <p role="alert" className="text-sm text-destructive">{exportError}</p>}
      {showForm && <RealizedDealForm advisors={advisors} />}

      <div className="crm-panel grid gap-3 p-5 sm:grid-cols-2 lg:grid-cols-5">
        <label className="text-xs font-bold text-muted-foreground">Desde<input aria-label="Desde cierre" type="date" value={filters.from} max={filters.to} onChange={(event) => onFilterChange("from", event.target.value)} className="mt-1 block w-full rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
        <label className="text-xs font-bold text-muted-foreground">Hasta<input aria-label="Hasta cierre" type="date" value={filters.to} min={filters.from} onChange={(event) => onFilterChange("to", event.target.value)} className="mt-1 block w-full rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
        <label className="text-xs font-bold text-muted-foreground">Asesor<select value={filters.advisorId} onChange={(event) => onFilterChange("advisorId", event.target.value)} className="mt-1 block w-full rounded-sm border border-input bg-card px-2 py-2 text-foreground"><option value="">Todos</option>{advisors.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}</select></label>
        <label className="text-xs font-bold text-muted-foreground">Negocio<select value={filters.operation} onChange={(event) => onFilterChange("operation", event.target.value)} className="mt-1 block w-full rounded-sm border border-input bg-card px-2 py-2 text-foreground"><option value="">Todos</option><option value="Sale">Venta</option><option value="Rent">Arriendo</option></select></label>
        <label className="text-xs font-bold text-muted-foreground">Tipo de inmueble<select value={filters.propertyType} onChange={(event) => onFilterChange("propertyType", event.target.value)} className="mt-1 block w-full rounded-sm border border-input bg-card px-2 py-2 text-foreground"><option value="">Todos</option>{["Apartamento", "Casa", "Local", "Lote", "Oficina", "Bodega", "Finca"].map((type) => <option key={type} value={type}>{type}</option>)}</select></label>
      </div>

      {isLoading ? <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando negocios…</div> : isError ? (
        <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el reporte de negocios.</div>
      ) : (
        <>
          <div className="grid gap-3 sm:grid-cols-2">
            {(report?.totalsByCurrency ?? []).map((total) => (
              <div key={total.currency} className="crm-panel grid grid-cols-3 gap-3 p-5">
                <ReportTotal label={`Negocios (${total.currency})`} value={total.count} />
                <ReportTotal label="Total bruto" value={formatMoney(total.grossAmount, total.currency)} />
                <ReportTotal label="Comisión" value={formatMoney(total.commissionAmount, total.currency)} />
              </div>
            ))}
          </div>
          <div className="crm-panel overflow-x-auto">
            <table className="w-full min-w-[850px] text-left text-sm">
              <thead className="border-b border-border bg-muted/50 text-muted-foreground"><tr>{["Inmueble", "Negocio", "Fecha", "Encargado", "Cliente", "Total bruto", "Comisión"].map((heading) => <th key={heading} className="px-4 py-3 font-semibold">{heading}</th>)}</tr></thead>
              <tbody className="divide-y divide-border">{(report?.items ?? []).map((item) => <tr key={item.id}>
                <td className="px-4 py-3">{item.propertyName}<small className="block text-muted-foreground">{item.propertyType}</small></td>
                <td className="px-4 py-3">{item.operation === "Sale" ? "Venta" : "Arriendo"}</td>
                <td className="px-4 py-3">{item.closedOn}</td>
                <td className="px-4 py-3">{item.advisorName}</td>
                <td className="px-4 py-3">{item.clientName}</td>
                <td className="px-4 py-3">{formatMoney(item.grossAmount, item.currency)}</td>
                <td className="px-4 py-3">{formatMoney(item.commissionAmount, item.currency)}</td>
              </tr>)}</tbody>
            </table>
            {report?.items.length === 0 && <p className="p-6 text-sm text-muted-foreground">No hay negocios en este periodo.</p>}
            <div className="flex items-center justify-between border-t border-border p-4 text-sm text-muted-foreground">
              <span>{report?.total ?? 0} negocios · página {page} de {maxPage}</span>
              <div className="flex gap-2"><button type="button" disabled={page <= 1} onClick={() => onPageChange(page - 1)} className="rounded-sm border border-border px-3 py-1 disabled:opacity-40">Anterior</button><button type="button" disabled={page >= maxPage} onClick={() => onPageChange(page + 1)} className="rounded-sm border border-border px-3 py-1 disabled:opacity-40">Siguiente</button></div>
            </div>
          </div>
        </>
      )}
    </section>
  )
}

function buildPrintableDealsReport(
  items: import("@/lib/types").RealizedDealReportPage["items"],
  totals: import("@/lib/types").RealizedDealReportPage["totalsByCurrency"],
  filters: { from: string; to: string; advisorId: string; operation: string; propertyType: string },
  advisors: { id: string; name: string }[],
) {
  const escape = (value: string) => value.replace(/[&<>"']/g, (character) => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;",
  })[character]!)
  const currency = (amount: number, code: string) => `${amount.toLocaleString("es-CO", { maximumFractionDigits: 2 })} ${escape(code)}`
  const advisorName = filters.advisorId ? advisors.find((advisor) => advisor.id === filters.advisorId)?.name ?? "Asesor" : "Todos"
  const operationName = filters.operation === "Sale" ? "Venta" : filters.operation === "Rent" ? "Arriendo" : "Todos"
  const rows = items.map((item) => `<tr><td>${escape(item.propertyName)}</td><td>${escape(item.propertyType)}</td><td>${item.operation === "Sale" ? "Venta" : "Arriendo"}</td><td>${escape(item.closedOn)}</td><td>${escape(item.advisorName)}</td><td>${escape(item.clientName)}</td><td class="number">${currency(item.grossAmount, item.currency)}</td><td class="number">${currency(item.commissionAmount, item.currency)}</td></tr>`).join("")
  const summary = totals.map((item) => `<p><b>${item.count} negocios · ${escape(item.currency)}</b> — Total bruto: ${currency(item.grossAmount, item.currency)} · Comisión: ${currency(item.commissionAmount, item.currency)}</p>`).join("")
  return `<!doctype html><html lang="es"><head><meta charset="utf-8"><title>Negocios realizados</title><style>
    @page{size:A4 landscape;margin:12mm}body{font:11px Arial,sans-serif;color:#202633}h1{font-size:22px;margin:0 0 8px}p{margin:4px 0}.meta{color:#596273;margin-bottom:14px}table{border-collapse:collapse;width:100%;margin-top:12px}thead{display:table-header-group}th,td{border:1px solid #cbd1dc;padding:6px;text-align:left;vertical-align:top}th{background:#e9edf4}.number{text-align:right;white-space:nowrap}tr{page-break-inside:avoid}
    </style></head><body><h1>Negocios realizados</h1><div class="meta">Periodo: ${escape(filters.from)} a ${escape(filters.to)} · Asesor: ${escape(advisorName)} · Operación: ${operationName} · Tipo: ${escape(filters.propertyType || "Todos")} · Filas: ${items.length}</div>${summary}<table><thead><tr><th>Inmueble</th><th>Tipo</th><th>Negocio</th><th>Fecha</th><th>Encargado</th><th>Cliente</th><th>Total bruto</th><th>Comisión</th></tr></thead><tbody>${rows}</tbody></table></body></html>`
}

function RealizedDealForm({ advisors }: { advisors: { id: string; name: string }[] }) {
  const [contactSearch, setContactSearch] = useState("")
  const [propertySearch, setPropertySearch] = useState("")
  const [operation, setOperation] = useState<"Sale" | "Rent">("Sale")
  const [errorMessage, setErrorMessage] = useState("")
  const contacts = useContacts({ q: contactSearch, page: 1, pageSize: 50 })
  const properties = useInmuebles({ search: propertySearch, page: 1, pageSize: 50 })
  const createDeal = useCreateRealizedDeal()
  const user = useAuthStore((state) => state.user)
  const isAdmin = user?.rol === "Administrador"

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const formElement = event.currentTarget
    setErrorMessage("")
    const form = new FormData(formElement)
    const grossAmount = Number(form.get("grossAmount"))
    const commissionAmount = Number(form.get("commissionAmount"))
    if (!Number.isFinite(grossAmount) || !Number.isFinite(commissionAmount)) {
      setErrorMessage("Ingresa valores numéricos válidos.")
      return
    }
    try {
      await createDeal.mutateAsync({
        propertyId: String(form.get("propertyId")),
        contactId: String(form.get("contactId")),
        advisorId: isAdmin ? String(form.get("advisorId")) : null,
        operation,
        grossAmount,
        commissionAmount,
        currency: String(form.get("currency")),
        closedOn: String(form.get("closedOn")),
      })
      formElement.reset()
      setPropertySearch("")
      setContactSearch("")
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "No se pudo registrar el negocio.")
    }
  }

  const availableProperties = (properties.data?.items ?? []).filter((property) =>
    ["Disponible", "Reservado"].includes(property.estado) &&
    (operation === "Sale" ? ["Venta", "VentaYArriendo"].includes(property.modalidad) : ["Arriendo", "VentaYArriendo"].includes(property.modalidad)),
  )

  return (
    <form onSubmit={submit} className="crm-panel grid gap-4 p-5 md:grid-cols-2 lg:grid-cols-4">
      <label className="text-xs font-bold text-muted-foreground">Tipo de negocio<select value={operation} onChange={(event) => setOperation(event.target.value as "Sale" | "Rent")} className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground"><option value="Sale">Venta</option><option value="Rent">Arriendo</option></select></label>
      <label className="text-xs font-bold text-muted-foreground">Buscar inmueble<input value={propertySearch} onChange={(event) => setPropertySearch(event.target.value)} className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground" /></label>
      <label className="text-xs font-bold text-muted-foreground">Inmueble<select name="propertyId" required defaultValue="" className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground"><option value="" disabled>Selecciona</option>{availableProperties.map((property) => <option key={property.id} value={property.id}>{property.nombre} · {property.ciudad}</option>)}</select></label>
      <label className="text-xs font-bold text-muted-foreground">Buscar cliente<input value={contactSearch} onChange={(event) => setContactSearch(event.target.value)} className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground" /></label>
      <label className="text-xs font-bold text-muted-foreground">Cliente<select name="contactId" required defaultValue="" className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground"><option value="" disabled>Selecciona</option>{(contacts.data?.items ?? []).map((contact) => <option key={contact.id} value={contact.id}>{contact.name} {contact.surname ?? ""}</option>)}</select></label>
      {isAdmin && <label className="text-xs font-bold text-muted-foreground">Asesor<select name="advisorId" required defaultValue="" className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground"><option value="" disabled>Selecciona</option>{advisors.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}</select></label>}
      <label className="text-xs font-bold text-muted-foreground">Fecha de cierre<input name="closedOn" type="date" required defaultValue={dateOffset(0)} className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground" /></label>
      <label className="text-xs font-bold text-muted-foreground">Valor bruto<input name="grossAmount" type="number" min="0.01" step="0.01" required className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground" /></label>
      <label className="text-xs font-bold text-muted-foreground">Comisión<input name="commissionAmount" type="number" min="0" step="0.01" required className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-foreground" /></label>
      <label className="text-xs font-bold text-muted-foreground">Moneda<input name="currency" required minLength={3} maxLength={3} defaultValue="COP" className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 uppercase text-foreground" /></label>
      <div className="flex items-end"><button type="submit" disabled={createDeal.isPending} className="rounded-sm bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">{createDeal.isPending ? "Guardando…" : "Guardar cierre"}</button></div>
      {errorMessage && <p role="alert" className="text-sm text-destructive md:col-span-2 lg:col-span-4">{errorMessage}</p>}
      {createDeal.isSuccess && <p role="status" className="text-sm text-green-700 md:col-span-2 lg:col-span-4">Negocio registrado; el inmueble ya no aparece como disponible.</p>}
    </form>
  )
}

function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat("es-CO", { style: "currency", currency, maximumFractionDigits: 2 }).format(amount)
}

function dateOffset(days: number) {
  const date = new Date()
  date.setUTCDate(date.getUTCDate() + days)
  return date.toISOString().slice(0, 10)
}

function ResponseBreakdown({ title, rows, isLoading }: { title: string; rows: { name: string; respondedLeads: number; averageHours: number }[]; isLoading: boolean }) {
  return (
    <section className="crm-panel overflow-hidden" aria-label={title}>
      <h3 className="border-b border-border p-5 font-display text-xl">{title}</h3>
      {rows.length === 0 ? <p className="p-5 text-sm text-muted-foreground">{isLoading ? "Cargando respuestas…" : "Sin respuestas registradas en el periodo."}</p> : (
        <ul className="divide-y divide-border">{rows.map((row) => <li key={row.name} className="flex items-center justify-between gap-4 px-5 py-3 text-sm"><span className="truncate">{row.name}</span><span className="text-right"><b>{row.averageHours.toFixed(1)} h</b><small className="block text-muted-foreground">{row.respondedLeads} respondidas</small></span></li>)}</ul>
      )}
    </section>
  )
}

function CrmBreakdown({ title, rows }: { title: string; rows: { name: string; leads: number; respondedLeads: number; averageResponseHours: number | null; visits: number; wonLeads: number; lostLeads: number }[] }) {
  return (
    <section className="crm-panel overflow-hidden" aria-label={`Reporte por ${title.toLowerCase()}`}>
      <h3 className="border-b border-border p-5 font-display text-xl">{title}</h3>
      {rows.length === 0 ? <p className="p-5 text-sm text-muted-foreground">Aún no hay oportunidades.</p> : (
        <ul className="divide-y divide-border">
          {rows.map((row) => <li key={row.name} className="flex items-start justify-between gap-4 px-5 py-3 text-sm"><span className="truncate">{row.name}</span><span className="text-right"><b>{row.leads} leads</b><small className="block text-muted-foreground">{row.respondedLeads}/{row.leads} respondidas · {row.averageResponseHours == null ? "sin tiempo" : `${row.averageResponseHours.toFixed(1)} h`} · {row.visits} visitas · {row.wonLeads} ganados · {row.lostLeads} perdidos</small></span></li>)}
        </ul>
      )}
    </section>
  )
}

function ReportTotal({ label, value }: { label: string; value: number | string }) {
  return <div><span className="text-xs text-muted-foreground">{label}</span><b className="mt-1 block font-display text-2xl">{value}</b></div>
}

function InventoryBreakdown({ title, rows }: { title: string; rows: { name: string; count: number }[] }) {
  return (
    <section className="crm-panel overflow-hidden" aria-label={title}>
      <h3 className="border-b border-border p-5 font-display text-xl">{title}</h3>
      {rows.length === 0 ? <p className="p-5 text-sm text-muted-foreground">Sin propiedades disponibles.</p> : (
        <ul className="divide-y divide-border">{rows.map((row) => <li key={row.name} className="flex items-center justify-between gap-4 px-5 py-3 text-sm"><span>{row.name}</span><b>{row.count}</b></li>)}</ul>
      )}
    </section>
  )
}

function Metric({ label, value, icon }: { label: string; value: number | string; icon: React.ReactNode }) {
  return (
    <article className="crm-panel min-h-36 p-5">
      <div className="flex items-start justify-between gap-4">
        <span className="text-[.68rem] font-bold uppercase tracking-[.1em] text-muted-foreground">{label}</span>
        <span className="text-accent [&>svg]:h-5 [&>svg]:w-5">{icon}</span>
      </div>
      <p className="mt-7 font-display text-4xl tracking-tight">{value}</p>
    </article>
  )
}
