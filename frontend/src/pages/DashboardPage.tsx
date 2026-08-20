import { usePipeline, useAlertas, useVisitas } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
import {
  Users,
  Home,
  CalendarCheck,
  AlertTriangle,
  TrendingUp,
  Clock,
  ArrowUpRight,
  Activity,
} from "lucide-react"
import { format, parseISO, differenceInDays } from "date-fns"
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
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-gradient-to-r from-[#1a4d2e] to-[#22c55e] dark:from-[#154225] dark:to-[#16a34a] rounded-lg p-6 text-white">
        <h1 className="text-2xl font-bold">
          Bienvenido, {user?.nombre || "Asesor"}
        </h1>
        <p className="text-white/80 mt-1">
          Resumen de tu actividad inmobiliaria
        </p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <KPICard
          icon={<Users className="h-5 w-5" />}
          label="Total Leads"
          value={totalLeads}
          color="bg-[#1a4d2e] dark:bg-[#22c55e]"
          loading={loadingPipeline}
        />
        <KPICard
          icon={<Activity className="h-5 w-5" />}
          label="Leads Activos"
          value={leadsActivos.length}
          color="bg-[#3b82f6]"
          loading={loadingPipeline}
        />
        <KPICard
          icon={<CalendarCheck className="h-5 w-5" />}
          label="Visitas Pendientes"
          value={visitasPendientes.length}
          color="bg-[#f59e0b]"
          loading={loadingVisitas}
        />
        <KPICard
          icon={<TrendingUp className="h-5 w-5" />}
          label="Tasa Conversion"
          value={`${tasaConversion}%`}
          color="bg-[#c9a84c]"
          loading={loadingPipeline}
        />
      </div>

      {/* Main content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Pipeline chart */}
        <div className="lg:col-span-2 bg-card rounded-lg border p-4">
          <h2 className="font-semibold text-foreground mb-4">
            Pipeline por Etapa
          </h2>
          <div className="space-y-3">
            {Object.entries(leadsPorEtapa).map(([etapa, count]) => (
              <div key={etapa} className="flex items-center gap-3">
                <span className="text-sm text-muted-foreground w-32 truncate">
                  {etapa}
                </span>
                <div className="flex-1 h-6 bg-muted rounded overflow-hidden">
                  <div
                    className="h-full bg-gradient-to-r from-[#1a4d2e] to-[#22c55e] dark:from-[#154225] dark:to-[#16a34a] rounded transition-all duration-500"
                    style={{
                      width: `${Math.max(
                        ((count as number) / totalLeads) * 100,
                        4
                      )}%`,
                    }}
                  />
                </div>
                <span className="text-sm font-medium w-8 text-right">
                  {count as number}
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Alertas */}
        <div className="bg-card rounded-lg border p-4">
          <h2 className="font-semibold text-foreground mb-4 flex items-center gap-2">
            <AlertTriangle className="h-4 w-4 text-[#f59e0b]" />
            Alertas
          </h2>
          {loadingAlertas ? (
            <div className="text-sm text-muted-foreground">Cargando...</div>
          ) : alertasLeads.length === 0 ? (
            <div className="text-center py-6">
              <p className="text-sm text-muted-foreground">
                Sin alertas pendientes
              </p>
            </div>
          ) : (
            <div className="space-y-2">
              {alertasLeads.slice(0, 5).map((alerta: any) => (
                <div
                  key={alerta.id}
                  className="flex items-center gap-2 p-2 rounded bg-[#f59e0b]/10 border border-[#f59e0b]/20"
                >
                  <div className="h-2 w-2 rounded-full bg-[#f59e0b] animate-pulse" />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">
                      {alerta.nombre}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {alerta.diasSinActividad} dias sin actividad
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Visitas proximas */}
      <div className="bg-card rounded-lg border p-4">
        <h2 className="font-semibold text-foreground mb-4 flex items-center gap-2">
          <Clock className="h-4 w-4 text-[#3b82f6]" />
          Proximas Visitas
        </h2>
        {loadingVisitas ? (
          <div className="text-sm text-muted-foreground">Cargando...</div>
        ) : visitasPendientes.length === 0 ? (
          <div className="text-center py-6">
            <p className="text-sm text-muted-foreground">
              No hay visitas programadas
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            {visitasPendientes.slice(0, 6).map((visita: any) => (
              <div
                key={visita.id}
                className="flex items-center gap-3 p-3 rounded-lg border hover:bg-accent/5 transition-colors"
              >
                <div className="h-10 w-10 rounded-lg bg-[#3b82f6]/10 flex items-center justify-center">
                  <CalendarCheck className="h-5 w-5 text-[#3b82f6]" />
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
  color,
  loading,
}: {
  icon: React.ReactNode
  label: string
  value: number | string
  color: string
  loading: boolean
}) {
  return (
    <div className="bg-card rounded-lg border p-4">
      <div className="flex items-center gap-3">
        <div
          className={`h-10 w-10 rounded-lg ${color} flex items-center justify-center text-white`}
        >
          {icon}
        </div>
        <div>
          <p className="text-sm text-muted-foreground">{label}</p>
          <p className="text-2xl font-bold text-foreground">
            {loading ? "..." : value}
          </p>
        </div>
      </div>
    </div>
  )
}
