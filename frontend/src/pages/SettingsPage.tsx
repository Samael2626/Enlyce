import { useEffect, useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { Check, Settings2 } from "lucide-react"
import { api } from "@/api/client"
import type { LeadDistributionRule } from "@/lib/types"

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
    </main>
  )
}
