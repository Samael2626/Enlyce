import { useEffect, useMemo, useState } from "react"
import { Mail, Phone, Search, UserRound } from "lucide-react"
import { useContact, useContacts, useCreateCommercialTask, useCompleteCommercialTask, useCancelCommercialTask, useCommercialTaskHistory, useAddCommercialTaskComment, useCreateCustomerDemand, useCustomerDemands, useDemandMatches, useLinkDemandProperty, useSetDemandPropertyStatus, useUpdateContact } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"

export function ContactsPage() {
  const contactsQuery = useContacts()
  const [selectedId, setSelectedId] = useState("")
  const [search, setSearch] = useState("")
  const filteredContacts = useMemo(() => {
    const term = search.trim().toLocaleLowerCase()
    return (contactsQuery.data ?? []).filter((contact) =>
      !term || `${contact.name} ${contact.email} ${contact.phone ?? ""}`.toLocaleLowerCase().includes(term)
    )
  }, [contactsQuery.data, search])

  useEffect(() => {
    if (!filteredContacts.some((contact) => contact.id === selectedId))
      setSelectedId(filteredContacts[0]?.id ?? "")
  }, [filteredContacts, selectedId])

  const detailQuery = useContact(selectedId)
  const contact = detailQuery.data?.contact
  const updateContact = useUpdateContact()
  const createTask = useCreateCommercialTask()
  const completeTask = useCompleteCommercialTask()
  const cancelTask = useCancelCommercialTask()
  const [selectedTaskId, setSelectedTaskId] = useState("")
  const taskHistory = useCommercialTaskHistory(selectedTaskId)
  const addTaskComment = useAddCommercialTaskComment()
  const createDemand = useCreateCustomerDemand()
  const demandsQuery = useCustomerDemands(contact?.id ?? "")
  const [selectedDemandId, setSelectedDemandId] = useState("")
  const matchesQuery = useDemandMatches(selectedDemandId)
  const linkProperty = useLinkDemandProperty()
  const setPropertyStatus = useSetDemandPropertyStatus()
  const user = useAuthStore((state) => state.user)
  const [name, setName] = useState("")
  const [phone, setPhone] = useState("")
  const [taskTitle, setTaskTitle] = useState("")
  const [taskDescription, setTaskDescription] = useState("")
  const [taskReminderAt, setTaskReminderAt] = useState("")
  const [taskComment, setTaskComment] = useState("")
  const [taskType, setTaskType] = useState("FollowUp")
  const [taskPriority, setTaskPriority] = useState("Normal")
  const [taskDueAt, setTaskDueAt] = useState(() => toLocalDateTimeInput(new Date(Date.now() + 60 * 60 * 1000)))
  const [demandOperation, setDemandOperation] = useState("Venta")
  const [demandType, setDemandType] = useState("")
  const [demandCity, setDemandCity] = useState("")
  const [demandNeighborhood, setDemandNeighborhood] = useState("")
  const [demandMaxPrice, setDemandMaxPrice] = useState("")
  const [demandBedrooms, setDemandBedrooms] = useState("")

  useEffect(() => {
    setName(contact?.name ?? "")
    setPhone(contact?.phone ?? "")
  }, [contact?.id, contact?.name, contact?.phone])

  useEffect(() => {
    if (!demandsQuery.data?.some((demand) => demand.id === selectedDemandId))
      setSelectedDemandId(demandsQuery.data?.[0]?.id ?? "")
  }, [demandsQuery.data, selectedDemandId])

  const timeline = useMemo(() => {
    const events = (detailQuery.data?.opportunities ?? []).flatMap((opportunity) => [
      ...opportunity.interactions.map((item) => ({
        date: item.date,
        label: item.type,
        detail: item.summary,
        context: `${opportunity.operation} · ${opportunity.stage}`,
      })),
      ...opportunity.visits.map((item) => ({
        date: item.completedAt ?? item.scheduledAt,
        label: `Visita ${item.status.toLocaleLowerCase()}`,
        detail: item.feedback,
        context: `${opportunity.operation} · inmueble ${item.propertyId.slice(0, 8)}`,
      })),
    ]).sort((left, right) => Date.parse(right.date) - Date.parse(left.date))
    return events
  }, [detailQuery.data])

  return (
    <div className="space-y-6">
      <header>
        <p className="crm-eyebrow">Relación comercial</p>
        <h1 className="mt-2 font-display text-3xl text-foreground">Contactos</h1>
        <p className="mt-2 text-sm text-muted-foreground">Personas, oportunidades y actividad en un solo expediente.</p>
      </header>

      <div className="grid gap-5 xl:grid-cols-[340px_minmax(0,1fr)]">
        <section className="crm-panel overflow-hidden">
          <div className="border-b border-border p-4">
            <label className="relative block">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <input
                className="w-full rounded-md border border-border bg-background py-2.5 pl-9 pr-3 text-sm outline-none focus:border-accent"
                placeholder="Buscar contacto"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
              />
            </label>
          </div>
          <div className="max-h-[70vh] overflow-y-auto">
            {contactsQuery.isPending && <p className="p-5 text-sm text-muted-foreground">Cargando contactos…</p>}
            {contactsQuery.isError && <p role="alert" className="p-5 text-sm text-destructive">No se pudieron cargar los contactos.</p>}
            {!contactsQuery.isPending && filteredContacts.length === 0 && (
              <p className="p-5 text-sm text-muted-foreground">Todavía no hay contactos para mostrar.</p>
            )}
            {filteredContacts.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => setSelectedId(item.id)}
                aria-pressed={selectedId === item.id}
                className={`block w-full border-b border-border px-4 py-4 text-left transition-colors hover:bg-muted/60 ${selectedId === item.id ? "bg-muted" : ""}`}
              >
                <span className="block font-semibold text-foreground">{item.name}</span>
                <span className="mt-1 block truncate text-xs text-muted-foreground">{item.email}</span>
                {item.phone && <span className="mt-1 block text-xs text-muted-foreground">{item.phone}</span>}
              </button>
            ))}
          </div>
        </section>

        <section className="space-y-5">
          {!selectedId && <div className="crm-panel p-8 text-sm text-muted-foreground">Selecciona un contacto para abrir su expediente.</div>}
          {detailQuery.isError && <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el expediente.</div>}
          {detailQuery.isPending && selectedId && <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando expediente…</div>}
          {contact && detailQuery.data && (
            <>
              <div className="crm-panel border-t-4 border-t-accent p-6">
                <div className="flex flex-wrap items-start justify-between gap-5">
                  <div className="flex items-start gap-4">
                    <span className="rounded-full bg-accent/10 p-3 text-accent"><UserRound className="h-5 w-5" /></span>
                    <div>
                      <p className="crm-eyebrow">Ficha de contacto</p>
                      <h2 className="mt-1 font-display text-2xl text-foreground">{contact.name}</h2>
                      <p className="mt-1 flex items-center gap-2 text-sm text-muted-foreground"><Mail className="h-4 w-4" />{contact.email}</p>
                      <p className="mt-1 flex items-center gap-2 text-sm text-muted-foreground"><Phone className="h-4 w-4" />{contact.phone || "Sin teléfono"}</p>
                    </div>
                  </div>
                  <form
                    className="flex flex-wrap items-end gap-2"
                    onSubmit={(event) => {
                      event.preventDefault()
                      updateContact.mutate({ id: contact.id, name, phone: phone || undefined })
                    }}
                  >
                    <label className="text-xs text-muted-foreground">Nombre
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={name} onChange={(event) => setName(event.target.value)} maxLength={200} required />
                    </label>
                    <label className="text-xs text-muted-foreground">Teléfono
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={phone} onChange={(event) => setPhone(event.target.value)} maxLength={20} />
                    </label>
                    <button disabled={updateContact.isPending} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">
                      {updateContact.isPending ? "Guardando…" : "Guardar"}
                    </button>
                  </form>
                </div>
                {updateContact.isError && <p role="alert" className="mt-3 text-sm text-destructive">{updateContact.error.message}</p>}
                {updateContact.isSuccess && <p className="mt-3 text-sm text-emerald-700">Cambios guardados.</p>}
              </div>

              <div className="grid gap-5 lg:grid-cols-2">
                <section className="crm-panel p-5 lg:col-span-2">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div>
                      <h3 className="font-display text-xl text-foreground">Seguimiento</h3>
                      <p className="mt-1 text-sm text-muted-foreground">Próximos compromisos e historial de tareas comerciales.</p>
                    </div>
                    <form
                      className="flex flex-wrap items-end gap-2"
                      onSubmit={(event) => {
                        event.preventDefault()
                        const opportunity = detailQuery.data?.opportunities[0]
                        createTask.mutate({
                          contactId: contact.id,
                          leadId: opportunity?.id,
                          advisorId: user?.rol === "Administrador" ? opportunity?.advisorId : undefined,
                          type: taskType,
                          title: taskTitle,
                          description: taskDescription || undefined,
                          dueAt: new Date(taskDueAt).toISOString(),
                          reminderAt: taskReminderAt ? new Date(taskReminderAt).toISOString() : undefined,
                          priority: taskPriority,
                        }, { onSuccess: () => { setTaskTitle(""); setTaskDescription(""); setTaskReminderAt("") } })
                      }}
                    >
                      <label className="text-xs text-muted-foreground">Compromiso
                        <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskTitle} onChange={(event) => setTaskTitle(event.target.value)} maxLength={200} placeholder="Llamar para confirmar interés" required />
                      </label>
                      <label className="text-xs text-muted-foreground">Tipo
                        <select className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskType} onChange={(event) => setTaskType(event.target.value)}>
                          <option value="Call">Llamada</option><option value="Message">Mensaje</option><option value="Meeting">Reunión</option><option value="FollowUp">Seguimiento</option><option value="Other">Otro</option>
                        </select>
                      </label>
                      <label className="text-xs text-muted-foreground">Fecha
                        <input type="datetime-local" className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskDueAt} onChange={(event) => setTaskDueAt(event.target.value)} required />
                      </label>
                      <label className="text-xs text-muted-foreground">Recordatorio opcional
                        <input type="datetime-local" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskReminderAt} onChange={(event) => setTaskReminderAt(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground sm:col-span-2">Notas
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskDescription} onChange={(event) => setTaskDescription(event.target.value)} maxLength={2000} />
                      </label>
                      <label className="text-xs text-muted-foreground">Prioridad
                        <select className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskPriority} onChange={(event) => setTaskPriority(event.target.value)}>
                          <option value="Low">Baja</option><option value="Normal">Normal</option><option value="High">Alta</option><option value="Urgent">Urgente</option>
                        </select>
                      </label>
                      <button disabled={createTask.isPending || (user?.rol === "Administrador" && !detailQuery.data?.opportunities[0]?.advisorId)} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">
                        {createTask.isPending ? "Creando…" : "Crear tarea"}
                      </button>
                    </form>
                  </div>
                  {createTask.isError && <p role="alert" className="mt-3 text-sm text-destructive">{createTask.error.message}</p>}
                  {user?.rol === "Administrador" && !detailQuery.data.opportunities[0]?.advisorId && <p className="mt-3 text-sm text-muted-foreground">Asigna primero una oportunidad a un asesor para crearle una tarea.</p>}
                  <div className="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                    {detailQuery.data.tasks.map((task) => (
                      <article key={task.id} className="rounded-md border border-border p-4">
                        <div className="flex items-start justify-between gap-2">
                          <p className="font-semibold text-foreground">{task.title}</p>
                          <span className="rounded-full bg-muted px-2 py-1 text-[.65rem] font-semibold text-muted-foreground">{task.priority}</span>
                        </div>
                        <p className="mt-2 text-xs text-muted-foreground">{task.type} · {new Date(task.dueAt).toLocaleString("es-CO")}</p>
                        <p className="mt-1 text-xs text-muted-foreground">{task.status === "Pending" ? "Pendiente" : task.status === "Completed" ? "Completada" : "Cancelada"}</p>
                        {task.status === "Pending" && <><button onClick={() => completeTask.mutate(task.id)} disabled={completeTask.isPending} className="mt-3 rounded border border-border px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-muted">Marcar completada</button><button onClick={() => cancelTask.mutate(task.id)} disabled={cancelTask.isPending} className="ml-2 mt-3 rounded border border-destructive/40 px-3 py-1.5 text-xs font-semibold text-destructive">Cancelar</button></>}
                        <button onClick={() => setSelectedTaskId(selectedTaskId === task.id ? "" : task.id)} className="ml-2 mt-3 rounded border border-border px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-muted">Historial</button>
                        {selectedTaskId === task.id && (
                          <div className="mt-3 border-t border-border pt-3">
                            {taskHistory.data?.map((event) => <p key={event.id} className="mt-1 text-xs text-muted-foreground">{new Date(event.occurredAt).toLocaleString("es-CO")} · {event.action}{event.comment ? `: ${event.comment}` : ""}</p>)}
                            <form className="mt-2 flex gap-2" onSubmit={(event) => { event.preventDefault(); addTaskComment.mutate({ id: task.id, comment: taskComment }, { onSuccess: () => setTaskComment("") }) }}>
                              <input className="min-w-0 flex-1 rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground" value={taskComment} onChange={(event) => setTaskComment(event.target.value)} maxLength={2000} placeholder="Agregar comentario" required />
                              <button disabled={addTaskComment.isPending} className="rounded border border-border px-2 py-1 text-xs font-semibold text-foreground">Añadir</button>
                            </form>
                          </div>
                        )}
                      </article>
                    ))}
                    {detailQuery.data.tasks.length === 0 && <p className="text-sm text-muted-foreground">Sin tareas todavía.</p>}
                  </div>
                </section>
                <section className="crm-panel p-5 lg:col-span-2">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div>
                      <h3 className="font-display text-xl text-foreground">Búsqueda del cliente</h3>
                      <p className="mt-1 text-sm text-muted-foreground">Registra su demanda y encuentra inmuebles disponibles que coincidan.</p>
                    </div>
                    <form
                      className="grid w-full gap-2 sm:grid-cols-2 xl:grid-cols-6"
                      onSubmit={(event) => {
                        event.preventDefault()
                        createDemand.mutate({
                          contactId: contact.id,
                          leadId: detailQuery.data?.opportunities[0]?.id,
                          operation: demandOperation,
                          propertyType: demandType || undefined,
                          city: demandCity,
                          neighborhood: demandNeighborhood || undefined,
                          maximumPrice: demandMaxPrice ? Number(demandMaxPrice) : undefined,
                          bedrooms: demandBedrooms ? Number(demandBedrooms) : undefined,
                        }, { onSuccess: (demand) => { setSelectedDemandId(demand.id); setDemandCity(""); setDemandNeighborhood("") } })
                      }}
                    >
                      <label className="text-xs text-muted-foreground">Operación
                        <select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandOperation} onChange={(event) => setDemandOperation(event.target.value)}><option value="Venta">Venta</option><option value="Arriendo">Arriendo</option></select>
                      </label>
                      <label className="text-xs text-muted-foreground">Tipo de inmueble
                        <select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandType} onChange={(event) => setDemandType(event.target.value)}><option value="">Cualquiera</option><option value="Apartamento">Apartamento</option><option value="Casa">Casa</option><option value="Local">Local</option><option value="Oficina">Oficina</option><option value="Lote">Lote</option><option value="Bodega">Bodega</option><option value="Finca">Finca</option></select>
                      </label>
                      <label className="text-xs text-muted-foreground">Ciudad
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandCity} onChange={(event) => setDemandCity(event.target.value)} maxLength={100} required />
                      </label>
                      <label className="text-xs text-muted-foreground">Barrio
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandNeighborhood} onChange={(event) => setDemandNeighborhood(event.target.value)} maxLength={100} />
                      </label>
                      <label className="text-xs text-muted-foreground">Presupuesto máximo
                        <input type="number" min="0" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandMaxPrice} onChange={(event) => setDemandMaxPrice(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground">Habitaciones mínimas
                        <input type="number" min="0" max="50" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandBedrooms} onChange={(event) => setDemandBedrooms(event.target.value)} />
                      </label>
                      <div className="sm:col-span-2 xl:col-span-6">
                        <button disabled={createDemand.isPending} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">{createDemand.isPending ? "Guardando…" : "Guardar búsqueda"}</button>
                      </div>
                    </form>
                  </div>
                  {createDemand.isError && <p role="alert" className="mt-3 text-sm text-destructive">{createDemand.error.message}</p>}
                  <div className="mt-5 grid gap-5 lg:grid-cols-[minmax(220px,.65fr)_minmax(0,1fr)]">
                    <div className="space-y-2">
                      <h4 className="text-sm font-semibold text-foreground">Demandas guardadas</h4>
                      {demandsQuery.data?.map((demand) => (
                        <button key={demand.id} type="button" onClick={() => setSelectedDemandId(demand.id)} aria-pressed={selectedDemandId === demand.id} className={`block w-full rounded-md border border-border p-3 text-left hover:bg-muted ${selectedDemandId === demand.id ? "bg-muted" : ""}`}>
                          <span className="block text-sm font-semibold text-foreground">{demand.operation} · {demand.propertyType ?? "Cualquier inmueble"}</span>
                          <span className="mt-1 block text-xs text-muted-foreground">{demand.city}{demand.neighborhood ? ` · ${demand.neighborhood}` : ""}{demand.maximumPrice ? ` · Hasta $${demand.maximumPrice.toLocaleString("es-CO")}` : ""}</span>
                        </button>
                      ))}
                      {demandsQuery.data?.length === 0 && <p className="text-sm text-muted-foreground">Todavía no hay una búsqueda guardada.</p>}
                    </div>
                    <div>
                      <h4 className="text-sm font-semibold text-foreground">Inmuebles compatibles</h4>
                      {matchesQuery.isPending && selectedDemandId && <p className="mt-3 text-sm text-muted-foreground">Buscando en el inventario…</p>}
                      {matchesQuery.data?.map((match) => (
                        <article key={match.propertyId} className="mt-2 flex flex-wrap items-center justify-between gap-3 rounded-md border border-border p-3">
                          <div>
                            <p className="font-semibold text-foreground">{match.name}</p>
                            <p className="mt-1 text-xs text-muted-foreground">{match.propertyType} · {match.city}{match.neighborhood ? `, ${match.neighborhood}` : ""} · {match.bedrooms} hab. · {match.currency} {match.price.toLocaleString("es-CO")}</p>
                          </div>
                          {match.relationshipStatus ? (
                            <select aria-label={`Estado de ${match.name}`} value={match.relationshipStatus} onChange={(event) => setPropertyStatus.mutate({ demandId: selectedDemandId, propertyId: match.propertyId, status: event.target.value })} className="rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground">
                              <option value="Suggested">Sugerido</option><option value="Shared">Compartido</option><option value="Visited">Visitado</option><option value="Discarded">Descartado</option>
                            </select>
                          ) : <button onClick={() => linkProperty.mutate({ demandId: selectedDemandId, propertyId: match.propertyId })} disabled={linkProperty.isPending} className="rounded-md border border-border px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted">Relacionar</button>}
                        </article>
                      ))}
                      {selectedDemandId && matchesQuery.data?.length === 0 && <p className="mt-3 text-sm text-muted-foreground">No encontramos inmuebles disponibles que cumplan estos criterios.</p>}
                      {(matchesQuery.isError || linkProperty.isError || setPropertyStatus.isError) && <p role="alert" className="mt-3 text-sm text-destructive">No se pudo consultar o guardar la relación con el inmueble.</p>}
                    </div>
                  </div>
                </section>
                <section className="crm-panel p-5">
                  <h3 className="font-display text-xl text-foreground">Oportunidades</h3>
                  <div className="mt-4 space-y-3">
                    {detailQuery.data?.opportunities.length === 0 && <p className="text-sm text-muted-foreground">Sin oportunidades visibles.</p>}
                    {detailQuery.data?.opportunities.map((opportunity) => (
                      <article key={opportunity.id} className="rounded-md border border-border p-3">
                        <p className="font-semibold text-foreground">{opportunity.operation} · {opportunity.stage}</p>
                        <p className="mt-1 text-xs text-muted-foreground">Origen: {opportunity.source}</p>
                        <p className="mt-1 text-xs text-muted-foreground">Creada {new Date(opportunity.createdAt).toLocaleDateString("es-CO")}</p>
                      </article>
                    ))}
                  </div>
                </section>
                <section className="crm-panel p-5">
                  <h3 className="font-display text-xl text-foreground">Actividad reciente</h3>
                  <div className="mt-4 space-y-3">
                    {timeline.length === 0 && <p className="text-sm text-muted-foreground">Todavía no hay interacciones ni visitas registradas.</p>}
                    {timeline.map((event, index) => (
                      <article key={`${event.date}-${event.label}-${index}`} className="border-l-2 border-accent/50 pl-3">
                        <p className="text-sm font-semibold text-foreground">{event.label}</p>
                        <p className="text-xs text-muted-foreground">{event.context} · {new Date(event.date).toLocaleString("es-CO")}</p>
                        {event.detail && <p className="mt-1 text-sm text-muted-foreground">{event.detail}</p>}
                      </article>
                    ))}
                  </div>
                </section>
              </div>
            </>
          )}
        </section>
      </div>
    </div>
  )
}

function toLocalDateTimeInput(date: Date) {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 16)
}
