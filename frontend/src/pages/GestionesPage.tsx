import { useMemo, useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import { TIPOS_INTERACCION } from "@/lib/constants"
import { useAuthStore } from "@/stores/authStore"

const pageSize = 20

export function GestionesPage() {
  const user = useAuthStore((state) => state.user)
  const admin = user?.rol === "Administrador"
  const [type, setType] = useState("")
  const [client, setClient] = useState("")
  const [advisorId, setAdvisorId] = useState("")
  const [page, setPage] = useState(1)
  const [range, setRange] = useState(() => {
    const today = new Date()
    const first = new Date(today.getFullYear(), today.getMonth(), 1)
    return { from: toDateInput(first), to: toDateInput(today) }
  })
  const advisorsQuery = useQuery({ queryKey: ["advisors"], queryFn: () => api.getAdvisors(), enabled: admin })
  const from = useMemo(() => range.from ? new Date(`${range.from}T00:00:00`).toISOString() : "", [range.from])
  const to = useMemo(() => range.to ? new Date(new Date(`${range.to}T00:00:00`).getTime() + 86_400_000).toISOString() : "", [range.to])
  const interactionsQuery = useQuery({
    queryKey: ["interactions", { type, client, advisorId, from, to, page }],
    queryFn: () => api.getInteractions({ type, query: client, advisorId, from, to, page, pageSize }),
  })
  const total = interactionsQuery.data?.total ?? 0
  const pageCount = Math.max(1, Math.ceil(total / pageSize))

  return <div className="space-y-6">
    <header>
      <p className="crm-eyebrow">Operación comercial</p>
      <h1 className="mt-2 font-display text-3xl text-foreground">Gestiones realizadas</h1>
      <p className="mt-2 text-sm text-muted-foreground">Registro de llamadas, correos, WhatsApp, visitas y reuniones.</p>
    </header>
    <section className="crm-panel space-y-4 p-5">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
        <label className="text-sm font-medium text-foreground">Tipo de gestión
          <select value={type} onChange={(event) => { setType(event.target.value); setPage(1) }} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2">
            <option value="">Todos</option>{TIPOS_INTERACCION.map((item) => <option key={item} value={item === "Reunión" ? "Reunion" : item}>{item}</option>)}
          </select>
        </label>
        <label className="text-sm font-medium text-foreground">Nombre cliente
          <input value={client} onChange={(event) => { setClient(event.target.value); setPage(1) }} maxLength={200} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2" />
        </label>
        <label className="text-sm font-medium text-foreground">Desde
          <input type="date" value={range.from} onChange={(event) => { setRange((value) => ({ ...value, from: event.target.value })); setPage(1) }} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2" />
        </label>
        <label className="text-sm font-medium text-foreground">Hasta
          <input type="date" value={range.to} onChange={(event) => { setRange((value) => ({ ...value, to: event.target.value })); setPage(1) }} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2" />
        </label>
        {admin && <label className="text-sm font-medium text-foreground">Usuario encargado
          <select value={advisorId} onChange={(event) => { setAdvisorId(event.target.value); setPage(1) }} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2">
            <option value="">Todos</option>{(advisorsQuery.data ?? []).map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}
          </select>
        </label>}
      </div>
      {interactionsQuery.isPending && <p className="text-sm text-muted-foreground">Cargando gestiones…</p>}
      {interactionsQuery.isError && <p role="alert" className="text-sm text-destructive">No se pudieron cargar las gestiones.</p>}
      {!interactionsQuery.isPending && interactionsQuery.data?.items.length === 0 && <p className="py-8 text-center text-sm text-muted-foreground">No hay gestiones para estos filtros.</p>}
      {!!interactionsQuery.data?.items.length && <div className="overflow-x-auto">
        <table className="w-full min-w-[620px] text-left text-sm"><thead><tr className="border-b border-border text-muted-foreground"><th className="p-3">Tipo de gestión</th><th className="p-3">Asunto</th><th className="p-3">Cliente</th><th className="p-3">Fecha</th></tr></thead>
          <tbody>{interactionsQuery.data.items.map((item) => <tr key={item.id} className="border-b border-border last:border-0"><td className="p-3">{item.type}</td><td className="p-3">{item.summary || "—"}</td><td className="p-3">{item.clientName}</td><td className="p-3">{new Date(item.date).toLocaleString("es-CO")}</td></tr>)}</tbody>
        </table>
      </div>}
      <div className="flex items-center justify-between border-t border-border pt-3 text-sm">
        <span className="text-muted-foreground">{total} gestiones · página {page} de {pageCount}</span>
        <div className="flex gap-2"><button disabled={page <= 1} onClick={() => setPage((value) => value - 1)} className="rounded border border-border px-3 py-1.5 disabled:opacity-40">Anterior</button><button disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)} className="rounded border border-border px-3 py-1.5 disabled:opacity-40">Siguiente</button></div>
      </div>
    </section>
  </div>
}

function toDateInput(date: Date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`
}
