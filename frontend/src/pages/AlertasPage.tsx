import { useAlertas } from "@/hooks/useApi"
import { AlertTriangle, Clock, User, Mail, Phone } from "lucide-react"
import { format, parseISO, formatDistanceToNow } from "date-fns"
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
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Alertas</h1>
        <p className="text-sm text-muted-foreground">
          Leads que requieren seguimiento
        </p>
      </div>

      {leads.length === 0 ? (
        <div className="text-center py-12 bg-card rounded-lg border">
          <AlertTriangle className="h-12 w-12 text-muted-foreground mx-auto mb-3" />
          <h3 className="font-medium text-foreground">Sin alertas</h3>
          <p className="text-sm text-muted-foreground mt-1">
            Todos tus leads estan al dia
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {leads.map((lead: any) => (
            <div
              key={lead.id}
              className="bg-card rounded-lg border p-4 hover:bg-accent/5 transition-colors"
            >
              <div className="flex items-start gap-4">
                <div className="h-10 w-10 rounded-full bg-[#f59e0b]/10 flex items-center justify-center flex-shrink-0">
                  <AlertTriangle className="h-5 w-5 text-[#f59e0b]" />
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <h3 className="font-medium text-foreground">
                      {lead.nombre}
                    </h3>
                    <span className="px-2 py-0.5 text-xs bg-[#f59e0b]/10 text-[#f59e0b] rounded-full">
                      {lead.diasSinActividad} dias
                    </span>
                  </div>

                  <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                    <span className="flex items-center gap-1">
                      <Mail className="h-3 w-3" />
                      {lead.email}
                    </span>
                    <span className="flex items-center gap-1">
                      <Clock className="h-3 w-3" />
                      Ultima actividad:{" "}
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

                <button className="px-3 py-1.5 text-xs bg-[#1a4d2e] dark:bg-[#22c55e] text-white rounded-md hover:bg-[#154225] dark:hover:bg-[#16a34a] transition-colors">
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
