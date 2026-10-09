import { useCallback, useEffect, useState } from "react"
import { usePipeline, useMoveLeadInPipeline } from "@/hooks/useApi"
import { ETAPAS_PIPELINE, ETIQUETAS_PIPELINE } from "@/lib/constants"
import { cn } from "@/lib/utils"
import {
  DragDropContext,
  Droppable,
  Draggable,
  type DropResult,
} from "@hello-pangea/dnd"
import { User, Phone, Mail, Clock, MoreHorizontal, AlertCircle } from "lucide-react"
import { format, parseISO } from "date-fns"
import { es } from "date-fns/locale"

export function PipelinePage() {
  const [query, setQuery] = useState("")
  const [operation, setOperation] = useState("")
  const [from, setFrom] = useState("")
  const [to, setTo] = useState("")
  const [page, setPage] = useState(1)
  const pageSize = 50
  const { data: pipeline, isLoading } = usePipeline({ q: query, operacion: operation, desde: from, hasta: to, page, pageSize })
  const moveLead = useMoveLeadInPipeline()
  const pageCount = Math.max(1, Math.ceil((pipeline?.total ?? 0) / pageSize))
  useEffect(() => setPage(1), [query, operation, from, to])

  const etapas = [...new Set([...ETAPAS_PIPELINE.VENTA, ...ETAPAS_PIPELINE.ARRIENDO])]

  const leadsPorEtapa = etapas.reduce(
    (acc: Record<string, any[]>, etapa) => {
      acc[etapa] =
        pipeline?.leads?.filter((l: any) => l.etapaPipeline === etapa) || []
      return acc
    },
    {}
  )

  const handleDragEnd = useCallback(
    (result: DropResult) => {
      if (!result.destination) return

      const { source, destination, draggableId } = result

      if (source.droppableId === destination.droppableId) return

      moveLead.mutate({
        leadId: draggableId,
        nuevaEtapa: destination.droppableId,
      })
    },
    [moveLead]
  )

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-muted-foreground">Cargando pipeline...</div>
      </div>
    )
  }

  return (
    <div className="h-full flex flex-col">
      <div className="mb-7">
        <span className="crm-eyebrow">Oportunidades</span>
        <h1 className="crm-page-title mt-2">Pipeline</h1>
        <p className="mt-3 text-sm text-muted-foreground">
          Arrastra las oportunidades entre etapas para actualizar su estado
        </p>
      </div>

      <div className="mb-5 grid gap-3 rounded-md border border-border bg-card p-4 md:grid-cols-[minmax(0,1fr)_160px_150px_150px_auto]">
        <input aria-label="Buscar oportunidad" value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Buscar nombre, correo o teléfono" className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" />
        <select aria-label="Filtrar operación" value={operation} onChange={(event) => setOperation(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground"><option value="">Venta y arriendo</option><option value="Venta">Venta</option><option value="Arriendo">Arriendo</option></select>
        <label className="text-xs text-muted-foreground">Desde<input aria-label="Desde" type="date" value={from} max={to || undefined} onChange={(event) => setFrom(event.target.value)} className="mt-1 block w-full rounded border border-border bg-background px-2 py-2 text-foreground" /></label>
        <label className="text-xs text-muted-foreground">Hasta<input aria-label="Hasta" type="date" value={to} min={from || undefined} onChange={(event) => setTo(event.target.value)} className="mt-1 block w-full rounded border border-border bg-background px-2 py-2 text-foreground" /></label>
        <div className="flex items-end text-xs text-muted-foreground">{pipeline?.total ?? 0} oportunidades · página {page}/{pageCount}</div>
      </div>

      {moveLead.isError && (
        <div role="alert" className="mb-4 flex items-center gap-2 rounded-md border border-destructive/40 bg-destructive/10 p-3 text-sm text-destructive">
          <AlertCircle className="h-4 w-4 flex-shrink-0" />
          <span>
            {moveLead.error?.message || "No se pudo cambiar de etapa la oportunidad."}
          </span>
        </div>
      )}

      <DragDropContext onDragEnd={handleDragEnd}>
        <div className="flex-1 overflow-x-auto">
          <div className="flex min-w-max gap-4 pb-4">
            {etapas.map((etapa) => (
              <div
                key={etapa}
                className="flex w-72 flex-shrink-0 flex-col rounded-lg border border-border bg-card/50"
              >
                {/* Column header */}
                <div className="border-b border-border px-4 py-4">
                  <div className="flex items-center justify-between">
                    <h3 className="font-display text-lg text-foreground">
                      {ETIQUETAS_PIPELINE[etapa] || etapa}
                    </h3>
                    <span className="rounded-full border border-border bg-background px-2 py-0.5 text-xs text-muted-foreground">
                      {leadsPorEtapa[etapa]?.length || 0}
                    </span>
                  </div>
                </div>

                {/* Droppable area */}
                <Droppable droppableId={etapa}>
                  {(provided, snapshot) => (
                    <div
                      ref={provided.innerRef}
                      {...provided.droppableProps}
                      className={cn(
                        "min-h-[200px] flex-1 space-y-2 p-3 transition-colors",
                        snapshot.isDraggingOver && "bg-secondary/60"
                      )}
                    >
                      {leadsPorEtapa[etapa]?.map(
                        (lead: any, index: number) => (
                          <Draggable
                            key={lead.id}
                            draggableId={lead.id}
                            index={index}
                          >
                            {(provided, snapshot) => (
                              <div
                                ref={provided.innerRef}
                                {...provided.draggableProps}
                                {...provided.dragHandleProps}
                                className={cn(
                                  "crm-panel cursor-grab p-4 shadow-sm transition-shadow active:cursor-grabbing",
                                  snapshot.isDragging && "shadow-md"
                                )}
                              >
                                {/* Lead name */}
                                <div className="flex items-center justify-between mb-2">
                                  <h4 className="font-medium text-sm text-foreground truncate">
                                    {lead.nombre}
                                  </h4>
                                  <button className="rounded-sm p-1 hover:bg-muted" aria-label="Más opciones">
                                    <MoreHorizontal className="h-4 w-4 text-muted-foreground" />
                                  </button>
                                </div>

                                {/* Contact info */}
                                <div className="space-y-1">
                                  {lead.email && (
                                    <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                                      <Mail className="h-3 w-3" />
                                      <span className="truncate">
                                        {lead.email}
                                      </span>
                                    </div>
                                  )}
                                  {lead.telefono && (
                                    <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                                      <Phone className="h-3 w-3" />
                                      <span>{lead.telefono}</span>
                                    </div>
                                  )}
                                </div>

                                {/* Footer */}
                                <div className="mt-2 pt-2 border-t flex items-center justify-between">
                                  <span className="text-xs text-muted-foreground flex items-center gap-1">
                                    <Clock className="h-3 w-3" />
                                    {format(
                                      parseISO(lead.fechaCreacion),
                                      "dd MMM",
                                      { locale: es }
                                    )}
                                  </span>
                                  {lead.asesorNombre && (
                                    <div className="flex items-center gap-1">
                                      <div className="flex h-5 w-5 items-center justify-center rounded-sm bg-secondary">
                                        <User className="h-3 w-3 text-foreground" />
                                      </div>
                                      <span className="text-xs text-muted-foreground">
                                        {lead.asesorNombre}
                                      </span>
                                    </div>
                                  )}
                                </div>
                              </div>
                            )}
                          </Draggable>
                        )
                      )}
                      {provided.placeholder}
                    </div>
                  )}
                </Droppable>
              </div>
            ))}
          </div>
        </div>
      </DragDropContext>
      <div className="mt-3 flex items-center justify-between border-t border-border pt-3 text-xs">
        <span className="text-muted-foreground">{pipeline?.total ?? 0} coincidencias</span>
        <span className="flex gap-2"><button disabled={page <= 1} onClick={() => setPage((value) => value - 1)} className="rounded border border-border px-3 py-2 disabled:opacity-40">Anterior</button><button disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)} className="rounded border border-border px-3 py-2 disabled:opacity-40">Siguiente</button></span>
      </div>
    </div>
  )
}
