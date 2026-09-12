import { useAlertas } from "@/hooks/useApi"
import { AlertTriangle, Clock, Mail } from "lucide-react"
import { parseISO, formatDistanceToNow } from "date-fns"
import { es } from "date-fns/locale"

export function AlertasPage() {
  const { data: alertas, isLoading } = useAlertas()

  const leads = alertas?.leads || []

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-muted-foreground">Cargando alertas...</div>
      </div>
    )
  }

  return (
    <div className="space-y-7">
      <div>
        <span className="crm-eyebrow">Seguimiento</span>
        <h1 className="crm-page-title mt-2">Alertas</h1>
        <p className="mt-3 text-sm text-muted-foreground">
          Leads que requieren seguimiento
        </p>
      </div>

      {leads.length === 0 ? (
        <div className="crm-panel border-t-4 border-t-accent py-16 text-center">
          <AlertTriangle className="mx-auto mb-4 h-10 w-10 text-accent" />
          <h3 className="font-display text-2xl">Sin alertas</h3>
          <p className="text-sm text-muted-foreground mt-1">
            Todos tus leads están al día
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {leads.map((lead: any) => (
            <div
              key={lead.id}
              className="crm-panel p-5 transition-colors hover:bg-muted/35"
            >
              <div className="flex items-start gap-4">
                <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-sm bg-secondary">
                  <AlertTriangle className="h-5 w-5 text-warning" />
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <h3 className="font-medium text-foreground">
                      {lead.nombre}
                    </h3>
                    <span className="rounded-full bg-secondary px-2 py-0.5 text-xs font-semibold text-secondary-foreground">
                      {lead.diasSinActividad} días
                    </span>
                  </div>

                  <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                    <span className="flex items-center gap-1">
                      <Mail className="h-3 w-3" />
                      {lead.email}
                    </span>
                    <span className="flex items-center gap-1">
                      <Clock className="h-3 w-3" />
                      Última actividad:{" "}
                      {formatDistanceToNow(
                        parseISO(lead.fechaUltimaActividad),
                        { addSuffix: true, locale: es }
                      )}
                    </span>
                  </div>

                  <div className="mt-2">
                    <span className="text-xs text-muted-foreground">
                      Etapa: {lead.etapaPipeline}
                    </span>
                  </div>
                </div>

                <button className="crm-button min-h-9 px-3 py-1.5 text-xs">
                  Contactar
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
