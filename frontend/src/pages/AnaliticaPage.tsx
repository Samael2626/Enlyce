import { Fragment, useState } from "react"
import { Link, Navigate } from "react-router-dom"
import { ArrowDownRight, BarChart3, MousePointerClick, TrendingUp } from "lucide-react"
import { api } from "@/api/client"
import { useAdvisors, useAdvisorPerformanceReport, useAnalyticsFunnel, useContacts, useCreateRealizedDeal, useCrmReportMetrics, useFirstResponseMetrics, useInmuebles, useInventoryAnalytics, useOwnerPerformanceReport, useRealizedDealsReport, useSentEmailReport, useUpdateRealizedDeal, useWebPropertyVisitsReport } from "@/hooks/useApi"
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
  const [emailFrom, setEmailFrom] = useState(() => dateOffset(-29))
  const [emailTo, setEmailTo] = useState(() => dateOffset(0))
  const [emailPage, setEmailPage] = useState(1)
  const [webFrom, setWebFrom] = useState(() => dateOffset(-29))
  const [webTo, setWebTo] = useState(() => dateOffset(0))
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
  const advisorReport = useAdvisorPerformanceReport(dealFrom, dealTo)
  const ownerReport = useOwnerPerformanceReport(dealFrom, dealTo)
  const sentEmailReport = useSentEmailReport({ from: emailFrom, to: emailTo, page: emailPage, pageSize: 10 })
  const webPropertyReport = useWebPropertyVisitsReport(webFrom, webTo)

  if (user?.rol !== "Administrador") return <Navigate to="/dashboard" replace />

  const steps = funnel.data?.steps ?? []
  const visits = steps.find((step) => step.event === "page_view")?.uniqueSessions ?? 0
  const conversions = steps.find((step) => step.event === "conversion")?.uniqueSessions ?? 0
  const conversionRate = visits === 0 ? 0 : Math.round((conversions * 1000) / visits) / 10
  return (
    <div className="space-y-8">
      <header className="analytics-hero rounded-lg p-7 md:p-10">
        <div className="relative z-10 flex flex-wrap items-end justify-between gap-6">
          <div className="max-w-3xl">
            <span className="text-[.68rem] font-bold uppercase tracking-[.18em] text-[#e3be65]">Lectura comercial</span>
            <h1 className="mt-4 font-display text-[clamp(2.7rem,6vw,5.4rem)] leading-[.9] tracking-[-.05em]">Del interés a la oportunidad.</h1>
            <p className="mt-5 max-w-xl text-sm leading-6 text-white/72">Sesiones anónimas de la web pública. Sin nombres, correos, teléfonos ni vigilancia decorativa.</p>
          </div>
          <Link
            to="/mercado"
            className="flex items-center gap-2 rounded-md border border-white/20 bg-white/10 px-4 py-2.5 text-xs font-semibold uppercase tracking-wider text-white transition hover:bg-white/20"
          >
            <TrendingUp className="h-4 w-4 text-[#e3be65]" />
            Mercado Inmobiliario (ACM)
          </Link>
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
          <div className="grid gap-4 lg:grid-cols-2">
            <div className="crm-panel p-5"><span className="text-xs text-muted-foreground">Total disponible</span><b className="mt-1 block font-display text-3xl">{inventory.data?.totalProperties ?? 0}</b></div>
            <InventoryBreakdown title="Por tipo de inmueble" rows={inventory.data?.byPropertyType ?? []} />
            <InventoryBreakdown title="Por modalidad" rows={inventory.data?.byOperation ?? []} />
            <InventoryTypeOperationBreakdown rows={inventory.data?.byTypeAndOperation ?? []} />
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

      <AdvisorPerformanceSection report={advisorReport.data} from={dealFrom} to={dealTo} isLoading={advisorReport.isLoading} isError={advisorReport.isError} />
      <OwnerPerformanceSection report={ownerReport.data} from={dealFrom} to={dealTo} isLoading={ownerReport.isLoading} isError={ownerReport.isError} />
      <SentEmailReportSection
        report={sentEmailReport.data}
        from={emailFrom}
        to={emailTo}
        page={emailPage}
        isLoading={sentEmailReport.isLoading}
        isError={sentEmailReport.isError}
        onFromChange={(value) => { setEmailPage(1); setEmailFrom(value) }}
        onToChange={(value) => { setEmailPage(1); setEmailTo(value) }}
        onPageChange={setEmailPage}
      />
      <WebPropertyVisitsSection
        report={webPropertyReport.data}
        from={webFrom}
        to={webTo}
        isLoading={webPropertyReport.isLoading}
        isError={webPropertyReport.isError}
        onFromChange={setWebFrom}
        onToChange={setWebTo}
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

function AdvisorPerformanceSection({
  report,
  from,
  to,
  isLoading,
  isError,
}: {
  report?: import("@/lib/types").AdvisorPerformanceReport
  from: string
  to: string
  isLoading: boolean
  isError: boolean
}) {
  const [exportingId, setExportingId] = useState("")
  const [emailAdvisorId, setEmailAdvisorId] = useState("")
  const [recipientEmail, setRecipientEmail] = useState("")
  const [sendingEmail, setSendingEmail] = useState(false)
  const [emailMessage, setEmailMessage] = useState("")
  const [actionError, setActionError] = useState("")
  const [detailAdvisorId, setDetailAdvisorId] = useState("")
  const [activityDetails, setActivityDetails] = useState<Record<string, import("@/lib/types").AdvisorActivityDetail>>({})
  const [loadingDetailId, setLoadingDetailId] = useState("")
  const metrics = [
    ["Interacciones", "interactions", "previousInteractions"],
    ["Visitas", "visits", "previousVisits"],
    ["Ventas", "sales", "previousSales"],
    ["Arriendos", "rentals", "previousRentals"],
    ["Clientes nuevos", "newClients", "previousNewClients"],
    ["Inmuebles nuevos", "newProperties", "previousNewProperties"],
  ] as const

  async function exportXlsx(advisor: import("@/lib/types").AdvisorPerformanceReport["advisors"][number]) {
    setExportingId(advisor.advisorId)
    setActionError("")
    try {
      const blob = await api.downloadAdvisorReportXlsx(advisor.advisorId, from, to)
      const url = URL.createObjectURL(blob)
      const link = document.createElement("a")
      link.href = url
      link.download = `reporte-${advisor.advisorName.replace(/[^a-z0-9_-]/gi, "-")}-${from}-${to}.xlsx`
      link.click()
      window.setTimeout(() => URL.revokeObjectURL(url), 1_000)
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "No se pudo exportar el reporte.")
    } finally {
      setExportingId("")
    }
  }

  async function exportPdf(advisor: import("@/lib/types").AdvisorPerformanceReport["advisors"][number]) {
    const popup = window.open("", "_blank")
    if (!popup) {
      setActionError("El navegador bloqueó la ventana de impresión.")
      return
    }
    setActionError("")
    popup.document.write("<p style='font:14px Arial;padding:24px'>Preparando reporte…</p>")
    try {
      const detail = await api.getAdvisorActivityDetail(advisor.advisorId, from, to)
      const summaryRows = metrics.map(([label, currentKey, previousKey]) =>
        `<tr><th>${escapeHtml(label)}</th><td>${advisor[currentKey]}</td><td>${advisor[previousKey]}</td></tr>`
      ).join("")
      const table = (headers: string[], rows: string) => `<table><thead><tr>${headers.map((header) => `<th>${escapeHtml(header)}</th>`).join("")}</tr></thead><tbody>${rows || `<tr><td colspan="${headers.length}">Sin registros</td></tr>`}</tbody></table>`
      const interactionRows = detail.interactions.map((item) => `<tr><td>${escapeHtml(new Date(item.date).toLocaleString("es-CO"))}</td><td>${escapeHtml(item.type)}</td><td>${escapeHtml(item.clientName)}</td><td>${escapeHtml(item.summary || "")}</td></tr>`).join("")
      const visitRows = detail.visits.map((item) => `<tr><td>${escapeHtml(new Date(item.scheduledAt).toLocaleString("es-CO"))}</td><td>${escapeHtml(item.status)}</td><td>${escapeHtml(item.clientName)}</td><td>${escapeHtml(item.propertyName)}</td><td>${escapeHtml(item.feedback || "")}</td></tr>`).join("")
      const dealRows = detail.deals.map((item) => `<tr><td>${escapeHtml(item.closedOn)}</td><td>${escapeHtml(item.operation)}</td><td>${escapeHtml(item.propertyName)}</td><td>${escapeHtml(item.clientName)}</td><td>${item.grossAmount.toLocaleString("es-CO")} ${escapeHtml(item.currency)}</td><td>${item.commissionAmount.toLocaleString("es-CO")} ${escapeHtml(item.currency)}</td></tr>`).join("")
      popup.document.open()
      popup.document.write(`<!doctype html><html lang="es"><head><meta charset="utf-8"><title>Reporte de ${escapeHtml(advisor.advisorName)}</title><style>body{font:12px Arial,sans-serif;margin:28px;color:#222}h1{font-size:22px}h2{font-size:16px;margin-top:28px}p{color:#555}table{border-collapse:collapse;width:100%;margin-top:12px}th,td{border:1px solid #ddd;padding:6px;text-align:left;vertical-align:top}thead{background:#eee}@media print{body{margin:10mm}h2{break-after:avoid}tr{break-inside:avoid}}</style></head><body><h1>Rendimiento por usuario encargado</h1><p>${escapeHtml(advisor.advisorName)} · periodo ${escapeHtml(report?.from ?? from)} a ${escapeHtml(report?.through ?? to)} · comparación ${escapeHtml(report?.previousFrom ?? "")} a ${escapeHtml(report?.previousThrough ?? "")}</p>${table(["Indicador", "Actual", "Anterior"], summaryRows)}<h2>Interacciones (${detail.interactions.length})</h2>${table(["Fecha", "Tipo", "Cliente", "Resumen"], interactionRows)}<h2>Visitas (${detail.visits.length})</h2>${table(["Programada", "Estado", "Cliente", "Inmueble", "Feedback"], visitRows)}<h2>Ventas y arriendos (${detail.deals.length})</h2>${table(["Fecha", "Operación", "Inmueble", "Cliente", "Valor bruto", "Comisión"], dealRows)}</body></html>`)
      popup.document.close()
      popup.focus()
      window.setTimeout(() => popup.print(), 300)
    } catch (error) {
      popup.close()
      setActionError(error instanceof Error ? error.message : "No se pudo cargar el detalle para PDF.")
    }
  }

  async function sendEmail(advisorId: string) {
    setSendingEmail(true)
    setEmailMessage("")
    setActionError("")
    try {
      const result = await api.emailAdvisorReport(advisorId, from, to, recipientEmail)
      setEmailMessage(result.message)
      setEmailAdvisorId("")
      setRecipientEmail("")
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "No se pudo enviar el reporte.")
    } finally {
      setSendingEmail(false)
    }
  }

  async function toggleDetail(advisorId: string) {
    if (detailAdvisorId === advisorId) {
      setDetailAdvisorId("")
      return
    }
    setDetailAdvisorId(advisorId)
    if (activityDetails[advisorId]) return
    setLoadingDetailId(advisorId)
    setActionError("")
    try {
      const detail = await api.getAdvisorActivityDetail(advisorId, from, to)
      setActivityDetails((current) => ({ ...current, [advisorId]: detail }))
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "No se pudo cargar el detalle del asesor.")
      setDetailAdvisorId("")
    } finally {
      setLoadingDetailId("")
    }
  }

  return (
    <section className="space-y-4" aria-labelledby="advisor-report-title">
      <div>
        <span className="crm-eyebrow">Equipo comercial</span>
        <h2 id="advisor-report-title" className="mt-2 font-display text-3xl tracking-tight">Rendimiento por usuario encargado</h2>
        <p className="mt-2 text-sm text-muted-foreground">Compara el rango del reporte de negocios con el periodo anterior de igual duración. Clientes e inmuebles se atribuyen al encargado actual.</p>
      </div>
      {isLoading ? (
        <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando rendimiento por asesor…</div>
      ) : isError ? (
        <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el reporte por asesor.</div>
      ) : (
        <div className="crm-panel overflow-x-auto">
          {actionError && <p role="alert" className="border-b border-border px-5 py-3 text-sm text-destructive">{actionError}</p>}
          {emailMessage && <p role="status" className="border-b border-border px-5 py-3 text-sm text-success">{emailMessage}</p>}
          <p className="border-b border-border px-5 py-3 text-xs text-muted-foreground">
            Periodo {report?.from} a {report?.through} · comparación {report?.previousFrom} a {report?.previousThrough}. Interacciones son registros del CRM; visitas son citas no canceladas por fecha programada.
          </p>
          <table className="w-full min-w-[900px] text-left text-sm">
            <thead className="bg-muted/50 text-xs text-muted-foreground">
              <tr><th className="px-4 py-3">Usuario encargado</th>{metrics.map(([label]) => <th key={label} className="px-3 py-3">{label}</th>)}<th className="px-3 py-3">Opciones</th></tr>
            </thead>
            <tbody>
              {(report?.advisors ?? []).map((advisor) => (
                <Fragment key={advisor.advisorId}>
                  <tr className="border-t border-border">
                    <th scope="row" className="px-4 py-3 font-medium">{advisor.advisorName}</th>
                    {metrics.map(([label, currentKey, previousKey]) => {
                      const current = advisor[currentKey]
                      const difference = current - advisor[previousKey]
                      return <td key={label} className="px-3 py-3 tabular-nums">{current} <span className="text-xs text-muted-foreground">{difference > 0 ? `↑ ${difference}` : difference < 0 ? `↓ ${Math.abs(difference)}` : "→ 0"}</span></td>
                    })}
                    <td className="px-3 py-2"><div className="flex flex-wrap gap-1"><button type="button" className="rounded border px-2 py-1 text-xs" onClick={() => void toggleDetail(advisor.advisorId)}>{loadingDetailId === advisor.advisorId ? "Cargando…" : detailAdvisorId === advisor.advisorId ? "Ocultar detalle" : "Detalle"}</button><button type="button" disabled={Boolean(exportingId)} className="rounded border px-2 py-1 text-xs disabled:opacity-50" onClick={() => void exportXlsx(advisor)}>{exportingId === advisor.advisorId ? "Preparando…" : "Excel"}</button><button type="button" className="rounded border px-2 py-1 text-xs" onClick={() => void exportPdf(advisor)}>PDF</button><button type="button" className="rounded border px-2 py-1 text-xs" onClick={() => { setEmailAdvisorId((current) => current === advisor.advisorId ? "" : advisor.advisorId); setRecipientEmail(""); setEmailMessage(""); setActionError("") }}>Enviar</button></div></td>
                  </tr>
                  {detailAdvisorId === advisor.advisorId && <tr className="border-t border-border bg-muted/10"><td colSpan={metrics.length + 2} className="space-y-5 px-4 py-4"><AdvisorActivityDetailView detail={activityDetails[advisor.advisorId]} loading={loadingDetailId === advisor.advisorId} /></td></tr>}
                  {emailAdvisorId === advisor.advisorId && <tr key={`${advisor.advisorId}-email`} className="border-t border-border bg-muted/20"><td colSpan={metrics.length + 2} className="px-4 py-3"><form className="flex flex-wrap items-end gap-2" onSubmit={(event) => { event.preventDefault(); void sendEmail(advisor.advisorId) }}><label className="min-w-64 flex-1 text-xs font-semibold text-muted-foreground">Correo destinatario<input type="email" value={recipientEmail} onChange={(event) => setRecipientEmail(event.target.value)} placeholder="Vacío: correo del asesor" className="mt-1 block w-full rounded-sm border border-input bg-card px-3 py-2 text-sm text-foreground" /></label><button type="submit" disabled={sendingEmail} className="crm-button disabled:opacity-50">{sendingEmail ? "Enviando…" : "Enviar reporte ZIP"}</button><button type="button" disabled={sendingEmail} className="rounded border px-3 py-2 text-sm" onClick={() => setEmailAdvisorId("")}>Cancelar</button></form></td></tr>}
                </Fragment>
              ))}
              {report?.advisors.length === 0 && <tr><td colSpan={metrics.length + 2} className="px-4 py-8 text-center text-muted-foreground">No hay usuarios encargados para mostrar.</td></tr>}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}

function AdvisorActivityDetailView({ detail, loading }: { detail?: import("@/lib/types").AdvisorActivityDetail; loading: boolean }) {
  if (loading || !detail) return <p className="text-sm text-muted-foreground">{loading ? "Cargando registros del periodo…" : "Sin detalle cargado."}</p>
  const date = (value: string) => new Date(value).toLocaleString("es-CO")
  return <div className="grid gap-5 xl:grid-cols-3">
    <section><h3 className="mb-2 font-semibold">Interacciones ({detail.interactions.length})</h3><div className="max-h-72 overflow-auto rounded border border-border"><table className="w-full text-left text-xs"><thead className="sticky top-0 bg-muted"><tr><th className="p-2">Fecha</th><th className="p-2">Tipo / cliente</th><th className="p-2">Resumen</th></tr></thead><tbody>{detail.interactions.map((item, index) => <tr key={`${item.date}-${index}`} className="border-t border-border"><td className="p-2 whitespace-nowrap">{date(item.date)}</td><td className="p-2">{item.type}<br />{item.clientName}</td><td className="p-2">{item.summary || "—"}</td></tr>)}</tbody></table>{detail.interactions.length === 0 && <p className="p-3 text-muted-foreground">Sin registros.</p>}</div></section>
    <section><h3 className="mb-2 font-semibold">Visitas ({detail.visits.length})</h3><div className="max-h-72 overflow-auto rounded border border-border"><table className="w-full text-left text-xs"><thead className="sticky top-0 bg-muted"><tr><th className="p-2">Fecha</th><th className="p-2">Cliente / inmueble</th><th className="p-2">Estado / feedback</th></tr></thead><tbody>{detail.visits.map((item, index) => <tr key={`${item.scheduledAt}-${index}`} className="border-t border-border"><td className="p-2 whitespace-nowrap">{date(item.scheduledAt)}</td><td className="p-2">{item.clientName}<br />{item.propertyName}</td><td className="p-2">{item.status}{item.feedback && <><br />{item.feedback}</>}</td></tr>)}</tbody></table>{detail.visits.length === 0 && <p className="p-3 text-muted-foreground">Sin registros.</p>}</div></section>
    <section><h3 className="mb-2 font-semibold">Ventas y arriendos ({detail.deals.length})</h3><div className="max-h-72 overflow-auto rounded border border-border"><table className="w-full text-left text-xs"><thead className="sticky top-0 bg-muted"><tr><th className="p-2">Fecha / operación</th><th className="p-2">Cliente / inmueble</th><th className="p-2">Valor / comisión</th></tr></thead><tbody>{detail.deals.map((item, index) => <tr key={`${item.closedOn}-${index}`} className="border-t border-border"><td className="p-2">{item.closedOn}<br />{item.operation}</td><td className="p-2">{item.clientName}<br />{item.propertyName}</td><td className="p-2">{item.grossAmount.toLocaleString("es-CO")} {item.currency}<br />{item.commissionAmount.toLocaleString("es-CO")} {item.currency}</td></tr>)}</tbody></table>{detail.deals.length === 0 && <p className="p-3 text-muted-foreground">Sin registros.</p>}</div></section>
  </div>
}

function OwnerPerformanceSection({
  report,
  from,
  to,
  isLoading,
  isError,
}: {
  report?: import("@/lib/types").OwnerPerformanceReport
  from: string
  to: string
  isLoading: boolean
  isError: boolean
}) {
  const [exporting, setExporting] = useState(false)
  const [exportError, setExportError] = useState("")
  const columns = [
    ["Visitas en página", "pageVisits"],
    ["Llamadas", "calls"],
    ["Correos", "emails"],
    ["Eventos", "events"],
    ["Visitas", "visits"],
    ["Contactos recibidos", "receivedContacts"],
  ] as const

  async function exportXlsx() {
    setExporting(true)
    setExportError("")
    try {
      const blob = await api.downloadOwnerPerformanceReportXlsx(from, to)
      const url = URL.createObjectURL(blob)
      const link = document.createElement("a")
      link.href = url
      link.download = `propietarios-${from}-${to}.xlsx`
      link.click()
      window.setTimeout(() => URL.revokeObjectURL(url), 1_000)
    } catch (error) {
      setExportError(error instanceof Error ? error.message : "No se pudo exportar el reporte.")
    } finally {
      setExporting(false)
    }
  }

  return (
    <section className="space-y-4" aria-labelledby="owner-report-title">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Propietarios</span>
          <h2 id="owner-report-title" className="mt-2 font-display text-3xl tracking-tight">Actividad por propietario</h2>
          <p className="mt-2 text-sm text-muted-foreground">Periodo {report?.from ?? from} a {report?.through ?? to}. Actividad asociada a consultas de publicaciones de sus inmuebles.</p>
        </div>
        <button type="button" className="crm-button min-h-9 px-3 py-1 text-xs" disabled={exporting || isLoading || isError} onClick={() => void exportXlsx()}>
          {exporting ? "Exportando…" : "Descargar Excel"}
        </button>
      </div>
      {exportError && <p role="alert" className="text-sm text-destructive">{exportError}</p>}
      {isLoading ? (
        <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando actividad de propietarios…</div>
      ) : isError ? (
        <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el reporte de propietarios.</div>
      ) : (
        <div className="crm-panel overflow-x-auto">
          <table className="w-full min-w-[880px] text-left text-sm">
            <thead className="bg-muted/50 text-xs text-muted-foreground">
              <tr><th className="px-4 py-3">Propietario</th>{columns.map(([label]) => <th key={label} className="px-3 py-3">{label}</th>)}</tr>
            </thead>
            <tbody>
              {(report?.owners ?? []).map((owner) => (
                <tr key={owner.ownerId} className="border-t border-border">
                  <th scope="row" className="px-4 py-3 font-medium">{owner.ownerName}</th>
                  {columns.map(([label, key]) => <td key={label} className="px-3 py-3 tabular-nums">{owner[key]}</td>)}
                </tr>
              ))}
              {report?.owners.length === 0 && <tr><td colSpan={columns.length + 1} className="px-4 py-8 text-center text-muted-foreground">No hay propietarios registrados.</td></tr>}
            </tbody>
          </table>
          <p className="border-t border-border px-5 py-3 text-xs text-muted-foreground">Visitas en página cuenta eventos de página de inmuebles publicados. Llamadas, correos y eventos cuentan interacciones registradas; visitas cuenta citas no canceladas. Los contactos recibidos son leads vinculados a una publicación.</p>
        </div>
      )}
    </section>
  )
}

function SentEmailReportSection({
  report,
  from,
  to,
  page,
  isLoading,
  isError,
  onFromChange,
  onToChange,
  onPageChange,
}: {
  report?: import("@/lib/types").SentEmailReportPage
  from: string
  to: string
  page: number
  isLoading: boolean
  isError: boolean
  onFromChange: (value: string) => void
  onToChange: (value: string) => void
  onPageChange: (value: number) => void
}) {
  const pageCount = Math.max(1, Math.ceil((report?.total ?? 0) / (report?.pageSize ?? 10)))
  const [exporting, setExporting] = useState(false)
  const [exportError, setExportError] = useState("")
  const statusLabels = {
    AcceptedBySmtp: "Aceptado por SMTP",
    Failed: "Falló el envío",
    NotConfigured: "SMTP no configurado",
  }

  async function exportXlsx() {
    setExporting(true)
    setExportError("")
    try {
      const blob = await api.downloadSentEmailReportXlsx(from, to)
      const url = URL.createObjectURL(blob)
      const link = document.createElement("a")
      link.href = url
      link.download = `correos-enviados-${from}-${to}.xlsx`
      link.click()
      window.setTimeout(() => URL.revokeObjectURL(url), 1_000)
    } catch (error) {
      setExportError(error instanceof Error ? error.message : "No se pudo exportar el reporte.")
    } finally {
      setExporting(false)
    }
  }

  return (
    <section className="space-y-4" aria-labelledby="sent-email-report-title">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Trazabilidad</span>
          <h2 id="sent-email-report-title" className="mt-2 font-display text-3xl tracking-tight">Correos enviados</h2>
          <p className="mt-2 text-sm text-muted-foreground">Registra intentos desde que se habilite la migración. “Aceptado por SMTP” no confirma entrega en la bandeja.</p>
        </div>
        <div className="flex flex-wrap items-end gap-3 text-xs font-bold text-muted-foreground">
          <label>Desde <input aria-label="Correos desde" type="date" value={from} max={to} onChange={(event) => onFromChange(event.target.value)} className="ml-2 rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
          <label>Hasta <input aria-label="Correos hasta" type="date" value={to} min={from} onChange={(event) => onToChange(event.target.value)} className="ml-2 rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
          <button type="button" className="crm-button min-h-9 px-3 py-1 text-xs" disabled={exporting || isLoading || isError} onClick={() => void exportXlsx()}>
            {exporting ? "Exportando…" : "Descargar Excel"}
          </button>
        </div>
      </div>
      {exportError && <p role="alert" className="text-sm text-destructive">{exportError}</p>}
      {isLoading ? (
        <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando registro de correos…</div>
      ) : isError ? (
        <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el registro de correos.</div>
      ) : (
        <div className="crm-panel overflow-x-auto">
          <table className="w-full min-w-[900px] text-left text-sm">
            <thead className="bg-muted/50 text-xs text-muted-foreground">
              <tr><th className="px-4 py-3">Fecha</th><th className="px-3 py-3">Asunto</th><th className="px-3 py-3">Descripción</th><th className="px-3 py-3">Destinatario</th><th className="px-3 py-3">Usuario encargado</th><th className="px-3 py-3">Estado</th></tr>
            </thead>
            <tbody>
              {(report?.items ?? []).map((item) => (
                <tr key={item.id} className="border-t border-border">
                  <td className="whitespace-nowrap px-4 py-3">{new Intl.DateTimeFormat("es-CO", { dateStyle: "short", timeStyle: "short" }).format(new Date(item.attemptedAt))}</td>
                  <td className="px-3 py-3">{item.subject}</td>
                  <td className="px-3 py-3">{item.description}</td>
                  <td className="px-3 py-3">{item.recipientAddress}</td>
                  <td className="px-3 py-3">{item.senderName}</td>
                  <td className="px-3 py-3">{statusLabels[item.status]}{item.failureCode ? ` (${item.failureCode})` : ""}</td>
                </tr>
              ))}
              {report?.items.length === 0 && <tr><td colSpan={6} className="px-4 py-8 text-center text-muted-foreground">No hay intentos de correo en este periodo.</td></tr>}
            </tbody>
          </table>
          <footer className="flex items-center justify-between border-t border-border px-4 py-3 text-sm">
            <span>{report?.total ?? 0} registros · página {page} de {pageCount}</span>
            <div className="flex gap-2">
              <button type="button" className="crm-button min-h-8 px-3 py-1 text-xs" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>Anterior</button>
              <button type="button" className="crm-button min-h-8 px-3 py-1 text-xs" disabled={page >= pageCount} onClick={() => onPageChange(page + 1)}>Siguiente</button>
            </div>
          </footer>
        </div>
      )}
    </section>
  )
}

function WebPropertyVisitsSection({
  report,
  from,
  to,
  isLoading,
  isError,
  onFromChange,
  onToChange,
}: {
  report?: import("@/lib/types").WebPropertyVisitsReport
  from: string
  to: string
  isLoading: boolean
  isError: boolean
  onFromChange: (value: string) => void
  onToChange: (value: string) => void
}) {
  const [exporting, setExporting] = useState(false)
  const [exportError, setExportError] = useState("")

  async function exportXlsx() {
    setExporting(true)
    setExportError("")
    try {
      const blob = await api.downloadWebPropertyVisitsXlsx(from, to)
      const url = URL.createObjectURL(blob)
      const link = document.createElement("a")
      link.href = url
      link.download = `visitas-inmuebles-${from}-${to}.xlsx`
      link.click()
      window.setTimeout(() => URL.revokeObjectURL(url), 1_000)
    } catch (error) {
      setExportError(error instanceof Error ? error.message : "No se pudo exportar el reporte.")
    } finally {
      setExporting(false)
    }
  }

  function exportPdf() {
    const popup = window.open("", "_blank")
    if (!popup) {
      setExportError("El navegador bloqueó la ventana de impresión.")
      return
    }
    setExportError("")
    const rows = (report?.properties ?? []).map((property) =>
      `<tr><th>${escapeHtml(property.title)}</th><td>${property.visits}</td></tr>`
    ).join("")
    popup.document.write(`<!doctype html><html lang="es"><head><meta charset="utf-8"><title>Visitas por inmueble</title><style>body{font:14px Arial,sans-serif;margin:32px;color:#222}h1{font-size:22px}p{color:#555}table{border-collapse:collapse;width:100%;margin-top:24px}th,td{border-bottom:1px solid #ddd;padding:10px;text-align:left}td{text-align:right}@media print{body{margin:12mm}}</style></head><body><h1>Visitas por inmueble publicado</h1><p>Periodo: ${escapeHtml(from)} a ${escapeHtml(to)}</p><table><thead><tr><th>Título inmueble</th><th>Visitas</th></tr></thead><tbody>${rows || "<tr><td colspan='2'>No hay inmuebles publicados.</td></tr>"}</tbody></table></body></html>`)
    popup.document.close()
    popup.focus()
    window.setTimeout(() => popup.print(), 300)
  }

  return (
    <section className="space-y-4" aria-labelledby="web-property-report-title">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Sitio web</span>
          <h2 id="web-property-report-title" className="mt-2 font-display text-3xl tracking-tight">Visitas por inmueble publicado</h2>
          <p className="mt-2 text-sm text-muted-foreground">Cuenta sesiones únicas por URL actual; incluye publicaciones publicadas, pausadas o retiradas.</p>
        </div>
        <div className="flex flex-wrap items-end gap-3 text-xs font-bold text-muted-foreground">
          <label>Desde <input aria-label="Visitas web desde" type="date" value={from} max={to} onChange={(event) => onFromChange(event.target.value)} className="ml-2 rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
          <label>Hasta <input aria-label="Visitas web hasta" type="date" value={to} min={from} onChange={(event) => onToChange(event.target.value)} className="ml-2 rounded-sm border border-input bg-card px-2 py-2 text-foreground" /></label>
          <button type="button" disabled={exporting || isLoading || isError} className="rounded-sm border border-border px-3 py-2 text-xs font-semibold text-foreground disabled:opacity-50" onClick={() => void exportXlsx()}>{exporting ? "Preparando…" : "Excel"}</button>
          <button type="button" disabled={isLoading || isError} className="rounded-sm border border-border px-3 py-2 text-xs font-semibold text-foreground disabled:opacity-50" onClick={exportPdf}>PDF</button>
        </div>
      </div>
      {exportError && <p role="alert" className="text-sm text-destructive">{exportError}</p>}
      {isLoading ? (
        <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando visitas del sitio…</div>
      ) : isError ? (
        <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el reporte de visitas web.</div>
      ) : (
        <div className="crm-panel overflow-x-auto">
          <table className="w-full min-w-[440px] text-left text-sm">
            <thead className="bg-muted/50 text-xs text-muted-foreground">
              <tr><th className="px-4 py-3">Título inmueble</th><th className="px-4 py-3">Visitas</th></tr>
            </thead>
            <tbody>
              {(report?.properties ?? []).map((property) => (
                <tr key={property.publicationId} className="border-t border-border">
                  <th scope="row" className="px-4 py-3 font-medium">{property.title}</th>
                  <td className="px-4 py-3 tabular-nums">{property.visits}</td>
                </tr>
              ))}
              {report?.properties.length === 0 && <tr><td colSpan={2} className="px-4 py-8 text-center text-muted-foreground">No hay inmuebles publicados.</td></tr>}
            </tbody>
          </table>
        </div>
      )}
    </section>
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
  const [editingDealId, setEditingDealId] = useState<string | null>(null)
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
              <thead className="border-b border-border bg-muted/50 text-muted-foreground"><tr>{["Inmueble", "Negocio", "Fecha", "Encargado", "Cliente", "Total bruto", "Comisión", "Acciones"].map((heading) => <th key={heading} className="px-4 py-3 font-semibold">{heading}</th>)}</tr></thead>
              <tbody className="divide-y divide-border">{(report?.items ?? []).map((item) => <tr key={item.id}>
                <td className="px-4 py-3">{item.propertyName}<small className="block text-muted-foreground">{item.propertyType}</small></td>
                <td className="px-4 py-3">{item.operation === "Sale" ? "Venta" : "Arriendo"}</td>
                <td className="px-4 py-3">{item.closedOn}</td>
                <td className="px-4 py-3">{item.advisorName}</td>
                <td className="px-4 py-3">{item.clientName}</td>
                <td className="px-4 py-3">{formatMoney(item.grossAmount, item.currency)}</td>
                <td className="px-4 py-3">{formatMoney(item.commissionAmount, item.currency)}</td>
                <td className="px-4 py-3">{Date.now() < new Date(item.recordedAtUtc).getTime() + 3 * 24 * 60 * 60 * 1000 && <button type="button" onClick={() => setEditingDealId(item.id)} className="text-sm font-semibold text-accent underline">Modificar</button>}</td>
              </tr>)}</tbody>
            </table>
            {report?.items.map((item) => item.id === editingDealId && <RealizedDealEditForm key={item.id} item={item} onClose={() => setEditingDealId(null)} />)}
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

function RealizedDealEditForm({ item, onClose }: {
  item: import("@/lib/types").RealizedDealReportPage["items"][number]
  onClose: () => void
}) {
  const updateDeal = useUpdateRealizedDeal()
  const [errorMessage, setErrorMessage] = useState("")

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage("")
    const form = new FormData(event.currentTarget)
    try {
      await updateDeal.mutateAsync({ id: item.id, input: {
        closedOn: String(form.get("closedOn")),
        grossAmount: Number(form.get("grossAmount")),
        commissionAmount: Number(form.get("commissionAmount")),
        currency: String(form.get("currency")),
      } })
      onClose()
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "No se pudo modificar el negocio.")
    }
  }

  return <form onSubmit={submit} className="grid gap-3 border-t border-border p-4 md:grid-cols-4">
    <p className="md:col-span-4 text-sm font-semibold">Modificar cierre de {item.propertyName}. Disponible durante 72 horas desde el registro.</p>
    <label className="text-xs font-bold">Fecha de cierre<input name="closedOn" type="date" required defaultValue={item.closedOn} className="mt-1 block w-full rounded border border-input p-2" /></label>
    <label className="text-xs font-bold">Valor bruto<input name="grossAmount" type="number" min="0.01" step="0.01" required defaultValue={item.grossAmount} className="mt-1 block w-full rounded border border-input p-2" /></label>
    <label className="text-xs font-bold">Comisión<input name="commissionAmount" type="number" min="0" step="0.01" required defaultValue={item.commissionAmount} className="mt-1 block w-full rounded border border-input p-2" /></label>
    <label className="text-xs font-bold">Moneda<input name="currency" minLength={3} maxLength={3} required defaultValue={item.currency} className="mt-1 block w-full rounded border border-input p-2 uppercase" /></label>
    <div className="flex gap-2 md:col-span-4"><button type="submit" disabled={updateDeal.isPending} className="rounded bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">Guardar</button><button type="button" onClick={onClose} className="rounded border border-border px-3 py-2 text-sm">Cancelar</button></div>
    {errorMessage && <p role="alert" className="text-sm text-destructive md:col-span-4">{errorMessage}</p>}
  </form>
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

function escapeHtml(value: string) {
  return value.replace(/[&<>\"']/g, (character) => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    "\"": "&quot;",
    "'": "&#39;",
  })[character]!)
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

function InventoryTypeOperationBreakdown({ rows }: { rows: { name: string; sale: number; rent: number; transfer: number; total: number }[] }) {
  return (
    <section className="crm-panel overflow-x-auto lg:col-span-2" aria-label="Inventario por tipo y negocio">
      <h3 className="border-b border-border p-5 font-display text-xl">Por tipo y negocio</h3>
      <table className="w-full text-left text-sm"><thead><tr className="border-b text-muted-foreground"><th className="p-3">Tipo</th><th className="p-3">Venta</th><th className="p-3">Arriendo</th><th className="p-3">Permuta</th><th className="p-3">Total de inmuebles</th></tr></thead>
        <tbody>{rows.map((row) => <tr key={row.name} className="border-b border-border"><td className="p-3">{row.name}</td><td className="p-3">{row.sale}</td><td className="p-3">{row.rent}</td><td className="p-3">{row.transfer}</td><td className="p-3">{row.total}</td></tr>)}
          {rows.length === 0 && <tr><td colSpan={5} className="p-5 text-muted-foreground">Sin propiedades disponibles.</td></tr>}</tbody></table>
      <p className="p-3 text-xs text-muted-foreground">Venta y arriendo cuenta en ambas columnas, pero una sola vez en el total.</p>
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
