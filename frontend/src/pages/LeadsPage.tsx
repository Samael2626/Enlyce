import { useState } from "react"
import { usePipeline, useLead, useInteracciones, useRegistrarInteraccion } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
import { cn } from "@/lib/utils"
import {
  User,
  Phone,
  Mail,
  Calendar,
  MessageSquare,
  ChevronDown,
  ChevronUp,
  Plus,
  Clock,
  Building2,
  FileText,
  X,
} from "lucide-react"
import { format, parseISO, formatDistanceToNow } from "date-fns"
import { es } from "date-fns/locale"

export function LeadsPage() {
  const { data: pipeline, isLoading } = usePipeline()
  const [expandedId, setExpandedId] = useState<string | null>(null)

  const leads = pipeline?.leads || []

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-muted-foreground">Cargando leads...</div>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Leads</h1>
          <p className="text-sm text-muted-foreground">
            {leads.length} leads registrados
          </p>
        </div>
        <button className="px-4 py-2 bg-[#1a4d2e] hover:bg-[#154225] dark:bg-[#22c55e] dark:hover:bg-[#16a34a] text-white text-sm font-medium rounded-md transition-colors flex items-center gap-2">
          <Plus className="h-4 w-4" />
          Nuevo Lead
        </button>
      </div>

      {/* Leads list */}
      <div className="space-y-2">
        {leads.length === 0 ? (
          <div className="text-center py-12 bg-card rounded-lg border">
            <User className="h-12 w-12 text-muted-foreground mx-auto mb-3" />
            <h3 className="font-medium text-foreground">Primer lead</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Agrega tu primer lead para comenzar
            </p>
          </div>
        ) : (
          leads.map((lead: any) => (
            <LeadCard
              key={lead.id}
              lead={lead}
              isExpanded={expandedId === lead.id}
              onToggle={() =>
                setExpandedId(expandedId === lead.id ? null : lead.id)
              }
            />
          ))
        )}
      </div>
    </div>
  )
}

function LeadCard({
  lead,
  isExpanded,
  onToggle,
}: {
  lead: any
  isExpanded: boolean
  onToggle: () => void
}) {
  const { data: leadDetail, isLoading: loadingDetail } = useLead(
    isExpanded ? lead.id : ""
  )
  const { data: interacciones, isLoading: loadingInteracciones } =
    useInteracciones(isExpanded ? lead.id : "")
  const registrarInteraccion = useRegistrarInteraccion()
  const user = useAuthStore((s) => s.user)

  const [showInteraccionForm, setShowInteraccionForm] = useState(false)
  const [tipoInteraccion, setTipoInteraccion] = useState("Llamada")
  const [resumen, setResumen] = useState("")

  const handleRegistrarInteraccion = () => {
    if (!user) return

    registrarInteraccion.mutate(
      {
        leadId: lead.id,
        asesorId: user.email,
        tipo: tipoInteraccion,
        resumen,
      },
      {
        onSuccess: () => {
          setResumen("")
          setShowInteraccionForm(false)
        },
      }
    )
  }

  return (
    <div className="bg-card rounded-lg border overflow-hidden">
      {/* Compact view */}
      <div
        className="flex items-center gap-4 p-4 cursor-pointer hover:bg-accent/5 transition-colors"
        onClick={onToggle}
      >
        <div className="h-10 w-10 rounded-full bg-[#1a4d2e] dark:bg-[#22c55e] flex items-center justify-center flex-shrink-0">
          <User className="h-5 w-5 text-white" />
        </div>

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="font-medium text-foreground truncate">
              {lead.nombre}
            </h3>
            <span className="px-2 py-0.5 text-xs bg-[#1a4d2e]/10 dark:bg-[#22c55e]/10 text-[#1a4d2e] dark:text-[#22c55e] rounded-full">
              {lead.etapaPipeline}
            </span>
          </div>
          <div className="flex items-center gap-4 mt-1 text-sm text-muted-foreground">
            <span className="flex items-center gap-1">
              <Mail className="h-3 w-3" />
              {lead.email}
            </span>
            {lead.telefono && (
              <span className="flex items-center gap-1">
                <Phone className="h-3 w-3" />
                {lead.telefono}
              </span>
            )}
            <span className="flex items-center gap-1">
              <Clock className="h-3 w-3" />
              {formatDistanceToNow(parseISO(lead.fechaCreacion), {
                addSuffix: true,
                locale: es,
              })}
            </span>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {lead.interaccionesCount > 0 && (
            <span className="text-xs text-muted-foreground flex items-center gap-1">
              <MessageSquare className="h-3 w-3" />
              {lead.interaccionesCount}
            </span>
          )}
          {isExpanded ? (
            <ChevronUp className="h-5 w-5 text-muted-foreground" />
          ) : (
            <ChevronDown className="h-5 w-5 text-muted-foreground" />
          )}
        </div>
      </div>

      {/* Expanded view */}
      {isExpanded && (
        <div className="border-t">
          {loadingDetail ? (
            <div className="p-4 text-sm text-muted-foreground">
              Cargando detalles...
            </div>
          ) : leadDetail ? (
            <div className="p-4 space-y-4">
              {/* Details grid */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <DetailItem
                  icon={<Building2 className="h-4 w-4" />}
                  label="Tipo Operacion"
                  value={leadDetail.tipoOperacion}
                />
                <DetailItem
                  icon={<FileText className="h-4 w-4" />}
                  label="Fuente"
                  value={leadDetail.fuente}
                />
                <DetailItem
                  icon={<Calendar className="h-4 w-4" />}
                  label="Fecha Contacto"
                  value={
                    leadDetail.fechaUltimoContacto
                      ? format(parseISO(leadDetail.fechaUltimoContacto), "dd MMM yyyy", {
                          locale: es,
                        })
                      : "Sin contacto"
                  }
                />
                <DetailItem
                  icon={<User className="h-4 w-4" />}
                  label="Autorizacion"
                  value={leadDetail.autorizacionDatos ? "Si" : "No"}
                />
              </div>

              {/* Interacciones */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <h4 className="font-medium text-sm text-foreground">
                    Interacciones
                  </h4>
                  <button
                    onClick={(e) => {
                      e.stopPropagation()
                      setShowInteraccionForm(!showInteraccionForm)
                    }}
                    className="text-xs text-[#1a4d2e] dark:text-[#22c55e] hover:underline flex items-center gap-1"
                  >
                    <Plus className="h-3 w-3" />
                    Registrar
                  </button>
                </div>

                {showInteraccionForm && (
                  <div className="mb-3 p-3 bg-muted rounded-lg space-y-2">
                    <div className="flex gap-2">
                      <select
                        value={tipoInteraccion}
                        onChange={(e) => setTipoInteraccion(e.target.value)}
                        className="px-2 py-1 text-sm border rounded bg-background"
                      >
                        <option>Llamada</option>
                        <option>Email</option>
                        <option>WhatsApp</option>
                        <option>Visita</option>
                        <option>Reunion</option>
                      </select>
                      <input
                        type="text"
                        value={resumen}
                        onChange={(e) => setResumen(e.target.value)}
                        placeholder="Resumen..."
                        className="flex-1 px-2 py-1 text-sm border rounded bg-background"
                      />
                      <button
                        onClick={handleRegistrarInteraccion}
                        disabled={registrarInteraccion.isPending}
                        className="px-3 py-1 text-xs bg-[#1a4d2e] dark:bg-[#22c55e] text-white rounded"
                      >
                        Guardar
                      </button>
                    </div>
                  </div>
                )}

                {loadingInteracciones ? (
                  <div className="text-sm text-muted-foreground">
                    Cargando...
                  </div>
                ) : interacciones?.length === 0 ? (
                  <p className="text-sm text-muted-foreground">
                    Sin interacciones registradas
                  </p>
                ) : (
                  <div className="space-y-2">
                    {interacciones?.map((inter: any) => (
                      <div
                        key={inter.id}
                        className="flex items-start gap-2 p-2 rounded bg-muted/50"
                      >
                        <MessageSquare className="h-4 w-4 text-muted-foreground mt-0.5" />
                        <div>
                          <p className="text-sm font-medium">{inter.tipo}</p>
                          {inter.resumen && (
                            <p className="text-xs text-muted-foreground">
                              {inter.resumen}
                            </p>
                          )}
                          <p className="text-xs text-muted-foreground mt-1">
                            {format(
                              parseISO(inter.fecha),
                              "dd MMM yyyy HH:mm",
                              { locale: es }
                            )}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          ) : null}
        </div>
      )}
    </div>
  )
}

function DetailItem({
  icon,
  label,
  value,
}: {
  icon: React.ReactNode
  label: string
  value: string
}) {
  return (
    <div className="flex items-start gap-2">
      <div className="text-muted-foreground mt-0.5">{icon}</div>
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="text-sm font-medium text-foreground">{value}</p>
      </div>
    </div>
  )
}
