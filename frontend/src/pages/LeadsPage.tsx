import { useState } from "react"
import { usePipeline, useLead, useInteracciones, useRegistrarInteraccion } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
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
    <div className="space-y-7">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Relaciones</span>
          <h1 className="crm-page-title mt-2">Leads</h1>
          <p className="mt-3 text-sm text-muted-foreground">
            {leads.length} leads registrados
          </p>
        </div>
        <button className="crm-button">
          <Plus className="h-4 w-4" />
          Nuevo Lead
        </button>
      </div>

      <div className="space-y-3 border-t border-border pt-6">
        {leads.length === 0 ? (
          <div className="crm-panel py-14 text-center">
            <User className="h-12 w-12 text-muted-foreground mx-auto mb-3" />
            <h3 className="font-display text-2xl">Primer lead</h3>
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
    <div className="crm-panel overflow-hidden">
      {/* Compact view */}
      <div
        className="flex cursor-pointer items-center gap-4 p-4 transition-colors hover:bg-muted/40 md:p-5"
        onClick={onToggle}
      >
        <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-sm bg-secondary">
          <User className="h-5 w-5 text-foreground" />
        </div>

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="truncate font-display text-lg text-foreground">
              {lead.nombre}
            </h3>
            <span className="rounded-full bg-secondary px-2 py-0.5 text-[.68rem] font-semibold text-secondary-foreground">
              {lead.etapaPipeline}
            </span>
          </div>
          <div className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
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
                  label="Tipo de operación"
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
                  label="Autorización"
                  value={leadDetail.autorizacionDatos ? "Sí" : "No"}
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
                    className="flex items-center gap-1 text-xs font-semibold text-foreground hover:underline"
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
                        <option value="Reunion">Reunión</option>
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
                        className="crm-button min-h-8 px-3 py-1 text-xs"
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
