import { useCallback } from "react"
import { usePipeline, useMoveLeadInPipeline } from "@/hooks/useApi"
import { ETAPAS_PIPELINE, ETIQUETAS_PIPELINE } from "@/lib/constants"
import { cn } from "@/lib/utils"
import {
  DragDropContext,
  Droppable,
  Draggable,
  type DropResult,
} from "@hello-pangea/dnd"
import { User, Phone, Mail, Clock, MoreHorizontal } from "lucide-react"
import { format, parseISO } from "date-fns"
import { es } from "date-fns/locale"

export function PipelinePage() {
  const { data: pipeline, isLoading } = usePipeline()
  const moveLead = useMoveLeadInPipeline()

  const etapas = ETAPAS_PIPELINE.VENTA

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
          Arrastra los leads entre etapas para actualizar su estado
        </p>
      </div>

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
    </div>
  )
}
