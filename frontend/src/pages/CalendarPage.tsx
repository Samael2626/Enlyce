import { useEffect, useMemo, useState } from "react"
import { CalendarClock, Check, ChevronLeft, ChevronRight, Clock3 } from "lucide-react"
import { useBulkCancelCommercialTasks, useBulkCompleteCommercialTasks, useCommercialTasks, useCompleteCommercialTask, useCancelCommercialTask, useManageVisit, useRescheduleCommercialTask, useVisitas } from "@/hooks/useApi"

export function CalendarPage() {
  const [anchor, setAnchor] = useState(() => new Date())
  const [view, setView] = useState<"month" | "week" | "day">("month")
  const [newDates, setNewDates] = useState<Record<string, string>>({})
  const [visitDates, setVisitDates] = useState<Record<string, string>>({})
  const [visitFeedback, setVisitFeedback] = useState<Record<string, string>>({})
  const [query, setQuery] = useState("")
  const [status, setStatus] = useState("")
  const [priority, setPriority] = useState("")
  const [page, setPage] = useState(1)
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const pageSize = 100
  const from = useMemo(() => {
    const start = view === "month"
      ? new Date(anchor.getFullYear(), anchor.getMonth(), 1)
      : view === "week" ? startOfWeek(anchor) : new Date(anchor.getFullYear(), anchor.getMonth(), anchor.getDate())
    return start.toISOString()
  }, [anchor, view])
  const to = useMemo(() => {
    const end = view === "month"
      ? new Date(anchor.getFullYear(), anchor.getMonth() + 1, 1)
      : view === "week" ? addDays(startOfWeek(anchor), 7) : new Date(anchor.getFullYear(), anchor.getMonth(), anchor.getDate() + 1)
    return end.toISOString()
  }, [anchor, view])
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
  const periodVisits = (visitsQuery.data ?? []).filter((visit) => {
    const date = new Date(visit.fechaProgramada)
    return date >= new Date(from) && date < new Date(to)
  })
  const calendarDays = useMemo(() => getCalendarDays(anchor, view), [anchor, view])
  const calendarItems = useMemo(() => {
    const tasks = (tasksQuery.data?.items ?? []).map((task) => ({
      id: task.id, date: new Date(task.dueAt), title: task.title, kind: "Tarea", status: task.status,
    }))
    const visits = periodVisits.map((visit) => ({
      id: visit.id, date: new Date(visit.fechaProgramada), title: `Visita · inmueble ${visit.inmuebleId.slice(0, 8)}`, kind: "Visita", status: visit.estado,
    }))
    return [...tasks, ...visits]
  }, [tasksQuery.data?.items, periodVisits])

  return (
    <div className="space-y-6">
      <header>
        <p className="crm-eyebrow">Operación diaria</p>
        <h1 className="mt-2 font-display text-3xl text-foreground">Agenda comercial</h1>
        <p className="mt-2 text-sm text-muted-foreground">Tareas y visitas en calendario, con lista de acciones debajo.</p>
      </header>

      <section className="crm-panel overflow-hidden">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <div className="flex items-center gap-2">
            <button aria-label="Periodo anterior" onClick={() => { setAnchor((date) => shiftDate(date, view, -1)); setPage(1) }} className="rounded border border-border p-2 text-foreground"><ChevronLeft className="h-4 w-4" /></button>
            <button onClick={() => { setAnchor(new Date()); setPage(1) }} className="rounded border border-border px-3 py-1.5 text-sm text-foreground">Hoy</button>
            <button aria-label="Periodo siguiente" onClick={() => { setAnchor((date) => shiftDate(date, view, 1)); setPage(1) }} className="rounded border border-border p-2 text-foreground"><ChevronRight className="h-4 w-4" /></button>
            <h2 className="ml-2 font-display text-xl capitalize text-foreground">{periodLabel(anchor, view)}</h2>
          </div>
          <div className="flex items-center gap-1" aria-label="Vista de calendario">
            {([ ["month", "Mes"], ["week", "Semana"], ["day", "Día"] ] as const).map(([value, label]) => <button key={value} aria-pressed={view === value} onClick={() => { setView(value); setPage(1) }} className={`rounded px-3 py-1.5 text-sm ${view === value ? "bg-primary text-primary-foreground" : "border border-border text-foreground"}`}>{label}</button>)}
          </div>
        </div>
        {view !== "day" && <div className="grid grid-cols-7 border-b border-border" aria-hidden="true">{["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom"].map((day) => <span key={day} className="border-r border-border px-2 py-2 text-xs font-semibold text-muted-foreground">{day}</span>)}</div>}
        <div className={`grid ${view === "day" ? "grid-cols-1" : "grid-cols-7"} border-b border-border`} role="grid" aria-label={`Calendario ${view}`}>
          {calendarDays.map((date) => {
            const dayItems = calendarItems.filter((item) => sameDay(item.date, date))
            const outsideMonth = view === "month" && date.getMonth() !== anchor.getMonth()
            return <button type="button" role="gridcell" key={date.toISOString()} onClick={() => { setAnchor(date); setView("day"); setPage(1) }} className={`min-h-24 border-b border-r border-border p-2 text-left align-top hover:bg-muted/50 ${outsideMonth ? "bg-muted/20 text-muted-foreground" : "text-foreground"}`}>
              <span className={`text-xs ${sameDay(date, new Date()) ? "rounded-full bg-primary px-2 py-1 text-primary-foreground" : ""}`}>{view === "day" ? date.toLocaleDateString("es-CO", { weekday: "long", day: "numeric", month: "long" }) : date.getDate()}</span>
              <span className="mt-1 block space-y-1">{dayItems.slice(0, view === "month" ? 2 : 8).map((item) => <span key={`${item.kind}-${item.id}`} className={`block truncate rounded px-1.5 py-1 text-[11px] ${item.kind === "Visita" ? "bg-emerald-100 text-emerald-900" : "bg-primary/10 text-primary"}`} title={`${item.kind}: ${item.title} · ${item.status}`}>{item.date.toLocaleTimeString("es-CO", { hour: "2-digit", minute: "2-digit" })} {item.title}</span>)}{dayItems.length > (view === "month" ? 2 : 8) && <span className="text-xs text-muted-foreground">+{dayItems.length - (view === "month" ? 2 : 8)} más</span>}</span>
            </button>
          })}
        </div>
        <div className="grid gap-3 border-b border-border p-4 md:grid-cols-[minmax(0,1fr)_170px_150px_auto]">
          <input aria-label="Buscar tareas" value={query} onChange={(event) => { setQuery(event.target.value); setPage(1) }} placeholder="Buscar título o contacto" className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" />
          <select aria-label="Filtrar por estado" value={status} onChange={(event) => { setStatus(event.target.value); setPage(1) }} className="rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground">
            <option value="">Todos los estados</option><option value="Pending">Pendientes</option><option value="Overdue">Vencidas</option><option value="Completed">Terminadas</option><option value="Cancelled">Canceladas</option>
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
                    {task.status === "Pending" && new Date(task.dueAt) < new Date() && <span className="font-semibold text-destructive">Vencida</span>}
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
          <h2 className="font-display text-xl text-foreground">Visitas del periodo</h2>
          <span className="text-sm text-muted-foreground">{periodVisits.length} visitas</span>
        </div>
        {visitsQuery.isPending && <p className="p-6 text-sm text-muted-foreground">Cargando visitas…</p>}
        {visitsQuery.isError && <p role="alert" className="p-6 text-sm text-destructive">No se pudieron cargar las visitas.</p>}
        {!visitsQuery.isPending && periodVisits.length === 0 && <p className="p-8 text-sm text-muted-foreground">No hay visitas registradas en este periodo.</p>}
        <div className="divide-y divide-border">
          {periodVisits.map((visit) => (
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

function startOfWeek(date: Date) {
  const start = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  start.setDate(start.getDate() - ((start.getDay() + 6) % 7))
  return start
}

function addDays(date: Date, days: number) {
  const result = new Date(date)
  result.setDate(result.getDate() + days)
  return result
}

function getCalendarDays(anchor: Date, view: "month" | "week" | "day") {
  if (view === "day") return [new Date(anchor.getFullYear(), anchor.getMonth(), anchor.getDate())]
  const start = view === "week" ? startOfWeek(anchor) : startOfWeek(new Date(anchor.getFullYear(), anchor.getMonth(), 1))
  const count = view === "week" ? 7 : 42
  return Array.from({ length: count }, (_, index) => addDays(start, index))
}

function sameDay(left: Date, right: Date) {
  return left.getFullYear() === right.getFullYear() && left.getMonth() === right.getMonth() && left.getDate() === right.getDate()
}

function shiftDate(date: Date, view: "month" | "week" | "day", direction: number) {
  if (view === "month") return new Date(date.getFullYear(), date.getMonth() + direction, 1)
  return addDays(date, (view === "week" ? 7 : 1) * direction)
}

function periodLabel(date: Date, view: "month" | "week" | "day") {
  if (view === "month") return date.toLocaleDateString("es-CO", { month: "long", year: "numeric" })
  if (view === "day") return date.toLocaleDateString("es-CO", { weekday: "long", day: "numeric", month: "long", year: "numeric" })
  const start = startOfWeek(date)
  const end = addDays(start, 6)
  return `${start.toLocaleDateString("es-CO", { day: "numeric", month: "short" })} – ${end.toLocaleDateString("es-CO", { day: "numeric", month: "short", year: "numeric" })}`
}
