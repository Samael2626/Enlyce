import { useEffect, useState } from "react"
import type { FormEvent } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import {
  Check,
  ExternalLink,
  Globe,
  Palette,
  Pencil,
  Plus,
  RefreshCw,
  Save,
  Settings2,
} from "lucide-react"
import { api } from "@/api/client"
import { useAdminWebsiteConfig, useUpdateWebsiteConfig } from "@/hooks/useApi"
import type {
  LeadDistributionRule,
  LeadSlaRule,
  UpdateWebsiteConfigInput,
  UpsertLeadSlaRule,
} from "@/lib/types"
import { useAuthStore } from "@/stores/authStore"

const RULE_LABELS: Record<LeadDistributionRule, string> = {
  LeastOpenLeads: "Menor carga abierta",
  RoundRobin: "Turnos rotativos",
}

export function SettingsPage() {
  const queryClient = useQueryClient()
  const user = useAuthStore((state) => state.user)
  const canManageFunnels = user?.rol === "Administrador"
  const [activeTab, setActiveTab] = useState<"crm" | "cms">("crm")

  // --- CRM: Catálogos y Embudos ---
  const customerCatalogsQuery = useQuery({
    queryKey: ["customer-catalogs"],
    queryFn: () => api.getCustomerCatalogs(),
  })
  const customerFunnelStagesQuery = useQuery({
    queryKey: ["customer-funnel-stages"],
    queryFn: () => api.getCustomerFunnelStages(),
  })
  const customerFunnels = [
    ...new Set([
      ...(customerCatalogsQuery.data?.funnels ?? []),
      ...(customerFunnelStagesQuery.data?.stages.map((stage) => stage.funnelName) ?? []),
    ]),
  ]
  const [catalog, setCatalog] = useState("Tipo")
  const [catalogName, setCatalogName] = useState("")
  const [catalogError, setCatalogError] = useState("")
  const addCatalogItem = useMutation({
    mutationFn: () => api.addCustomerCatalogItem(catalog, catalogName),
    onSuccess: async () => {
      setCatalogName("")
      setCatalogError("")
      await queryClient.invalidateQueries({ queryKey: ["customer-catalogs"] })
    },
    onError: (error) =>
      setCatalogError(error instanceof Error ? error.message : "No se pudo guardar la opcion."),
  })
  const [stageFunnel, setStageFunnel] = useState("Clientes")
  const [stageName, setStageName] = useState("")
  const [stageError, setStageError] = useState("")
  const addCustomerFunnelStage = useMutation({
    mutationFn: () => api.addCustomerFunnelStage(stageFunnel, stageName),
    onSuccess: async () => {
      setStageName("")
      setStageError("")
      await queryClient.invalidateQueries({ queryKey: ["customer-funnel-stages"] })
    },
    onError: (error) =>
      setStageError(error instanceof Error ? error.message : "No se pudo guardar la etapa."),
  })
  const setDefaultCustomerFunnelStage = useMutation({
    mutationFn: (id: string) => api.setDefaultCustomerFunnelStage(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["customer-funnel-stages"] })
    },
  })

  // --- CRM: Reparto automático ---
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

  // --- CRM: Reglas SLA ---
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
    setSlaDraft(
      rule
        ? {
            sourceKey: rule.sourceKey,
            operationType: rule.operationType,
            firstResponseMinutes: rule.firstResponseMinutes,
            inactivityDays: rule.inactivityDays,
            enabled: rule.enabled,
          }
        : {
            sourceKey: "*",
            operationType: "*",
            firstResponseMinutes: 45,
            inactivityDays: null,
            enabled: true,
          }
    )
  }

  function submitSla(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!slaDraft) return
    const sourceKey = slaDraft.sourceKey.trim()
    if (!sourceKey || sourceKey.length > 100) {
      setSlaError("Escribe * para cualquier origen o una raíz UTM de hasta 100 caracteres.")
      return
    }
    if (
      !Number.isInteger(slaDraft.firstResponseMinutes) ||
      slaDraft.firstResponseMinutes < 1 ||
      slaDraft.firstResponseMinutes > 43200
    ) {
      setSlaError("La primera respuesta debe estar entre 1 y 43200 minutos.")
      return
    }
    if (
      slaDraft.inactivityDays !== null &&
      (!Number.isInteger(slaDraft.inactivityDays) ||
        slaDraft.inactivityDays < 1 ||
        slaDraft.inactivityDays > 90)
    ) {
      setSlaError("La inactividad debe estar entre 1 y 90 días, o quedar vacía.")
      return
    }
    saveSla.mutate({ ...slaDraft, sourceKey })
  }

  // --- CMS: Configuración Sitio Web Público ---
  const websiteConfigQuery = useAdminWebsiteConfig()
  const updateWebsiteConfigMutation = useUpdateWebsiteConfig()
  const [websiteDraft, setWebsiteDraft] = useState<UpdateWebsiteConfigInput>({
    agencyName: "",
    slogan: "",
    heroTitle: "",
    heroSubtitle: "",
    heroBannerUrl: "",
    aboutTitle: "",
    aboutText: "",
    contactPhone: "",
    contactWhatsapp: "",
    contactEmail: "",
    address: "",
    instagramUrl: "",
    facebookUrl: "",
    seoTitle: "",
    seoDescription: "",
    primaryColor: "",
    accentColor: "",
  })
  const [websiteSuccessMsg, setWebsiteSuccessMsg] = useState(false)

  useEffect(() => {
    if (websiteConfigQuery.data) {
      setWebsiteDraft({
        agencyName: websiteConfigQuery.data.agencyName ?? "",
        slogan: websiteConfigQuery.data.slogan ?? "",
        heroTitle: websiteConfigQuery.data.heroTitle ?? "",
        heroSubtitle: websiteConfigQuery.data.heroSubtitle ?? "",
        heroBannerUrl: websiteConfigQuery.data.heroBannerUrl ?? "",
        aboutTitle: websiteConfigQuery.data.aboutTitle ?? "",
        aboutText: websiteConfigQuery.data.aboutText ?? "",
        contactPhone: websiteConfigQuery.data.contactPhone ?? "",
        contactWhatsapp: websiteConfigQuery.data.contactWhatsapp ?? "",
        contactEmail: websiteConfigQuery.data.contactEmail ?? "",
        address: websiteConfigQuery.data.address ?? "",
        instagramUrl: websiteConfigQuery.data.instagramUrl ?? "",
        facebookUrl: websiteConfigQuery.data.facebookUrl ?? "",
        seoTitle: websiteConfigQuery.data.seoTitle ?? "",
        seoDescription: websiteConfigQuery.data.seoDescription ?? "",
        primaryColor: websiteConfigQuery.data.primaryColor ?? "",
        accentColor: websiteConfigQuery.data.accentColor ?? "",
      })
    }
  }, [websiteConfigQuery.data])

  function submitWebsiteConfig(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setWebsiteSuccessMsg(false)
    updateWebsiteConfigMutation.mutate(websiteDraft, {
      onSuccess: () => {
        setWebsiteSuccessMsg(true)
        setTimeout(() => setWebsiteSuccessMsg(false), 5000)
      },
    })
  }

  function resetWebsiteConfig() {
    if (websiteConfigQuery.data) {
      setWebsiteDraft({
        agencyName: websiteConfigQuery.data.agencyName ?? "",
        slogan: websiteConfigQuery.data.slogan ?? "",
        heroTitle: websiteConfigQuery.data.heroTitle ?? "",
        heroSubtitle: websiteConfigQuery.data.heroSubtitle ?? "",
        heroBannerUrl: websiteConfigQuery.data.heroBannerUrl ?? "",
        aboutTitle: websiteConfigQuery.data.aboutTitle ?? "",
        aboutText: websiteConfigQuery.data.aboutText ?? "",
        contactPhone: websiteConfigQuery.data.contactPhone ?? "",
        contactWhatsapp: websiteConfigQuery.data.contactWhatsapp ?? "",
        contactEmail: websiteConfigQuery.data.contactEmail ?? "",
        address: websiteConfigQuery.data.address ?? "",
        instagramUrl: websiteConfigQuery.data.instagramUrl ?? "",
        facebookUrl: websiteConfigQuery.data.facebookUrl ?? "",
        seoTitle: websiteConfigQuery.data.seoTitle ?? "",
        seoDescription: websiteConfigQuery.data.seoDescription ?? "",
        primaryColor: websiteConfigQuery.data.primaryColor ?? "",
        accentColor: websiteConfigQuery.data.accentColor ?? "",
      })
    }
  }

  return (
    <main className="mx-auto max-w-4xl space-y-7">
      <header>
        <span className="crm-eyebrow">Administración</span>
        <h1 className="crm-page-title mt-2">Configuración</h1>
        <p className="mt-3 text-sm text-muted-foreground">
          Control de reglas del CRM, catálogos, y edición de contenidos del sitio web público.
        </p>
      </header>

      {/* Tabs de Configuración */}
      <nav className="flex border-b border-border" aria-label="Secciones de configuración">
        <button
          type="button"
          onClick={() => setActiveTab("crm")}
          className={`flex items-center gap-2 border-b-2 px-5 py-3 text-sm font-medium transition-colors ${
            activeTab === "crm"
              ? "border-accent text-accent font-semibold"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
        >
          <Settings2 className="h-4 w-4" />
          CRM & Operaciones
        </button>
        <button
          type="button"
          onClick={() => setActiveTab("cms")}
          className={`flex items-center gap-2 border-b-2 px-5 py-3 text-sm font-medium transition-colors ${
            activeTab === "cms"
              ? "border-accent text-accent font-semibold"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
        >
          <Globe className="h-4 w-4" />
          Sitio Web Público & Marketing (CMS)
        </button>
      </nav>

      {activeTab === "crm" ? (
        <div className="space-y-7">
          <section className="crm-panel space-y-6 p-6" aria-labelledby="distribution-title">
            <div className="flex items-start gap-4">
              <Settings2 className="mt-1 h-5 w-5 text-accent" />
              <div>
                <h2 id="distribution-title" className="font-display text-2xl">
                  Reparto automático
                </h2>
                <p className="mt-1 text-sm text-muted-foreground">
                  Los leads relacionados con una publicación mantienen el asesor responsable de esa
                  publicación.
                </p>
              </div>
            </div>

            {settings.isLoading ? (
              <p className="text-sm text-muted-foreground">Cargando regla...</p>
            ) : (
              <fieldset className="grid gap-3 sm:grid-cols-2" disabled={save.isPending}>
                {(Object.keys(RULE_LABELS) as LeadDistributionRule[]).map((option) => (
                  <label
                    key={option}
                    className={`cursor-pointer rounded-md border p-4 transition-colors ${
                      rule === option
                        ? "border-accent bg-accent/5"
                        : "border-border hover:border-accent/50"
                    }`}
                  >
                    <input
                      className="sr-only"
                      type="radio"
                      name="distribution-rule"
                      value={option}
                      checked={rule === option}
                      onChange={() => setRule(option)}
                    />
                    <span className="font-semibold">{RULE_LABELS[option]}</span>
                    <span className="mt-2 block text-sm text-muted-foreground">
                      {option === "LeastOpenLeads"
                        ? "Asigna al asesor activo con menos oportunidades abiertas."
                        : "Reparte consecutivamente entre asesores activos, en orden estable."}
                    </span>
                  </label>
                ))}
              </fieldset>
            )}

            <div className="flex flex-wrap items-center gap-3">
              <button
                className="crm-button inline-flex items-center gap-2"
                disabled={settings.isLoading || save.isPending || rule === settings.data?.rule}
                onClick={() => save.mutate()}
              >
                <Check className="h-4 w-4" />
                {save.isPending ? "Guardando..." : "Guardar regla"}
              </button>
              {save.isSuccess && <span className="text-sm text-emerald-700">Regla guardada.</span>}
              {(settings.isError || save.isError) && (
                <span role="alert" className="text-sm text-destructive">
                  No se pudo cargar o guardar la regla.
                </span>
              )}
            </div>
          </section>

          <section className="crm-panel space-y-5 p-6" aria-labelledby="sla-title">
            <div className="flex items-start gap-4">
              <Settings2 className="mt-1 h-5 w-5 text-accent" aria-hidden="true" />
              <div>
                <h2 id="sla-title" className="font-display text-2xl">
                  Seguimiento y SLA
                </h2>
                <p className="mt-1 text-sm text-muted-foreground">
                  Define objetivos por raíz UTM y tipo de operación. <code>*</code> aplica a todos.
                </p>
                <p className="mt-2 text-sm text-muted-foreground">
                  Esta política solo guarda objetivos. Todavía no crea tareas ni envía avisos.
                </p>
              </div>
            </div>

            {slaQuery.isLoading && (
              <p role="status" className="text-sm text-muted-foreground">
                Cargando reglas...
              </p>
            )}
            {slaQuery.isError && (
              <p role="alert" className="text-sm text-destructive">
                No se pudieron cargar las reglas SLA.
              </p>
            )}

            {slaQuery.data && (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[680px] text-left text-sm">
                  <thead>
                    <tr className="border-b text-muted-foreground">
                      <th scope="col" className="py-2 pr-3">
                        Origen UTM
                      </th>
                      <th scope="col" className="py-2 pr-3">
                        Operación
                      </th>
                      <th scope="col" className="py-2 pr-3">
                        Primera respuesta (h calendario)
                      </th>
                      <th scope="col" className="py-2 pr-3">
                        Inactividad
                      </th>
                      <th scope="col" className="py-2 pr-3">
                        Estado
                      </th>
                      <th scope="col" className="py-2 pr-3">
                        Actualizada
                      </th>
                      <th scope="col" className="py-2">
                        Acción
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {slaQuery.data.rules.map((item) => (
                      <tr
                        key={`${item.sourceKey}:${item.operationType}`}
                        className="border-b last:border-0"
                      >
                        <td className="py-3 pr-3 font-medium">{item.sourceKey}</td>
                        <td className="py-3 pr-3">
                          {item.operationType === "*" ? "Todas" : item.operationType}
                        </td>
                        <td className="py-3 pr-3">{item.firstResponseMinutes} min</td>
                        <td className="py-3 pr-3">
                          {item.inactivityDays === null
                            ? "Sin objetivo"
                            : `${item.inactivityDays} días`}
                        </td>
                        <td className="py-3 pr-3">{item.enabled ? "Activa" : "Deshabilitada"}</td>
                        <td className="py-3 pr-3">
                          {new Date(item.updatedAtUtc).toLocaleString("es-CO")}
                        </td>
                        <td className="py-3">
                          <button
                            type="button"
                            className="inline-flex items-center gap-2 rounded border border-border px-3 py-2 hover:bg-muted focus-visible:outline focus-visible:outline-2 focus-visible:outline-accent"
                            onClick={() => editSla(item)}
                            aria-label={`Editar regla ${item.sourceKey}, ${item.operationType}`}
                          >
                            <Pencil className="h-4 w-4" aria-hidden="true" />
                            Editar
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {slaQuery.data.rules.length === 0 && (
                  <p className="py-4 text-sm text-muted-foreground">
                    Aún no hay reglas configuradas.
                  </p>
                )}
              </div>
            )}

            {!slaDraft && (
              <button
                type="button"
                className="crm-button inline-flex items-center gap-2"
                onClick={() => editSla()}
              >
                <Plus className="h-4 w-4" aria-hidden="true" />
                Crear regla
              </button>
            )}

            {slaDraft && (
              <form
                className="grid gap-4 rounded-md border border-border p-4 sm:grid-cols-2"
                onSubmit={submitSla}
                aria-label="Regla de seguimiento SLA"
              >
                <label className="grid gap-1 text-sm font-medium">
                  Origen UTM
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={slaDraft.sourceKey}
                    maxLength={100}
                    required
                    readOnly={editingSla}
                    aria-describedby="sla-source-help"
                    onChange={(event) =>
                      setSlaDraft({ ...slaDraft, sourceKey: event.target.value })
                    }
                  />
                  <span id="sla-source-help" className="font-normal text-muted-foreground">
                    * para todos; si no, raíz UTM como google o referido.
                  </span>
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Tipo de operación
                  <select
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={slaDraft.operationType}
                    disabled={editingSla}
                    onChange={(event) =>
                      setSlaDraft({
                        ...slaDraft,
                        operationType: event.target.value as UpsertLeadSlaRule["operationType"],
                      })
                    }
                  >
                    <option value="*">Todas</option>
                    <option value="Venta">Venta</option>
                    <option value="Arriendo">Arriendo</option>
                  </select>
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Objetivo de primera respuesta (minutos calendario)
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    type="number"
                    min={1}
                    max={43200}
                    step={1}
                    required
                    value={slaDraft.firstResponseMinutes || ""}
                    onChange={(event) =>
                      setSlaDraft({
                        ...slaDraft,
                        firstResponseMinutes:
                          event.target.value === "" ? 0 : Number(event.target.value),
                      })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Días de inactividad
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    type="number"
                    min={1}
                    max={90}
                    step={1}
                    placeholder="Sin objetivo"
                    value={slaDraft.inactivityDays ?? ""}
                    onChange={(event) =>
                      setSlaDraft({
                        ...slaDraft,
                        inactivityDays:
                          event.target.value === "" ? null : Number(event.target.value),
                      })
                    }
                  />
                </label>
                <label className="flex items-center gap-2 text-sm sm:col-span-2">
                  <input
                    type="checkbox"
                    checked={slaDraft.enabled}
                    onChange={(event) =>
                      setSlaDraft({ ...slaDraft, enabled: event.target.checked })
                    }
                  />
                  Regla activa
                </label>
                {slaError && (
                  <p role="alert" className="text-sm text-destructive sm:col-span-2">
                    {slaError}
                  </p>
                )}
                {saveSla.isError && (
                  <p role="alert" className="text-sm text-destructive sm:col-span-2">
                    No se pudo guardar la regla. Revisa los valores e inténtalo de nuevo.
                  </p>
                )}
                <div className="flex gap-2 sm:col-span-2">
                  <button
                    className="crm-button inline-flex items-center gap-2"
                    type="submit"
                    disabled={saveSla.isPending}
                  >
                    <Check className="h-4 w-4" aria-hidden="true" />
                    {saveSla.isPending ? "Guardando..." : "Guardar regla"}
                  </button>
                  <button
                    className="rounded border border-border px-4 py-2 hover:bg-muted focus-visible:outline focus-visible:outline-2 focus-visible:outline-accent"
                    type="button"
                    disabled={saveSla.isPending}
                    onClick={() => {
                      setSlaDraft(null)
                      setEditingSla(false)
                      setSlaError(null)
                    }}
                  >
                    Cancelar
                  </button>
                </div>
              </form>
            )}
          </section>

          <section className="crm-panel space-y-5 p-6" aria-labelledby="customer-catalog-title">
            <div>
              <h2 id="customer-catalog-title" className="font-display text-2xl">
                Clasificación de clientes
              </h2>
              <p className="mt-1 text-sm text-muted-foreground">
                Agrega opciones reutilizables para tipos, etiquetas, embudos y medios de captación.
                Los valores guardados en fichas se conservan aunque no aparezcan en este catálogo.
              </p>
            </div>
            {customerCatalogsQuery.isLoading && (
              <p role="status" className="text-sm text-muted-foreground">
                Cargando catálogos…
              </p>
            )}
            {customerCatalogsQuery.isError && (
              <p role="alert" className="text-sm text-destructive">
                No se pudieron cargar los catálogos.
              </p>
            )}
            {customerCatalogsQuery.data && (
              <div className="grid gap-4 sm:grid-cols-2">
                {(
                  [
                    ["CustomerType", "Tipos de cliente", customerCatalogsQuery.data.types],
                    ["CustomerLabel", "Etiquetas", customerCatalogsQuery.data.labels],
                    ["CustomerFunnel", "Embudos", customerCatalogsQuery.data.funnels],
                    [
                      "AcquisitionMedium",
                      "Medios de captación",
                      customerCatalogsQuery.data.acquisitionMedia,
                    ],
                  ] as const
                ).map(([key, label, values]) => (
                  <article key={key} className="rounded-md border border-border p-4">
                    <h3 className="font-semibold">{label}</h3>
                    <ul className="mt-2 flex flex-wrap gap-2 text-sm">
                      {values.map((value) => (
                        <li key={value} className="rounded-full bg-muted px-3 py-1">
                          {value}
                        </li>
                      ))}
                    </ul>
                    {values.length === 0 && (
                      <p className="mt-2 text-sm text-muted-foreground">Sin opciones creadas.</p>
                    )}
                  </article>
                ))}
              </div>
            )}
            <form
              className="grid gap-3 sm:grid-cols-[220px_minmax(0,1fr)_auto]"
              onSubmit={(event) => {
                event.preventDefault()
                setCatalogError("")
                addCatalogItem.mutate()
              }}
            >
              <label className="grid gap-1 text-sm font-medium">
                Catálogo
                <select
                  className="rounded border border-border bg-background px-3 py-2"
                  value={catalog}
                  onChange={(event) => setCatalog(event.target.value)}
                >
                  <option value="CustomerType">Tipo de cliente</option>
                  <option value="CustomerLabel">Etiqueta</option>
                  <option value="CustomerFunnel">Embudo</option>
                  <option value="AcquisitionMedium">Medio de captación</option>
                </select>
              </label>
              <label className="grid gap-1 text-sm font-medium">
                Nueva opción
                <input
                  className="rounded border border-border bg-background px-3 py-2"
                  maxLength={
                    catalog === "CustomerType"
                      ? 30
                      : catalog === "CustomerLabel"
                      ? 50
                      : catalog === "CustomerFunnel"
                      ? 100
                      : 200
                  }
                  value={catalogName}
                  onChange={(event) => setCatalogName(event.target.value)}
                  required
                />
              </label>
              <button
                className="crm-button self-end inline-flex items-center justify-center gap-2"
                type="submit"
                disabled={addCatalogItem.isPending}
              >
                <Plus className="h-4 w-4" aria-hidden="true" />
                {addCatalogItem.isPending ? "Guardando…" : "Agregar"}
              </button>
              {catalogError && (
                <p role="alert" className="text-sm text-destructive sm:col-span-3">
                  {catalogError}
                </p>
              )}
            </form>
          </section>

          <section
            className="crm-panel space-y-5 p-6"
            aria-labelledby="customer-funnel-stages-title"
          >
            <div>
              <h2 id="customer-funnel-stages-title" className="font-display text-2xl">
                Etapas de embudos de clientes
              </h2>
              <p className="mt-1 text-sm text-muted-foreground">
                Cada etapa pertenece a un embudo. La opción predeterminada se conserva para próximos
                contactos entrantes.
              </p>
            </div>
            {customerFunnelStagesQuery.isError && (
              <p role="alert" className="text-sm text-destructive">
                No se pudieron cargar las etapas.
              </p>
            )}
            {customerCatalogsQuery.data && customerFunnelStagesQuery.data && (
              <div className="grid gap-4 sm:grid-cols-2">
                {customerFunnels.map((funnelName) => {
                  const stages = customerFunnelStagesQuery.data.stages.filter(
                    (stage) => stage.funnelName === funnelName
                  )
                  return (
                    <article key={funnelName} className="rounded-md border border-border p-4">
                      <h3 className="font-semibold">{funnelName}</h3>
                      {stages.length === 0 ? (
                        <p className="mt-2 text-sm text-muted-foreground">Sin etapas.</p>
                      ) : (
                        <ol className="mt-2 space-y-2 text-sm">
                          {stages.map((stage) => (
                            <li
                              key={stage.id}
                              className="flex items-center justify-between gap-3"
                            >
                              <span>
                                {stage.name}
                                {stage.isDefaultForInbound && (
                                  <span className="ml-2 rounded bg-accent/10 px-2 py-1 text-xs">
                                    Predeterminada
                                  </span>
                                )}
                              </span>
                              {canManageFunnels && !stage.isDefaultForInbound && (
                                <button
                                  type="button"
                                  className="rounded border border-border px-2 py-1 text-xs"
                                  disabled={setDefaultCustomerFunnelStage.isPending}
                                  onClick={() => setDefaultCustomerFunnelStage.mutate(stage.id)}
                                >
                                  Usar para contactos entrantes
                                </button>
                              )}
                            </li>
                          ))}
                        </ol>
                      )}
                      {setDefaultCustomerFunnelStage.isError && (
                        <p role="alert" className="mt-2 text-xs text-destructive">
                          No se pudo cambiar la etapa predeterminada.
                        </p>
                      )}
                    </article>
                  )
                })}
              </div>
            )}
            {canManageFunnels && customerCatalogsQuery.data && (
              <form
                className="grid gap-3 sm:grid-cols-[220px_minmax(0,1fr)_auto]"
                onSubmit={(event) => {
                  event.preventDefault()
                  setStageError("")
                  addCustomerFunnelStage.mutate()
                }}
              >
                <label className="grid gap-1 text-sm font-medium">
                  Embudo
                  <select
                    className="rounded border border-border bg-background px-3 py-2"
                    value={stageFunnel}
                    onChange={(event) => setStageFunnel(event.target.value)}
                  >
                    {customerFunnels.map((funnel) => (
                      <option key={funnel} value={funnel}>
                        {funnel}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Nueva etapa
                  <input
                    className="rounded border border-border bg-background px-3 py-2"
                    maxLength={100}
                    value={stageName}
                    onChange={(event) => setStageName(event.target.value)}
                    required
                  />
                </label>
                <button
                  className="crm-button self-end inline-flex items-center justify-center gap-2"
                  type="submit"
                  disabled={addCustomerFunnelStage.isPending}
                >
                  <Plus className="h-4 w-4" aria-hidden="true" />
                  {addCustomerFunnelStage.isPending ? "Guardando…" : "Agregar etapa"}
                </button>
                {stageError && (
                  <p role="alert" className="text-sm text-destructive sm:col-span-3">
                    {stageError}
                  </p>
                )}
              </form>
            )}
          </section>
        </div>
      ) : (
        /* --- Panel CMS Sitio Web Público --- */
        <div className="space-y-6">
          {/* Tarjeta de Vista Previa en Vivo */}
          <section
            className="overflow-hidden rounded-xl border border-border bg-card shadow-sm"
            aria-label="Vista previa del hero público"
          >
            <div className="flex items-center justify-between border-b border-border bg-muted/40 px-5 py-3 text-xs font-medium text-muted-foreground">
              <span className="flex items-center gap-2">
                <Globe className="h-4 w-4 text-accent" />
                Vista previa en vivo del Banner Principal (Página de Inicio)
              </span>
              <a
                href="http://localhost:3000"
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-1 text-accent hover:underline"
              >
                Abrir sitio público <ExternalLink className="h-3 w-3" />
              </a>
            </div>
            <div
              className="relative flex min-h-[240px] flex-col justify-end overflow-hidden p-6 sm:p-8"
              style={{
                backgroundColor: websiteDraft.primaryColor || "#0f2b1d",
              }}
            >
              {websiteDraft.heroBannerUrl && (
                <img
                  src={websiteDraft.heroBannerUrl}
                  alt="Banner preview"
                  className="absolute inset-0 h-full w-full object-cover opacity-35"
                  onError={(e) => {
                    ;(e.target as HTMLElement).style.display = "none"
                  }}
                />
              )}
              <div className="relative z-10 max-w-xl space-y-3">
                <div className="inline-block rounded bg-white/15 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-white backdrop-blur-sm">
                  {websiteDraft.agencyName || "L&C Propiedad Raíz"}
                </div>
                <h3 className="font-display text-2xl font-bold leading-tight text-white drop-shadow-sm sm:text-3xl">
                  {websiteDraft.heroTitle || "Propiedades con altura. Decisiones con criterio."}
                </h3>
                <p className="text-xs text-white/85 line-clamp-2 sm:text-sm">
                  {websiteDraft.heroSubtitle ||
                    "Inmuebles seleccionados y asesoría cercana para comprar, arrendar o vender..."}
                </p>
                <div className="flex flex-wrap gap-2 pt-2">
                  <span className="rounded bg-accent px-3 py-1 text-xs font-semibold text-accent-foreground shadow-sm">
                    Explorar propiedades
                  </span>
                  <span className="rounded border border-white/20 bg-white/10 px-3 py-1 text-xs text-white backdrop-blur-sm">
                    Buscar arriendo
                  </span>
                </div>
              </div>
            </div>
          </section>

          {/* Formulario CMS */}
          <form onSubmit={submitWebsiteConfig} className="space-y-6">
            {/* Identidad de Marca */}
            <div className="crm-panel space-y-4 p-6">
              <h2 className="font-display text-xl">1. Identidad de Marca & Eslogan</h2>
              <p className="text-xs text-muted-foreground">
                Configura el nombre corporativo y el lema que aparecerán en el encabezado, pie de
                página y metadatos del sitio.
              </p>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="grid gap-1 text-sm font-medium">
                  Nombre de la Agencia *
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.agencyName}
                    required
                    maxLength={150}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, agencyName: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Eslogan Comercial
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.slogan ?? ""}
                    maxLength={250}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, slogan: e.target.value })
                    }
                  />
                </label>
              </div>
            </div>

            {/* Banner Principal (Hero) */}
            <div className="crm-panel space-y-4 p-6">
              <h2 className="font-display text-xl">2. Banner Principal & Portada (Hero)</h2>
              <p className="text-xs text-muted-foreground">
                Títulos de impacto visual y URL de imagen de fondo que recibirán a los clientes al
                ingresar.
              </p>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="grid gap-1 text-sm font-medium sm:col-span-2">
                  Título Principal del Banner *
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.heroTitle}
                    required
                    maxLength={200}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, heroTitle: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium sm:col-span-2">
                  Subtítulo Descriptivo
                  <textarea
                    rows={2}
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.heroSubtitle ?? ""}
                    maxLength={500}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, heroSubtitle: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium sm:col-span-2">
                  URL de Imagen de Fondo / Banner
                  <input
                    type="text"
                    placeholder="/medellin-hero.png o https://cdn.ejemplo.com/hero.jpg"
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.heroBannerUrl ?? ""}
                    maxLength={500}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, heroBannerUrl: e.target.value })
                    }
                  />
                  <span className="text-xs text-muted-foreground">
                    Deja vacío o usa <code>/medellin-hero.png</code> para la imagen panorámica
                    predeterminada de Medellín.
                  </span>
                </label>
              </div>
            </div>

            {/* Acerca de la Agencia */}
            <div className="crm-panel space-y-4 p-6">
              <h2 className="font-display text-xl">3. Reseña Institucional (Acerca de)</h2>
              <div className="grid gap-4">
                <label className="grid gap-1 text-sm font-medium">
                  Título de la Sección
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.aboutTitle ?? ""}
                    maxLength={200}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, aboutTitle: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Texto o Manifiesto
                  <textarea
                    rows={3}
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.aboutText ?? ""}
                    maxLength={2000}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, aboutText: e.target.value })
                    }
                  />
                </label>
              </div>
            </div>

            {/* Canales de Contacto */}
            <div className="crm-panel space-y-4 p-6">
              <h2 className="font-display text-xl">4. Canales de Contacto & Ubicación</h2>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="grid gap-1 text-sm font-medium">
                  Teléfono de Contacto *
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.contactPhone}
                    required
                    maxLength={50}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, contactPhone: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  WhatsApp Directo (número con código de país)
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.contactWhatsapp ?? ""}
                    maxLength={50}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, contactWhatsapp: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Correo Electrónico de Contacto *
                  <input
                    type="email"
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.contactEmail}
                    required
                    maxLength={100}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, contactEmail: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Dirección Física de la Oficina
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.address ?? ""}
                    maxLength={250}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, address: e.target.value })
                    }
                  />
                </label>
              </div>
            </div>

            {/* Redes Sociales */}
            <div className="crm-panel space-y-4 p-6">
              <h2 className="font-display text-xl">5. Redes Sociales</h2>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="grid gap-1 text-sm font-medium">
                  Perfil de Instagram (URL completa)
                  <input
                    type="url"
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.instagramUrl ?? ""}
                    maxLength={250}
                    placeholder="https://instagram.com/tu_agencia"
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, instagramUrl: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Página de Facebook (URL completa)
                  <input
                    type="url"
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.facebookUrl ?? ""}
                    maxLength={250}
                    placeholder="https://facebook.com/tu_agencia"
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, facebookUrl: e.target.value })
                    }
                  />
                </label>
              </div>
            </div>

            {/* SEO & Colores de Marca */}
            <div className="crm-panel space-y-4 p-6">
              <h2 className="font-display text-xl">6. SEO & Colores de Marca</h2>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="grid gap-1 text-sm font-medium sm:col-span-2">
                  Título SEO (Meta Title)
                  <input
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.seoTitle ?? ""}
                    maxLength={150}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, seoTitle: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium sm:col-span-2">
                  Descripción SEO (Meta Description)
                  <textarea
                    rows={2}
                    className="mt-1 w-full rounded border border-border bg-background px-3 py-2 font-normal"
                    value={websiteDraft.seoDescription ?? ""}
                    maxLength={300}
                    onChange={(e) =>
                      setWebsiteDraft({ ...websiteDraft, seoDescription: e.target.value })
                    }
                  />
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Color Primario (Hexadecimal)
                  <div className="flex items-center gap-2 mt-1">
                    <input
                      type="color"
                      className="h-10 w-12 cursor-pointer rounded border border-border bg-background p-1"
                      value={
                        websiteDraft.primaryColor && /^#[0-9A-Fa-f]{6}$/.test(websiteDraft.primaryColor)
                          ? websiteDraft.primaryColor
                          : "#0f2b1d"
                      }
                      onChange={(e) =>
                        setWebsiteDraft({ ...websiteDraft, primaryColor: e.target.value })
                      }
                    />
                    <input
                      className="w-full rounded border border-border bg-background px-3 py-2 font-mono text-sm"
                      value={websiteDraft.primaryColor ?? ""}
                      placeholder="#0f2b1d"
                      maxLength={30}
                      onChange={(e) =>
                        setWebsiteDraft({ ...websiteDraft, primaryColor: e.target.value })
                      }
                    />
                  </div>
                </label>
                <label className="grid gap-1 text-sm font-medium">
                  Color de Acento (Hexadecimal)
                  <div className="flex items-center gap-2 mt-1">
                    <input
                      type="color"
                      className="h-10 w-12 cursor-pointer rounded border border-border bg-background p-1"
                      value={
                        websiteDraft.accentColor && /^#[0-9A-Fa-f]{6}$/.test(websiteDraft.accentColor)
                          ? websiteDraft.accentColor
                          : "#c59b27"
                      }
                      onChange={(e) =>
                        setWebsiteDraft({ ...websiteDraft, accentColor: e.target.value })
                      }
                    />
                    <input
                      className="w-full rounded border border-border bg-background px-3 py-2 font-mono text-sm"
                      value={websiteDraft.accentColor ?? ""}
                      placeholder="#c59b27"
                      maxLength={30}
                      onChange={(e) =>
                        setWebsiteDraft({ ...websiteDraft, accentColor: e.target.value })
                      }
                    />
                  </div>
                </label>
              </div>
            </div>

            {/* Barra de Acciones */}
            <div className="flex flex-wrap items-center justify-between gap-4 pt-2">
              <div className="flex items-center gap-3">
                <button
                  type="submit"
                  disabled={updateWebsiteConfigMutation.isPending}
                  className="crm-button inline-flex items-center gap-2 px-6 py-2.5 shadow-sm"
                >
                  <Save className="h-4 w-4" />
                  {updateWebsiteConfigMutation.isPending ? "Guardando..." : "Guardar cambios en sitio web"}
                </button>
                <button
                  type="button"
                  disabled={updateWebsiteConfigMutation.isPending}
                  onClick={resetWebsiteConfig}
                  className="inline-flex items-center gap-2 rounded border border-border px-4 py-2 text-sm hover:bg-muted"
                >
                  <RefreshCw className="h-4 w-4" />
                  Restaurar
                </button>
              </div>

              {websiteSuccessMsg && (
                <span className="flex items-center gap-1.5 text-sm font-medium text-emerald-700">
                  <Check className="h-4 w-4" /> Configuración web publicada exitosamente.
                </span>
              )}
              {updateWebsiteConfigMutation.isError && (
                <span role="alert" className="text-sm text-destructive">
                  Error:{" "}
                  {updateWebsiteConfigMutation.error instanceof Error
                    ? updateWebsiteConfigMutation.error.message
                    : "No se pudo actualizar la configuración web."}
                </span>
              )}
            </div>
          </form>
        </div>
      )}
    </main>
  )
}

