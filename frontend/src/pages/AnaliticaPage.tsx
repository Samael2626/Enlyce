import { useState } from "react"
import { Navigate } from "react-router-dom"
import { ArrowDownRight, BarChart3, MousePointerClick } from "lucide-react"
import { useAnalyticsFunnel, useCrmReportMetrics, useFirstResponseMetrics } from "@/hooks/useApi"
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
  const user = useAuthStore((state) => state.user)
  const funnel = useAnalyticsFunnel(days)
  const crmReport = useCrmReportMetrics(crmFrom, crmTo)
  const responseMetrics = useFirstResponseMetrics(crmFrom, crmTo)

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
