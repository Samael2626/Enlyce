import { useEffect, useState, type FormEvent } from "react"
import { api } from "@/api/client"
import { useAdvisors, useBulkAssignLeads, usePipeline, useLead, useInteracciones, useRegistrarInteraccion, useCreateLead, useLeadHistory } from "@/hooks/useApi"
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
  X,
  ChevronLeft,
  ChevronRight,
} from "lucide-react"
import { format, parseISO, formatDistanceToNow } from "date-fns"
import { es } from "date-fns/locale"
import { ETIQUETAS_PIPELINE } from "@/lib/constants"

const OWNER_SERVICE_LABELS: Record<string, string> = {
  Sell: "Venta",
  Rent: "Arriendo",
  Manage: "Administración",
  Valuation: "Valoración",
}

const OWNER_PROPERTY_TYPE_LABELS: Record<string, string> = {
  Apartment: "Apartamento",
  House: "Casa",
  CommercialSpace: "Local comercial",
  Office: "Oficina",
  Lot: "Lote",
  CountryHouse: "Casa campestre",
  Other: "Otro",
}

const CONTACT_CHANNEL_LABELS: Record<string, string> = {
  WhatsApp: "WhatsApp",
  Phone: "Llamada",
  Email: "Correo electrónico",
}

const currency = new Intl.NumberFormat("es-CO", {
  style: "currency",
  currency: "COP",
  maximumFractionDigits: 0,
})

export function LeadsPage() {
  const user = useAuthStore((state) => state.user)
  const pageSize = 20
  const [search, setSearch] = useState("")
  const [stage, setStage] = useState("")
  const [operation, setOperation] = useState("")
  const [createdFrom, setCreatedFrom] = useState("")
  const [createdTo, setCreatedTo] = useState("")
  const [page, setPage] = useState(1)
  const [selectedLeadIds, setSelectedLeadIds] = useState<string[]>([])
  const [targetAdvisorId, setTargetAdvisorId] = useState("")
  const [assignmentReason, setAssignmentReason] = useState("")
  const filters = { q: search, etapa: stage, operacion: operation, desde: createdFrom, hasta: createdTo, page, pageSize }
  const { data: pipeline, isLoading } = usePipeline(filters)
  const advisorQuery = useAdvisors(user?.rol === "Administrador")
  const bulkAssign = useBulkAssignLeads()
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [showNewLead, setShowNewLead] = useState(false)
  const [exportFrom, setExportFrom] = useState(() => `${new Date().getFullYear()}-01-01`)
  const [exportTo, setExportTo] = useState(() => {
    const today = new Date()
    return `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, "0")}-${String(today.getDate()).padStart(2, "0")}`
  })
  const [exportError, setExportError] = useState<string | null>(null)

  useEffect(() => { setPage(1) }, [search, stage, operation, createdFrom, createdTo])
  useEffect(() => { setSelectedLeadIds([]) }, [search, stage, operation, createdFrom, createdTo, page])

  const leads = pipeline?.leads || []
  const pageCount = Math.max(1, Math.ceil((pipeline?.total ?? 0) / pageSize))

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-muted-foreground">Cargando oportunidades...</div>
      </div>
    )
  }

  return (
    <div className="space-y-7">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Relaciones</span>
          <h1 className="crm-page-title mt-2">Oportunidades</h1>
          <p className="mt-3 text-sm text-muted-foreground">
            {pipeline?.total ?? 0} oportunidades · página {page} de {pageCount}
          </p>
        </div>
        <div className="flex flex-wrap items-end gap-2">
          <label className="text-xs text-muted-foreground">
            Desde
            <input type="date" value={exportFrom} onChange={(event) => setExportFrom(event.target.value)} className="mt-1 block rounded-sm border border-input bg-background px-2 py-2 text-foreground" />
          </label>
          <label className="text-xs text-muted-foreground">
            Hasta
            <input type="date" value={exportTo} onChange={(event) => setExportTo(event.target.value)} className="mt-1 block rounded-sm border border-input bg-background px-2 py-2 text-foreground" />
          </label>
          <button className="crm-button" onClick={async () => {
            setExportError(null)
            if (!exportFrom || !exportTo || exportFrom > exportTo) {
              setExportError("El rango de fechas no es válido.")
              return
            }
            try {
              const url = URL.createObjectURL(await api.downloadOpportunitiesCsv(exportFrom, exportTo))
              const link = document.createElement("a")
              link.href = url
              link.download = `oportunidades-${exportFrom}-${exportTo}.csv`
              link.click()
              URL.revokeObjectURL(url)
            } catch {
              setExportError("No se pudo exportar el archivo.")
            }
          }}>
            Descargar CSV
          </button>
          <button className="crm-button" onClick={() => setShowNewLead(true)}>
            <Plus className="h-4 w-4" />
            Nueva oportunidad
          </button>
        </div>
      </div>
      {exportError && <p role="alert" className="text-sm text-destructive">{exportError}</p>}

      <section className="crm-panel space-y-3 p-4" aria-label="Filtros de oportunidades">
        <div className="grid gap-3 md:grid-cols-5">
          <input aria-label="Buscar oportunidades" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Nombre, correo o teléfono" className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground md:col-span-2" />
          <select aria-label="Filtrar por etapa" value={stage} onChange={(event) => setStage(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground"><option value="">Todas las etapas</option>{Object.entries(ETIQUETAS_PIPELINE).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</select>
          <select aria-label="Filtrar por operación" value={operation} onChange={(event) => setOperation(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground"><option value="">Venta y arriendo</option><option value="Venta">Venta</option><option value="Arriendo">Arriendo</option></select>
          <label className="text-xs text-muted-foreground">Desde<input aria-label="Filtrar desde" type="date" value={createdFrom} max={createdTo || undefined} onChange={(event) => setCreatedFrom(event.target.value)} className="mt-1 block w-full rounded border border-border bg-background px-2 py-2 text-foreground" /></label>
          <label className="text-xs text-muted-foreground">Hasta<input aria-label="Filtrar hasta" type="date" value={createdTo} min={createdFrom || undefined} onChange={(event) => setCreatedTo(event.target.value)} className="mt-1 block w-full rounded border border-border bg-background px-2 py-2 text-foreground" /></label>
        </div>
        {user?.rol === "Administrador" && <div className="flex flex-wrap items-end gap-2 border-t border-border pt-3">
          <label className="flex items-center gap-2 text-xs text-muted-foreground"><input type="checkbox" checked={leads.length > 0 && leads.every((lead) => selectedLeadIds.includes(lead.id))} onChange={(event) => setSelectedLeadIds(event.target.checked ? leads.map((lead) => lead.id) : [])} />Seleccionar página</label>
          <label className="text-xs text-muted-foreground">Asignar responsable<select value={targetAdvisorId} onChange={(event) => setTargetAdvisorId(event.target.value)} className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-foreground"><option value="">Elegir asesor</option>{(advisorQuery.data ?? []).map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}</select></label>
          <label className="min-w-56 flex-1 text-xs text-muted-foreground">Motivo<input value={assignmentReason} onChange={(event) => setAssignmentReason(event.target.value)} maxLength={500} placeholder="Motivo de reasignación" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-foreground" /></label>
          <button disabled={!selectedLeadIds.length || !targetAdvisorId || !assignmentReason.trim() || bulkAssign.isPending} onClick={() => bulkAssign.mutate({ leadIds: selectedLeadIds, advisorId: targetAdvisorId, reason: assignmentReason }, { onSuccess: () => { setSelectedLeadIds([]); setAssignmentReason("") } })} className="crm-button disabled:opacity-40">Asignar seleccionadas ({selectedLeadIds.length})</button>
        </div>}
        {bulkAssign.data && <p role="status" className="text-sm text-muted-foreground">Asignadas: {bulkAssign.data.succeeded} · fallidas: {bulkAssign.data.failed}</p>}
        {bulkAssign.isError && <p role="alert" className="text-sm text-destructive">No se pudo completar la asignación masiva.</p>}
      </section>

      {showNewLead && <NewLeadModal onClose={() => setShowNewLead(false)} />}

      <div className="space-y-3 border-t border-border pt-6">
        {leads.length === 0 ? (
          <div className="crm-panel py-14 text-center">
            <User className="h-12 w-12 text-muted-foreground mx-auto mb-3" />
            <h3 className="font-display text-2xl">Primera oportunidad</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Agrega tu primera oportunidad para comenzar
            </p>
          </div>
        ) : (
          leads.map((lead: any) => (
            <LeadCard
              key={lead.id}
              lead={lead}
              isExpanded={expandedId === lead.id}
              selected={selectedLeadIds.includes(lead.id)}
              onSelect={(checked) => setSelectedLeadIds((ids) => checked ? [...ids, lead.id] : ids.filter((id) => id !== lead.id))}
              onToggle={() =>
                setExpandedId(expandedId === lead.id ? null : lead.id)
              }
            />
          ))
        )}
      </div>
      <div className="flex items-center justify-between border-t border-border pt-3 text-xs">
        <span className="text-muted-foreground">{pipeline?.total ?? 0} coincidencias</span>
        <span className="flex gap-2"><button aria-label="Página anterior" disabled={page <= 1} onClick={() => setPage((value) => value - 1)} className="rounded border border-border px-3 py-2 disabled:opacity-40">Anterior</button><button aria-label="Página siguiente" disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)} className="rounded border border-border px-3 py-2 disabled:opacity-40">Siguiente</button></span>
      </div>
    </div>
  )
}

function NewLeadModal({ onClose }: { onClose: () => void }) {
  const createLead = useCreateLead()
  const [form, setForm] = useState({
    nombre: "",
    email: "",
    telefono: "",
    fuente: "CRM manual",
    tipoOperacion: "Venta",
    autorizacionDatos: false,
  })

  useEffect(() => {
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !createLead.isPending) onClose()
    }
    document.addEventListener("keydown", closeOnEscape)
    return () => document.removeEventListener("keydown", closeOnEscape)
  }, [createLead.isPending, onClose])

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    createLead.mutate(
      {
        nombre: form.nombre.trim(),
        email: form.email.trim(),
        telefono: form.telefono.trim() || undefined,
        fuente: form.fuente.trim() || "CRM manual",
        tipoOperacion: form.tipoOperacion,
        autorizacionDatos: form.autorizacionDatos,
      },
      { onSuccess: onClose }
    )
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-[#060c20]/75 p-0 backdrop-blur-sm sm:items-center sm:p-6"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && !createLead.isPending) onClose()
      }}
    >
      <section
        role="dialog"
        aria-modal="true"
        aria-labelledby="new-lead-title"
        className="w-full max-w-2xl border-t-4 border-accent bg-card p-6 shadow-2xl sm:p-8"
      >
        <div className="flex items-start justify-between gap-6">
          <div>
            <span className="crm-eyebrow">Captación manual</span>
            <h2 id="new-lead-title" className="mt-2 font-display text-3xl">Nueva oportunidad</h2>
            <p className="mt-2 text-sm text-muted-foreground">Registra el contacto y su interés inicial.</p>
          </div>
          <button
            type="button"
            aria-label="Cerrar"
            disabled={createLead.isPending}
            onClick={onClose}
            className="p-2 text-muted-foreground transition-colors hover:text-foreground disabled:opacity-40"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <form className="mt-7 space-y-5" onSubmit={handleSubmit}>
          <div className="grid gap-5 sm:grid-cols-2">
            <label className="text-sm font-semibold sm:col-span-2">
              Nombre completo
              <input
                autoFocus
                required
                value={form.nombre}
                onChange={(event) => setForm({ ...form, nombre: event.target.value })}
                className="mt-1 block w-full rounded-sm border border-input bg-background px-3 py-2.5 text-foreground"
                placeholder="Ej. Laura Gómez"
              />
            </label>
            <label className="text-sm font-semibold">
              Correo electrónico
              <input
                required
                type="email"
                value={form.email}
                onChange={(event) => setForm({ ...form, email: event.target.value })}
                className="mt-1 block w-full rounded-sm border border-input bg-background px-3 py-2.5 text-foreground"
                placeholder="laura@correo.com"
              />
            </label>
            <label className="text-sm font-semibold">
              Teléfono
              <input
                type="tel"
                value={form.telefono}
                onChange={(event) => setForm({ ...form, telefono: event.target.value })}
                className="mt-1 block w-full rounded-sm border border-input bg-background px-3 py-2.5 text-foreground"
                placeholder="300 000 0000"
              />
            </label>
            <label className="text-sm font-semibold">
              Operación de interés
              <select
                value={form.tipoOperacion}
                onChange={(event) => setForm({ ...form, tipoOperacion: event.target.value })}
                className="mt-1 block w-full rounded-sm border border-input bg-background px-3 py-2.5 text-foreground"
              >
                <option value="Venta">Compra</option>
                <option value="Arriendo">Arriendo</option>
              </select>
            </label>
            <label className="text-sm font-semibold">
              Fuente
              <input
                value={form.fuente}
                onChange={(event) => setForm({ ...form, fuente: event.target.value })}
                className="mt-1 block w-full rounded-sm border border-input bg-background px-3 py-2.5 text-foreground"
                placeholder="Referido, llamada, portal..."
              />
            </label>
          </div>

          <label className="flex items-start gap-3 border border-border bg-muted/45 p-4 text-sm">
            <input
              required
              type="checkbox"
              checked={form.autorizacionDatos}
              onChange={(event) => setForm({ ...form, autorizacionDatos: event.target.checked })}
              className="mt-1 h-4 w-4 accent-[#b78a2d]"
            />
            <span>
              Confirmo que la persona autorizó el tratamiento de sus datos personales según la política vigente.
            </span>
          </label>

          {createLead.isError && (
            <p role="alert" className="border-l-4 border-destructive bg-destructive/10 px-4 py-3 text-sm text-destructive">
              {createLead.error instanceof Error ? createLead.error.message : "No fue posible crear la oportunidad."}
            </p>
          )}

          <div className="flex flex-col-reverse gap-3 border-t border-border pt-5 sm:flex-row sm:justify-end">
            <button type="button" onClick={onClose} disabled={createLead.isPending} className="min-h-11 border border-border bg-card px-5 text-sm font-bold hover:bg-muted disabled:opacity-40">
              Cancelar
            </button>
            <button type="submit" disabled={createLead.isPending} className="crm-button min-w-36">
              {createLead.isPending ? "Guardando..." : "Guardar oportunidad"}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}

function LeadCard({
  lead,
  isExpanded,
  selected,
  onSelect,
  onToggle,
}: {
  lead: any
  isExpanded: boolean
  selected: boolean
  onSelect: (checked: boolean) => void
  onToggle: () => void
}) {
  const { data: leadDetail, isLoading: loadingDetail } = useLead(
    isExpanded ? lead.id : ""
  )
  const { data: interacciones, isLoading: loadingInteracciones } =
    useInteracciones(isExpanded ? lead.id : "")
  const { data: history, isLoading: loadingHistory } = useLeadHistory(isExpanded ? lead.id : "")
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
        <input aria-label={`Seleccionar oportunidad de ${lead.nombre}`} type="checkbox" checked={selected} onClick={(event) => event.stopPropagation()} onChange={(event) => { event.stopPropagation(); onSelect(event.target.checked) }} />
        <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-sm bg-secondary">
          <User className="h-5 w-5 text-foreground" />
        </div>

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="truncate font-display text-lg text-foreground">
              {lead.nombre}
            </h3>
            <span className="rounded-full bg-secondary px-2 py-0.5 text-[.68rem] font-semibold text-secondary-foreground">
              {ETIQUETAS_PIPELINE[lead.etapaPipeline] || lead.etapaPipeline}
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

              {leadDetail.ownerService && (
                <section className="border-l-2 border-accent bg-muted/35 p-4">
                  <p className="crm-eyebrow">Solicitud de propietario</p>
                  <div className="mt-4 grid grid-cols-2 gap-4 md:grid-cols-3">
                    <DetailItem
                      icon={<Building2 className="h-4 w-4" />}
                      label="Servicio"
                      value={OWNER_SERVICE_LABELS[leadDetail.ownerService] ?? leadDetail.ownerService}
                    />
                    <DetailItem
                      icon={<Building2 className="h-4 w-4" />}
                      label="Inmueble"
                      value={leadDetail.ownerPropertyType
                        ? OWNER_PROPERTY_TYPE_LABELS[leadDetail.ownerPropertyType] ?? leadDetail.ownerPropertyType
                        : "Pendiente"}
                    />
                    <DetailItem
                      icon={<FileText className="h-4 w-4" />}
                      label="Ubicación"
                      value={leadDetail.ownerPropertyCity
                        ? [leadDetail.ownerPropertyNeighborhood, leadDetail.ownerPropertyCity].filter(Boolean).join(", ")
                        : "Pendiente"}
                    />
                    <DetailItem
                      icon={<FileText className="h-4 w-4" />}
                      label="Precio esperado"
                      value={leadDetail.ownerExpectedPrice
                        ? currency.format(leadDetail.ownerExpectedPrice)
                        : "No informado"}
                    />
                    <DetailItem
                      icon={<Phone className="h-4 w-4" />}
                      label="Canal preferido"
                      value={leadDetail.ownerPreferredContactChannel
                        ? CONTACT_CHANNEL_LABELS[leadDetail.ownerPreferredContactChannel] ?? leadDetail.ownerPreferredContactChannel
                        : "No informado"}
                    />
                  </div>
                  {leadDetail.ownerPropertyMessage && (
                    <div className="mt-4 border-t border-border pt-3">
                      <p className="text-xs text-muted-foreground">Mensaje del propietario</p>
                      <p className="mt-1 text-sm leading-6 text-foreground">{leadDetail.ownerPropertyMessage}</p>
                    </div>
                  )}
                </section>
              )}

              <section aria-label="Historial de oportunidad">
                <h4 className="mb-2 text-sm font-medium text-foreground">Historial de oportunidad</h4>
                {loadingHistory ? <p className="text-sm text-muted-foreground">Cargando historial…</p> :
                  !history?.length ? <p className="text-sm text-muted-foreground">Sin cambios registrados.</p> :
                  <ol className="space-y-2">
                    {history.map((item) => <li key={item.id} className="border-l-2 border-accent bg-muted/35 px-3 py-2 text-sm">
                      <div className="flex flex-wrap justify-between gap-2">
                        <strong>{item.type}: {item.from} → {item.to}</strong>
                        <time className="text-xs text-muted-foreground">{format(parseISO(item.occurredAt), "dd MMM yyyy HH:mm", { locale: es })}</time>
                      </div>
                      <p className="mt-1 text-xs text-muted-foreground">{item.reason} · Por {item.actor}</p>
                    </li>)}
                  </ol>}
              </section>

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

const assignmentSourceLabels: Record<string, string> = {
  Publication: "Asesor de publicación",
  AutomaticLoadBalance: "Menor carga abierta",
  AutomaticRoundRobin: "Turnos rotativos",
  ManualAssignment: "Asignación manual",
  ManualReassignment: "Reasignación manual",
  Legacy: "Asignación previa al historial",
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
