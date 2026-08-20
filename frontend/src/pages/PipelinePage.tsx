import { useState, useCallback } from "react"
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
      <div className="mb-4">
        <h1 className="text-2xl font-bold text-foreground">Pipeline</h1>
        <p className="text-sm text-muted-foreground">
          Arrastra los leads entre etapas para actualizar su estado
        </p>
      </div>

      <DragDropContext onDragEnd={handleDragEnd}>
        <div className="flex-1 overflow-x-auto">
          <div className="flex gap-3 min-w-max pb-4">
            {etapas.map((etapa) => (
              <div
                key={etapa}
                className="w-72 flex-shrink-0 flex flex-col bg-muted/50 rounded-lg"
              >
                {/* Column header */}
                <div className="p-3 border-b">
                  <div className="flex items-center justify-between">
                    <h3 className="font-medium text-sm text-foreground">
                      {ETIQUETAS_PIPELINE[etapa] || etapa}
                    </h3>
                    <span className="text-xs text-muted-foreground bg-background px-2 py-0.5 rounded-full">
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
                        "flex-1 p-2 space-y-2 min-h-[200px] transition-colors",
                        snapshot.isDraggingOver && "bg-[#1a4d2e]/5 dark:bg-[#22c55e]/5"
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
                                  "bg-card rounded-lg border p-3 shadow-sm cursor-grab active:cursor-grabbing transition-shadow",
                                  snapshot.isDragging && "shadow-md"
                                )}
                              >
                                {/* Lead name */}
                                <div className="flex items-center justify-between mb-2">
                                  <h4 className="font-medium text-sm text-foreground truncate">
                                    {lead.nombre}
                                  </h4>
                                  <button className="p-1 hover:bg-accent rounded">
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
                                      <div className="h-5 w-5 rounded-full bg-[#1a4d2e] dark:bg-[#22c55e] flex items-center justify-center">
                                        <User className="h-3 w-3 text-white" />
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
