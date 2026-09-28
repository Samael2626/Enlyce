import { useState } from "react"
import { Navigate } from "react-router-dom"
import { ArrowDownRight, BarChart3, MousePointerClick } from "lucide-react"
import { useAnalyticsFunnel } from "@/hooks/useApi"
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
  const user = useAuthStore((state) => state.user)
  const funnel = useAnalyticsFunnel(days)

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
    </div>
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
