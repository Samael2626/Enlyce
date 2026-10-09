import { useEffect, useMemo, useState } from "react"
import { CalendarClock, Check, ChevronLeft, ChevronRight, Clock3 } from "lucide-react"
import { useBulkCancelCommercialTasks, useBulkCompleteCommercialTasks, useCommercialTasks, useCompleteCommercialTask, useCancelCommercialTask, useManageVisit, useRescheduleCommercialTask, useVisitas } from "@/hooks/useApi"

export function CalendarPage() {
  const [anchor] = useState(() => new Date())
  const [newDates, setNewDates] = useState<Record<string, string>>({})
  const [visitDates, setVisitDates] = useState<Record<string, string>>({})
  const [visitFeedback, setVisitFeedback] = useState<Record<string, string>>({})
  const [query, setQuery] = useState("")
  const [status, setStatus] = useState("")
  const [priority, setPriority] = useState("")
  const [page, setPage] = useState(1)
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const pageSize = 20
  const from = useMemo(() => new Date(anchor.getFullYear(), anchor.getMonth(), 1).toISOString(), [anchor])
  const to = useMemo(() => new Date(anchor.getFullYear(), anchor.getMonth() + 1, 1).toISOString(), [anchor])
  const tasksQuery = useCommercialTasks({ from, to, query, status, priority, page, pageSize })
  const completeTask = useCompleteCommercialTask()
  const cancelTask = useCancelCommercialTask()
  const bulkComplete = useBulkCompleteCommercialTasks()
  const bulkCancel = useBulkCancelCommercialTasks()
  const rescheduleTask = useRescheduleCommercialTask()
  const visitsQuery = useVisitas()
  const manageVisit = useManageVisit()
  const tasks = tasksQuery.data?.items ?? []
  const totalTasks = tasksQuery.data?.total ?? 0
  const pageCount = Math.max(1, Math.ceil(totalTasks / pageSize))
  useEffect(() => setSelectedIds([]), [query, status, priority, page])
  const monthVisits = (visitsQuery.data ?? []).filter((visit) => {
    const date = new Date(visit.fechaProgramada)
    return date.getFullYear() === anchor.getFullYear() && date.getMonth() === anchor.getMonth()
  })

  return (
    <div className="space-y-6">
      <header>
        <p className="crm-eyebrow">Operación diaria</p>
        <h1 className="mt-2 font-display text-3xl text-foreground">Agenda comercial</h1>
        <p className="mt-2 text-sm text-muted-foreground">Compromisos del mes, ordenados por fecha y prioridad.</p>
      </header>

      <section className="crm-panel overflow-hidden">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h2 className="font-display text-xl text-foreground">{anchor.toLocaleDateString("es-CO", { month: "long", year: "numeric" })}</h2>
          <span className="text-sm text-muted-foreground">{totalTasks} tareas</span>
        </div>
        <div className="grid gap-3 border-b border-border p-4 md:grid-cols-[minmax(0,1fr)_170px_150px_auto]">
          <input aria-label="Buscar tareas" value={query} onChange={(event) => { setQuery(event.target.value); setPage(1) }} placeholder="Buscar título o contacto" className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" />
          <select aria-label="Filtrar por estado" value={status} onChange={(event) => { setStatus(event.target.value); setPage(1) }} className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground">
            <option value="">Todos los estados</option><option value="Pending">Pendientes</option><option value="Completed">Completadas</option><option value="Cancelled">Canceladas</option>
          </select>
          <select aria-label="Filtrar por prioridad" value={priority} onChange={(event) => { setPriority(event.target.value); setPage(1) }} className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground">
            <option value="">Toda prioridad</option><option value="High">Alta</option><option value="Normal">Normal</option><option value="Low">Baja</option>
          </select>
          <div className="flex flex-wrap items-center gap-2 text-xs">
            <label className="flex items-center gap-2 text-muted-foreground"><input type="checkbox" checked={tasks.some((task) => task.status === "Pending") && tasks.filter((task) => task.status === "Pending").every((task) => selectedIds.includes(task.id))} onChange={(event) => setSelectedIds(event.target.checked ? tasks.filter((task) => task.status === "Pending").map((task) => task.id) : [])} />Pendientes página</label>
            <button disabled={!selectedIds.length || bulkComplete.isPending} onClick={() => bulkComplete.mutate(selectedIds, { onSettled: () => setSelectedIds([]) })} className="rounded bg-primary px-2.5 py-2 font-semibold text-primary-foreground disabled:opacity-40">Completar ({selectedIds.length})</button>
            <button disabled={!selectedIds.length || bulkCancel.isPending} onClick={() => bulkCancel.mutate(selectedIds, { onSettled: () => setSelectedIds([]) })} className="rounded border border-destructive/40 px-2.5 py-2 font-semibold text-destructive disabled:opacity-40">Cancelar</button>
          </div>
        </div>
        {(bulkComplete.isError || bulkCancel.isError) && <p role="alert" className="px-5 pt-3 text-sm text-destructive">No se pudo aplicar la acción masiva.</p>}
        {tasksQuery.isPending && <p className="p-6 text-sm text-muted-foreground">Cargando agenda…</p>}
        {tasksQuery.isError && <p role="alert" className="p-6 text-sm text-destructive">No se pudo cargar la agenda.</p>}
        {!tasksQuery.isPending && tasks.length === 0 && <p className="p-8 text-sm text-muted-foreground">No hay compromisos para este mes. Crea tareas desde la ficha de un contacto.</p>}
        <div className="divide-y divide-border">
          {tasks.map((task) => (
            <article key={task.id} className="grid gap-4 px-5 py-4 md:grid-cols-[minmax(0,1fr)_auto] md:items-center">
              <div className="flex gap-3">
                {task.status === "Pending" && <input aria-label={`Seleccionar ${task.title}`} type="checkbox" checked={selectedIds.includes(task.id)} onChange={(event) => setSelectedIds((ids) => event.target.checked ? [...ids, task.id] : ids.filter((id) => id !== task.id))} />}
                <span className="mt-0.5 rounded-md bg-accent/10 p-2 text-accent"><CalendarClock className="h-4 w-4" /></span>
                <div>
                  <p className="font-semibold text-foreground">{task.title}</p>
                  <p className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-muted-foreground">
                    <span>{task.type}</span><span>{task.priority}</span>
                    <span className="inline-flex items-center gap-1"><Clock3 className="h-3 w-3" />{new Date(task.dueAt).toLocaleString("es-CO")}</span>
                  </p>
                  {task.reminderAt && <p className="mt-1 text-xs text-muted-foreground">Recordatorio: {new Date(task.reminderAt).toLocaleString("es-CO")}</p>}
                  {task.description && <p className="mt-2 text-sm text-muted-foreground">{task.description}</p>}
                </div>
              </div>
              {task.status === "Pending" ? (
                <div className="flex flex-wrap items-center gap-2 md:justify-end">
                  <input
                    aria-label={`Nueva fecha para ${task.title}`}
                    type="datetime-local"
                    className="rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground"
                    value={newDates[task.id] ?? toLocalDateTimeInput(new Date(task.dueAt))}
                    onChange={(event) => setNewDates((current) => ({ ...current, [task.id]: event.target.value }))}
                  />
                  <button
                    onClick={() => rescheduleTask.mutate({ id: task.id, dueAt: new Date(newDates[task.id] ?? toLocalDateTimeInput(new Date(task.dueAt))).toISOString() })}
                    disabled={rescheduleTask.isPending}
                    className="rounded-md border border-border px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted disabled:opacity-50"
                  >Reprogramar</button>
                  <button
                    onClick={() => completeTask.mutate(task.id)}
                    disabled={completeTask.isPending}
                    className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-2 text-xs font-semibold text-primary-foreground disabled:opacity-50"
                  ><Check className="h-3.5 w-3.5" />Completar</button>
                  <button
                    onClick={() => cancelTask.mutate(task.id)}
                    disabled={cancelTask.isPending}
                    className="rounded-md border border-destructive/40 px-3 py-2 text-xs font-semibold text-destructive disabled:opacity-50"
                  >Cancelar</button>
                </div>
              ) : <span className="text-xs font-semibold text-muted-foreground">{task.status === "Completed" ? "Completada" : "Cancelada"}</span>}
            </article>
          ))}
        </div>
        <div className="flex items-center justify-between border-t border-border px-5 py-3 text-xs">
          <span className="text-muted-foreground">Página {page} de {pageCount}</span>
          <span className="flex gap-1"><button aria-label="Página anterior" disabled={page <= 1} onClick={() => { setPage((value) => value - 1); setSelectedIds([]) }} className="rounded border border-border p-1.5 disabled:opacity-40"><ChevronLeft className="h-4 w-4" /></button><button aria-label="Página siguiente" disabled={page >= pageCount} onClick={() => { setPage((value) => value + 1); setSelectedIds([]) }} className="rounded border border-border p-1.5 disabled:opacity-40"><ChevronRight className="h-4 w-4" /></button></span>
        </div>
      </section>

      <section className="crm-panel overflow-hidden">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h2 className="font-display text-xl text-foreground">Visitas del mes</h2>
          <span className="text-sm text-muted-foreground">{monthVisits.length} visitas</span>
        </div>
        {visitsQuery.isPending && <p className="p-6 text-sm text-muted-foreground">Cargando visitas…</p>}
        {visitsQuery.isError && <p role="alert" className="p-6 text-sm text-destructive">No se pudieron cargar las visitas.</p>}
        {!visitsQuery.isPending && monthVisits.length === 0 && <p className="p-8 text-sm text-muted-foreground">No hay visitas registradas este mes.</p>}
        <div className="divide-y divide-border">
          {monthVisits.map((visit) => (
            <article key={visit.id} className="grid gap-3 px-5 py-4 md:grid-cols-[minmax(0,1fr)_auto] md:items-center">
              <div>
                <p className="font-semibold text-foreground">Visita · inmueble {visit.inmuebleId.slice(0, 8)}</p>
                <p className="mt-1 text-xs text-muted-foreground">{new Date(visit.fechaProgramada).toLocaleString("es-CO")} · {visit.estado}</p>
                {visit.feedback && <p className="mt-1 text-sm text-muted-foreground">{visit.feedback}</p>}
                {visit.estado === "Programada" && <input className="mt-2 w-full max-w-md rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground" placeholder="Resultado o comentarios de la visita" value={visitFeedback[visit.id] ?? ""} onChange={(event) => setVisitFeedback((current) => ({ ...current, [visit.id]: event.target.value }))} />}
              </div>
              {visit.estado === "Programada" && (
                <div className="flex flex-wrap items-center gap-2 md:justify-end">
                  <input
                    aria-label="Nueva fecha de visita"
                    type="datetime-local"
                    className="rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground"
                    value={visitDates[visit.id] ?? toLocalDateTimeInput(new Date(visit.fechaProgramada))}
                    onChange={(event) => setVisitDates((current) => ({ ...current, [visit.id]: event.target.value }))}
                  />
                  <button onClick={() => manageVisit.mutate({ id: visit.id, action: "reschedule", value: new Date(visitDates[visit.id] ?? toLocalDateTimeInput(new Date(visit.fechaProgramada))).toISOString() })} disabled={manageVisit.isPending} className="rounded-md border border-border px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted disabled:opacity-50">Reprogramar</button>
                  <button onClick={() => manageVisit.mutate({ id: visit.id, action: "complete", value: visitFeedback[visit.id] })} disabled={manageVisit.isPending} className="rounded-md bg-primary px-3 py-2 text-xs font-semibold text-primary-foreground disabled:opacity-50">Registrar resultado</button>
                  <button onClick={() => manageVisit.mutate({ id: visit.id, action: "cancel" })} disabled={manageVisit.isPending} className="rounded-md border border-destructive/40 px-3 py-2 text-xs font-semibold text-destructive disabled:opacity-50">Cancelar</button>
                </div>
              )}
            </article>
          ))}
        </div>
      </section>
      {(completeTask.isError || cancelTask.isError || rescheduleTask.isError) && <p role="alert" className="text-sm text-destructive">No se pudo guardar el cambio. Revisa que la tarea siga pendiente.</p>}
      {manageVisit.isError && <p role="alert" className="text-sm text-destructive">No se pudo guardar el cambio de visita.</p>}
    </div>
  )
}

function toLocalDateTimeInput(date: Date) {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 16)
}
