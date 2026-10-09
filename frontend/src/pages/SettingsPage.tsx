import { useEffect, useState } from "react"
import type { FormEvent } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { Check, Pencil, Plus, Settings2 } from "lucide-react"
import { api } from "@/api/client"
import type { LeadDistributionRule, LeadSlaRule, UpsertLeadSlaRule } from "@/lib/types"

const RULE_LABELS: Record<LeadDistributionRule, string> = {
  LeastOpenLeads: "Menor carga abierta",
  RoundRobin: "Turnos rotativos",
}

export function SettingsPage() {
  const queryClient = useQueryClient()
  const settings = useQuery({
    queryKey: ["lead-distribution-rule"],
    queryFn: () => api.getLeadDistributionRule(),
  })
  const [rule, setRule] = useState<LeadDistributionRule>("LeastOpenLeads")
  useEffect(() => {
    if (settings.data) setRule(settings.data.rule)
  }, [settings.data])

  const save = useMutation({
    mutationFn: () => api.setLeadDistributionRule(rule),
    onSuccess: async (data) => {
      setRule(data.rule)
      await queryClient.invalidateQueries({ queryKey: ["lead-distribution-rule"] })
    },
  })

  const slaQuery = useQuery({
    queryKey: ["lead-sla-rules"],
    queryFn: () => api.getLeadSlaRules(),
  })
  const [slaDraft, setSlaDraft] = useState<UpsertLeadSlaRule | null>(null)
  const [editingSla, setEditingSla] = useState(false)
  const [slaError, setSlaError] = useState<string | null>(null)
  const saveSla = useMutation({
    mutationFn: (value: UpsertLeadSlaRule) => api.upsertLeadSlaRule(value),
    onSuccess: async () => {
      setSlaDraft(null)
      setEditingSla(false)
      setSlaError(null)
      await queryClient.invalidateQueries({ queryKey: ["lead-sla-rules"] })
    },
  })

  function editSla(rule?: LeadSlaRule) {
    setSlaError(null)
    setEditingSla(Boolean(rule))
    setSlaDraft(rule ? {
      sourceKey: rule.sourceKey,
      operationType: rule.operationType,
      firstResponseHours: rule.firstResponseHours,
      inactivityDays: rule.inactivityDays,
      enabled: rule.enabled,
    } : {
      sourceKey: "",
      operationType: "*",
      firstResponseHours: 0,
      inactivityDays: null,
      enabled: false,
    })
  }

  function submitSla(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!slaDraft) return
    const sourceKey = slaDraft.sourceKey.trim()
    if (!sourceKey || sourceKey.length > 100) {
      setSlaError("Escribe * para cualquier origen o una raíz UTM de hasta 100 caracteres.")
      return
    }
    if (!Number.isInteger(slaDraft.firstResponseHours) || slaDraft.firstResponseHours < 1 || slaDraft.firstResponseHours > 720) {
      setSlaError("La primera respuesta debe estar entre 1 y 720 horas.")
      return
    }
    if (slaDraft.inactivityDays !== null && (!Number.isInteger(slaDraft.inactivityDays) || slaDraft.inactivityDays < 1 || slaDraft.inactivityDays > 90)) {
      setSlaError("La inactividad debe estar entre 1 y 90 días, o quedar vacía.")
      return
    }
    saveSla.mutate({ ...slaDraft, sourceKey })
  }

  return (
    <main className="mx-auto max-w-4xl space-y-7">
      <header>
        <span className="crm-eyebrow">Administración</span>
        <h1 className="crm-page-title mt-2">Configuración</h1>
        <p className="mt-3 text-sm text-muted-foreground">Define cómo se distribuyen los leads que llegan sin publicación asignada.</p>
      </header>

      <section className="crm-panel space-y-6 p-6" aria-labelledby="distribution-title">
        <div className="flex items-start gap-4">
          <Settings2 className="mt-1 h-5 w-5 text-accent" />
          <div>
            <h2 id="distribution-title" className="font-display text-2xl">Reparto automático</h2>
            <p className="mt-1 text-sm text-muted-foreground">Los leads relacionados con una publicación mantienen el asesor responsable de esa publicación.</p>
          </div>
        </div>

        {settings.isLoading ? <p className="text-sm text-muted-foreground">Cargando regla...</p> : (
          <fieldset className="grid gap-3 sm:grid-cols-2" disabled={save.isPending}>
            {(Object.keys(RULE_LABELS) as LeadDistributionRule[]).map((option) => (
              <label key={option} className={`cursor-pointer rounded-md border p-4 transition-colors ${rule === option ? "border-accent bg-accent/5" : "border-border hover:border-accent/50"}`}>
                <input className="sr-only" type="radio" name="distribution-rule" value={option} checked={rule === option} onChange={() => setRule(option)} />
                <span className="font-semibold">{RULE_LABELS[option]}</span>
                <span className="mt-2 block text-sm text-muted-foreground">
                  {option === "LeastOpenLeads" ? "Asigna al asesor activo con menos oportunidades abiertas." : "Reparte consecutivamente entre asesores activos, en orden estable."}
                </span>
              </label>
            ))}
          </fieldset>
        )}

        <div className="flex flex-wrap items-center gap-3">
          <button className="crm-button inline-flex items-center gap-2" disabled={settings.isLoading || save.isPending || rule === settings.data?.rule} onClick={() => save.mutate()}>
            <Check className="h-4 w-4" />{save.isPending ? "Guardando..." : "Guardar regla"}
          </button>
          {save.isSuccess && <span className="text-sm text-emerald-700">Regla guardada.</span>}
          {(settings.isError || save.isError) && <span role="alert" className="text-sm text-destructive">No se pudo cargar o guardar la regla.</span>}
        </div>
      </section>

      <section className="crm-panel space-y-5 p-6" aria-labelledby="sla-title">
        <div className="flex items-start gap-4">
          <Settings2 className="mt-1 h-5 w-5 text-accent" aria-hidden="true" />
          <div>
            <h2 id="sla-title" className="font-display text-2xl">Seguimiento y SLA</h2>
            <p className="mt-1 text-sm text-muted-foreground">Define objetivos por raíz UTM y tipo de operación. <code>*</code> aplica a todos.</p>
            <p className="mt-2 text-sm text-muted-foreground">Esta política solo guarda objetivos. Todavía no crea tareas ni envía avisos.</p>
          </div>
        </div>

        {slaQuery.isLoading && <p role="status" className="text-sm text-muted-foreground">Cargando reglas...</p>}
        {slaQuery.isError && <p role="alert" className="text-sm text-destructive">No se pudieron cargar las reglas SLA.</p>}

        {slaQuery.data && <div className="overflow-x-auto">
          <table className="w-full min-w-[680px] text-left text-sm">
            <thead><tr className="border-b text-muted-foreground">
              <th scope="col" className="py-2 pr-3">Origen UTM</th>
              <th scope="col" className="py-2 pr-3">Operación</th>
              <th scope="col" className="py-2 pr-3">Primera respuesta (h calendario)</th>
              <th scope="col" className="py-2 pr-3">Inactividad</th>
              <th scope="col" className="py-2 pr-3">Estado</th>
              <th scope="col" className="py-2 pr-3">Actualizada</th>
              <th scope="col" className="py-2">Acción</th>
            </tr></thead>
            <tbody>{slaQuery.data.rules.map((item) => <tr key={`${item.sourceKey}:${item.operationType}`} className="border-b last:border-0">
              <td className="py-3 pr-3 font-medium">{item.sourceKey}</td>
              <td className="py-3 pr-3">{item.operationType === "*" ? "Todas" : item.operationType}</td>
              <td className="py-3 pr-3">{item.firstResponseHours} h</td>
              <td className="py-3 pr-3">{item.inactivityDays === null ? "Sin objetivo" : `${item.inactivityDays} días`}</td>
              <td className="py-3 pr-3">{item.enabled ? "Activa" : "Deshabilitada"}</td>
              <td className="py-3 pr-3">{new Date(item.updatedAtUtc).toLocaleString("es-CO")}</td>
              <td className="py-3"><button type="button" className="inline-flex items-center gap-2 rounded border border-border px-3 py-2 hover:bg-muted focus-visible:outline focus-visible:outline-2 focus-visible:outline-accent" onClick={() => editSla(item)} aria-label={`Editar regla ${item.sourceKey}, ${item.operationType}`}>
                <Pencil className="h-4 w-4" aria-hidden="true" />Editar
              </button></td>
            </tr>)}</tbody>
          </table>
          {slaQuery.data.rules.length === 0 && <p className="py-4 text-sm text-muted-foreground">Aún no hay reglas configuradas.</p>}
        </div>}

        {!slaDraft && <button type="button" className="crm-button inline-flex items-center gap-2" onClick={() => editSla()}>
          <Plus className="h-4 w-4" aria-hidden="true" />Crear regla
        </button>}

        {slaDraft && <form className="grid gap-4 rounded-md border border-border p-4 sm:grid-cols-2" onSubmit={submitSla} aria-label="Regla de seguimiento SLA">
          <label className="grid gap-1 text-sm font-medium">Origen UTM
            <input className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal" value={slaDraft.sourceKey} maxLength={100} required readOnly={editingSla} aria-describedby="sla-source-help" onChange={(event) => setSlaDraft({ ...slaDraft, sourceKey: event.target.value })} />
            <span id="sla-source-help" className="font-normal text-muted-foreground">* para todos; si no, raíz UTM como google o referido.</span>
          </label>
          <label className="grid gap-1 text-sm font-medium">Tipo de operación
            <select className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal" value={slaDraft.operationType} disabled={editingSla} onChange={(event) => setSlaDraft({ ...slaDraft, operationType: event.target.value as UpsertLeadSlaRule["operationType"] })}>
              <option value="*">Todas</option><option value="Venta">Venta</option><option value="Arriendo">Arriendo</option>
            </select>
          </label>
          <label className="grid gap-1 text-sm font-medium">Objetivo de primera respuesta (horas calendario)
            <input className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal" type="number" min={1} max={720} step={1} required value={slaDraft.firstResponseHours || ""} onChange={(event) => setSlaDraft({ ...slaDraft, firstResponseHours: event.target.value === "" ? 0 : Number(event.target.value) })} />
          </label>
          <label className="grid gap-1 text-sm font-medium">Días de inactividad
            <input className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal" type="number" min={1} max={90} step={1} placeholder="Sin objetivo" value={slaDraft.inactivityDays ?? ""} onChange={(event) => setSlaDraft({ ...slaDraft, inactivityDays: event.target.value === "" ? null : Number(event.target.value) })} />
          </label>
          <label className="flex items-center gap-2 text-sm sm:col-span-2">
            <input type="checkbox" checked={slaDraft.enabled} onChange={(event) => setSlaDraft({ ...slaDraft, enabled: event.target.checked })} />Regla activa
          </label>
          {slaError && <p role="alert" className="text-sm text-destructive sm:col-span-2">{slaError}</p>}
          {saveSla.isError && <p role="alert" className="text-sm text-destructive sm:col-span-2">No se pudo guardar la regla. Revisa los valores e inténtalo de nuevo.</p>}
          <div className="flex gap-2 sm:col-span-2">
            <button className="crm-button inline-flex items-center gap-2" type="submit" disabled={saveSla.isPending}><Check className="h-4 w-4" aria-hidden="true" />{saveSla.isPending ? "Guardando..." : "Guardar regla"}</button>
            <button className="rounded border border-border px-4 py-2 hover:bg-muted focus-visible:outline focus-visible:outline-2 focus-visible:outline-accent" type="button" disabled={saveSla.isPending} onClick={() => { setSlaDraft(null); setEditingSla(false); setSlaError(null) }}>Cancelar</button>
          </div>
        </form>}
      </section>
    </main>
  )
}
