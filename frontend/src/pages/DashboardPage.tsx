import { usePipeline, useAlertas, useVisitas } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
import {
  Users,
  CalendarCheck,
  AlertTriangle,
  TrendingUp,
  Clock,
  ArrowUpRight,
  Activity,
} from "lucide-react"
import { format, parseISO } from "date-fns"
import { es } from "date-fns/locale"

export function DashboardPage() {
  const { data: pipeline, isLoading: loadingPipeline } = usePipeline()
  const { data: alertas, isLoading: loadingAlertas } = useAlertas()
  const { data: visitas, isLoading: loadingVisitas } = useVisitas()
  const user = useAuthStore((s) => s.user)

  const totalLeads = pipeline?.total || 0
  const leadsPorEtapa = pipeline?.leads?.reduce(
    (acc: Record<string, number>, lead: any) => {
      acc[lead.etapaPipeline] = (acc[lead.etapaPipeline] || 0) + 1
      return acc
    },
    {}
  ) || {}

  const leadsActivos =
    pipeline?.leads?.filter((l: any) =>
      !["Cerrado ganado", "Cerrado perdido"].includes(l.etapaPipeline)
    ) || []

  const visitasPendientes =
    visitas?.filter((v: any) => v.estado === "Programada") || []

  const alertasLeads = alertas?.leads || []

  // Calcular kpi
  const tasaConversion =
    totalLeads > 0
      ? Math.round(
          ((leadsPorEtapa["Cerrado ganado"] || 0) / totalLeads) * 100
        )
      : 0

  return (
    <div className="space-y-8">
      <div className="crm-hero flex min-h-[250px] flex-col justify-end rounded-lg p-7 md:p-10">
        <span className="text-[.7rem] font-bold uppercase tracking-[.18em] text-[#e7b99f]">Pulso comercial</span>
        <h1 className="mt-4 max-w-3xl font-display text-[clamp(2.6rem,5vw,5rem)] leading-[.95] tracking-[-.05em]">
          Buen día, {user?.nombre || "Asesor"}.
        </h1>
        <p className="mt-4 max-w-xl text-sm leading-6 text-white/80">Tu actividad inmobiliaria, clara y en movimiento.</p>
      </div>

      <div>
        <div className="mb-5 flex items-end justify-between gap-4">
          <div><span className="crm-eyebrow">Vista general</span><h2 className="mt-2 font-display text-3xl tracking-tight">Los números de hoy</h2></div>
          <span className="hidden text-xs font-semibold uppercase tracking-widest text-muted-foreground sm:block">L&C Propiedad Raíz</span>
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard
          icon={<Users className="h-5 w-5" />}
          label="Total de leads"
          value={totalLeads}
          loading={loadingPipeline}
        />
        <KPICard
          icon={<Activity className="h-5 w-5" />}
          label="Leads activos"
          value={leadsActivos.length}
          loading={loadingPipeline}
        />
        <KPICard
          icon={<CalendarCheck className="h-5 w-5" />}
          label="Visitas pendientes"
          value={visitasPendientes.length}
          loading={loadingVisitas}
        />
        <KPICard
          icon={<TrendingUp className="h-5 w-5" />}
          label="Tasa de conversión"
          value={`${tasaConversion}%`}
          loading={loadingPipeline}
        />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="crm-panel p-6 lg:col-span-2">
          <span className="crm-eyebrow">Movimiento</span>
          <h2 className="mb-6 mt-2 font-display text-2xl tracking-tight">
            Pipeline por etapa
          </h2>
          <div className="space-y-4">
            {Object.entries(leadsPorEtapa).map(([etapa, count]) => (
              <div key={etapa} className="flex items-center gap-3">
                <span className="w-32 truncate text-xs font-medium text-muted-foreground sm:w-40">
                  {etapa}
                </span>
                <div className="h-2 flex-1 overflow-hidden rounded-full bg-muted">
                  <div
                    className="h-full rounded-full bg-accent transition-all duration-500"
                    style={{
                      width: `${Math.max(
                        ((count as number) / totalLeads) * 100,
                        4
                      )}%`,
                    }}
                  />
                </div>
                <span className="w-8 text-right text-sm font-semibold">
                  {count as number}
                </span>
              </div>
            ))}
            {Object.keys(leadsPorEtapa).length === 0 && <p className="py-7 text-sm text-muted-foreground">El pipeline aparecerá cuando ingresen los primeros leads.</p>}
          </div>
        </div>

        <div className="crm-panel p-6">
          <span className="crm-eyebrow">Atención</span>
          <h2 className="mb-6 mt-2 flex items-center gap-2 font-display text-2xl tracking-tight">
            <AlertTriangle className="h-5 w-5 text-warning" /> Alertas
          </h2>
          {loadingAlertas ? (
            <div className="text-sm text-muted-foreground">Cargando...</div>
          ) : alertasLeads.length === 0 ? (
            <div className="rounded-md border border-border bg-muted/35 p-6">
              <p className="font-display text-xl">Todo al día.</p>
              <p className="mt-2 text-sm text-muted-foreground">No hay alertas pendientes.</p>
            </div>
          ) : (
            <div className="space-y-2">
              {alertasLeads.slice(0, 5).map((alerta: any) => (
                <div
                  key={alerta.id}
                  className="flex items-center gap-3 rounded-md border border-border bg-muted/35 p-3"
                >
                  <div className="h-2 w-2 rounded-full bg-warning" />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">
                      {alerta.nombre}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {alerta.diasSinActividad} días sin actividad
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className="crm-panel p-6">
        <span className="crm-eyebrow">Agenda</span>
        <h2 className="mb-6 mt-2 flex items-center gap-2 font-display text-2xl tracking-tight">
          <Clock className="h-5 w-5 text-accent" /> Próximas visitas
        </h2>
        {loadingVisitas ? (
          <div className="text-sm text-muted-foreground">Cargando...</div>
        ) : visitasPendientes.length === 0 ? (
          <div className="py-6 text-sm text-muted-foreground">
            No hay visitas programadas por ahora.
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            {visitasPendientes.slice(0, 6).map((visita: any) => (
              <div key={visita.id} className="flex items-center gap-3 rounded-md border border-border p-4 transition-colors hover:bg-muted/40">
                <div className="flex h-10 w-10 items-center justify-center rounded-sm bg-secondary">
                  <CalendarCheck className="h-5 w-5 text-foreground" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">
                    {visita.inmuebleNombre || "Visita"}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {format(parseISO(visita.fechaProgramada), "dd MMM HH:mm", {
                      locale: es,
                    })}
                  </p>
                </div>
                <ArrowUpRight className="h-4 w-4 text-muted-foreground" />
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

function KPICard({
  icon,
  label,
  value,
  loading,
}: {
  icon: React.ReactNode
  label: string
  value: number | string
  loading: boolean
}) {
  return (
    <div className="crm-panel min-h-36 p-5">
      <div className="flex items-start justify-between gap-3">
        <p className="text-[.7rem] font-bold uppercase tracking-[.11em] text-muted-foreground">{label}</p>
        <span className="text-accent">{icon}</span>
      </div>
      <p className="mt-7 font-display text-4xl leading-none tracking-tight">{loading ? "..." : value}</p>
      <div className="mt-4 h-[2px] w-10 bg-accent" />
    </div>
  )
}
