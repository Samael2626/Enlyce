import { useState } from "react"
import { Link } from "react-router-dom"
import { useAlertas, useCommercialTaskAlerts, useCompleteCommercialTask, useLeadSlaAlerts, useRescheduleCommercialTask } from "@/hooks/useApi"
import { AlertTriangle, Clock, Mail, CalendarClock, Check, Timer } from "lucide-react"
import { parseISO, format, formatDistanceToNow } from "date-fns"
import { es } from "date-fns/locale"
import { ETIQUETAS_PIPELINE } from "@/lib/constants"

export function AlertasPage() {
  const { data: alertas, isLoading } = useAlertas()
  const slaQuery = useLeadSlaAlerts()
  const now = new Date()
  const [newDates, setNewDates] = useState<Record<string, string>>({})
  const taskWindowEnd = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString()
  const tasksQuery = useCommercialTaskAlerts(taskWindowEnd)
  const completeTask = useCompleteCommercialTask()
  const rescheduleTask = useRescheduleCommercialTask()

  const leads = alertas?.leads || []
  const tasks = tasksQuery.data ?? []
  const slaAlerts = slaQuery.data?.alerts ?? []

  if (isLoading || tasksQuery.isLoading || slaQuery.isLoading) {
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
          Oportunidades que requieren seguimiento
        </p>
      </div>

      {leads.length === 0 && tasks.length === 0 && slaAlerts.length === 0 ? (
        <div className="crm-panel border-t-4 border-t-accent py-16 text-center">
          <AlertTriangle className="mx-auto mb-4 h-10 w-10 text-accent" />
          <h3 className="font-display text-2xl">Sin alertas</h3>
          <p className="text-sm text-muted-foreground mt-1">
            Todas tus oportunidades están al día
          </p>
        </div>
      ) : (
        <>
        {slaAlerts.length > 0 && <section className="space-y-3" aria-labelledby="sla-alerts-title">
          <div>
            <h2 id="sla-alerts-title" className="font-display text-2xl">Objetivos SLA</h2>
            <p className="mt-1 text-sm text-muted-foreground">Seguimiento interno según las reglas configuradas. No crea tareas ni envía avisos.</p>
          </div>
          {(["FirstResponse", "Inactivity"] as const).map((kind) => {
            const sectionAlerts = slaAlerts.filter((alert) => alert.kind === kind)
            if (sectionAlerts.length === 0) return null
            return <div key={kind} className="space-y-3">
              <h3 className="text-sm font-semibold text-muted-foreground">{kind === "FirstResponse" ? "Primera respuesta pendiente" : "Inactividad"}</h3>
              {sectionAlerts.map((alert) => {
                const dueAt = parseISO(alert.dueAtUtc)
                const overdue = dueAt < now
                return <article key={`${alert.leadId}:${alert.kind}`} className="crm-panel flex flex-wrap items-start gap-4 p-5">
                  <Timer className={`mt-1 h-5 w-5 shrink-0 ${overdue ? "text-destructive" : "text-accent"}`} aria-hidden="true" />
                  <div className="min-w-0 flex-1">
                    <h4 className="font-medium">{alert.nombre}</h4>
                    <p className={`mt-1 text-sm ${overdue ? "font-semibold text-destructive" : "text-muted-foreground"}`}>
                      {overdue ? `Venció ${format(dueAt, "d MMM yyyy, HH:mm", { locale: es })} · atraso ${alert.overdueHours} h` : `Vence ${formatDistanceToNow(dueAt, { addSuffix: true, locale: es })}`}
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {alert.email} · {alert.operationType} · {alert.sourceKey} · {ETIQUETAS_PIPELINE[alert.stage] || alert.stage}
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground">Cuenta desde {formatDistanceToNow(parseISO(alert.startedAtUtc), { addSuffix: true, locale: es })}</p>
                  </div>
                  <Link className="crm-button min-h-9 px-3 py-1.5 text-xs" to={`/leads?q=${encodeURIComponent(alert.email)}&leadId=${encodeURIComponent(alert.leadId)}`}>
                    Abrir oportunidad
                  </Link>
                </article>
              })}
            </div>
          })}
        </section>}
        {tasks.length > 0 && <section className="space-y-3" aria-labelledby="task-alerts-title">
          <h2 id="task-alerts-title" className="font-display text-2xl">Tareas próximas y vencidas</h2>
          {tasks.map((task) => {
            const overdue = new Date(task.dueAt) < now
            return <article key={task.id} className="crm-panel flex flex-wrap items-start gap-4 p-5">
              <CalendarClock className={`mt-1 h-5 w-5 shrink-0 ${overdue ? "text-destructive" : "text-accent"}`} />
              <div className="min-w-0 flex-1">
                <h3 className="font-medium">{task.title}</h3>
                <p className="mt-1 text-sm text-muted-foreground">{overdue ? "Vencida" : "Vence"} {formatDistanceToNow(parseISO(task.dueAt), { addSuffix: true, locale: es })}</p>
                {task.reminderAt && <p className="mt-1 text-xs text-muted-foreground">Recordatorio {formatDistanceToNow(parseISO(task.reminderAt), { addSuffix: true, locale: es })}</p>}
              </div>
              <div className="flex flex-wrap items-center justify-end gap-2">
                <input aria-label={`Nueva fecha para ${task.title}`} type="datetime-local" className="rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground" value={newDates[task.id] ?? toLocalDateTimeInput(new Date(task.dueAt))} onChange={(event) => setNewDates((current) => ({ ...current, [task.id]: event.target.value }))} />
                <button onClick={() => { const date = new Date(newDates[task.id] ?? toLocalDateTimeInput(new Date(task.dueAt))); if (!Number.isNaN(date.getTime())) rescheduleTask.mutate({ id: task.id, dueAt: date.toISOString() }) }} disabled={rescheduleTask.isPending} className="rounded-md border border-border px-3 py-2 text-xs font-semibold text-foreground disabled:opacity-50">Reprogramar</button>
                <button onClick={() => completeTask.mutate(task.id)} disabled={completeTask.isPending} className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-2 text-xs font-semibold text-primary-foreground disabled:opacity-50"><Check className="h-3.5 w-3.5" />Completar</button>
              </div>
            </article>
          })}
        </section>}
        {leads.length === 0 ? null : (
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
                      Etapa: {ETIQUETAS_PIPELINE[lead.etapaPipeline] || lead.etapaPipeline}
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
        </>
      )}
      {tasksQuery.isError && <p role="alert" className="text-sm text-destructive">No se pudieron cargar las tareas pendientes.</p>}
      {slaQuery.isError && <p role="alert" className="text-sm text-destructive">No se pudieron cargar las alertas SLA.</p>}
      {(completeTask.isError || rescheduleTask.isError) && <p role="alert" className="text-sm text-destructive">No se pudo guardar el cambio. Revisa que la tarea siga pendiente.</p>}
    </div>
  )
}

function toLocalDateTimeInput(date: Date) {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 16)
}
