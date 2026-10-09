import { useMemo, useState } from "react"
import {
  useChangePublicationStatus,
  useCreatePublication,
  usePublicationOptions,
  usePublications,
  useUpdatePublication,
  useUploadPublicationPhoto,
} from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
import { api } from "@/api/client"
import type {
  PropertyPublicationAdmin,
  PropertyPublicationInput,
  PublicationPropertyOption,
} from "@/lib/types"
import {
  Archive,
  Camera,
  CirclePause,
  ExternalLink,
  ImagePlus,
  Loader2,
  MapPin,
  Pencil,
  Plus,
  Send,
  X,
} from "lucide-react"

type PublicationAction = "publish" | "pause" | "withdraw"
const PUBLIC_SITE_URL = (import.meta.env.VITE_PUBLIC_SITE_URL ?? "http://localhost:3000").replace(/\/$/, "")

const STATUS_LABELS: Record<PropertyPublicationAdmin["status"], string> = {
  Draft: "Borrador",
  Published: "Publicada",
  Paused: "Pausada",
  Withdrawn: "Retirada",
}

const STATUS_STYLES: Record<PropertyPublicationAdmin["status"], string> = {
  Draft: "bg-muted text-muted-foreground",
  Published: "bg-success/12 text-success",
  Paused: "bg-warning/12 text-warning",
  Withdrawn: "bg-destructive/12 text-destructive",
}

const EMPTY_FORM = {
  propertyId: "",
  advisorId: "",
  slug: "",
  publicTitle: "",
  publicDescription: "",
  priceAmount: "",
  priceCurrency: "COP",
  municipality: "",
  neighborhood: "",
  approximateLatitude: "",
  approximateLongitude: "",
}

type PublicationForm = typeof EMPTY_FORM

function toSlug(value: string) {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "")
}

function toInput(form: PublicationForm): PropertyPublicationInput {
  return {
    propertyId: form.propertyId || undefined,
    advisorId: form.advisorId || undefined,
    slug: form.slug,
    publicTitle: form.publicTitle,
    publicDescription: form.publicDescription,
    priceAmount: Number(form.priceAmount),
    priceCurrency: form.priceCurrency,
    municipality: form.municipality,
    neighborhood: form.neighborhood,
    approximateLatitude: Number(form.approximateLatitude),
    approximateLongitude: Number(form.approximateLongitude),
  }
}

function fromPublication(publication: PropertyPublicationAdmin): PublicationForm {
  return {
    propertyId: publication.propertyId,
    advisorId: publication.advisorId,
    slug: publication.slug,
    publicTitle: publication.publicTitle,
    publicDescription: publication.publicDescription,
    priceAmount: String(publication.priceAmount ?? ""),
    priceCurrency: publication.priceCurrency ?? "COP",
    municipality: publication.municipality ?? "",
    neighborhood: publication.neighborhood ?? "",
    approximateLatitude: String(publication.approximateLatitude ?? ""),
    approximateLongitude: String(publication.approximateLongitude ?? ""),
  }
}

export function PublicacionesPage() {
  const { data: publications = [], isLoading, error } = usePublications()
  const { data: options } = usePublicationOptions()
  const changeStatus = useChangePublicationStatus()
  const [editor, setEditor] = useState<PropertyPublicationAdmin | "new" | null>(null)
  const published = publications.filter((item) => item.status === "Published").length
  const blocked = publications.filter((item) => item.photos.length < 3).length

  async function act(publication: PropertyPublicationAdmin, action: PublicationAction) {
    if (action === "withdraw" && !window.confirm("Retirar es definitivo. ¿Continuar?")) return
    try {
      await changeStatus.mutateAsync({ id: publication.id, action })
    } catch {
      // El error queda visible mediante el estado de la mutacion.
    }
  }

  return (
    <div className="space-y-7">
      <header className="grid gap-6 border-b border-border pb-7 lg:grid-cols-[1fr_auto] lg:items-end">
        <div>
          <span className="crm-eyebrow">Vitrina pública</span>
          <h1 className="crm-page-title mt-2">Mesa de publicación</h1>
          <p className="mt-3 max-w-2xl text-sm text-muted-foreground">
            Controla qué ve el visitante. La dirección exacta nunca sale del CRM.
          </p>
        </div>
        <button className="crm-button" onClick={() => setEditor("new")}>
          <Plus className="h-4 w-4" /> Nuevo borrador
        </button>
      </header>

      <section className="grid gap-3 sm:grid-cols-3" aria-label="Resumen de publicaciones">
        <Metric label="Total" value={publications.length} />
        <Metric label="En la web" value={published} accent />
        <Metric label="Faltan fotos" value={blocked} warning={blocked > 0} />
      </section>

      {isLoading && <StateMessage text="Cargando publicaciones…" />}
      {error && <StateMessage text={error.message} danger />}
      {changeStatus.error && <StateMessage text={changeStatus.error.message} danger />}
      {!isLoading && !error && publications.length === 0 && (
        <StateMessage text="No hay publicaciones. Crea el primer borrador desde un inmueble activo." />
      )}

      <section className="space-y-3">
        {publications.map((publication) => (
          <article key={publication.id} className="crm-panel overflow-hidden">
            <div className="grid gap-5 p-5 lg:grid-cols-[7rem_1fr_auto] lg:items-center">
              <div className="flex h-24 items-center justify-center overflow-hidden rounded-sm bg-muted">
                {publication.photos[0] ? (
                  <img
                    src={api.resolveMediaUrl(publication.photos.find((photo) => photo.isCover)?.url ?? publication.photos[0].url)}
                    alt=""
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <Camera className="h-7 w-7 text-muted-foreground" />
                )}
              </div>

              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-2">
                  <span className={`rounded-full px-2.5 py-1 text-[.68rem] font-bold uppercase tracking-wider ${STATUS_STYLES[publication.status]}`}>
                    {STATUS_LABELS[publication.status]}
                  </span>
                  <span className="text-xs text-muted-foreground">
                    {publication.photos.length}/3 fotos mínimas
                  </span>
                </div>
                <h2 className="mt-2 truncate font-display text-2xl">{publication.publicTitle}</h2>
                <p className="mt-1 truncate text-sm text-muted-foreground">
                  {publication.propertyName} · {publication.propertyType} · {publication.operation}
                </p>
                <div className="mt-3 flex flex-wrap gap-x-5 gap-y-1 text-xs text-muted-foreground">
                  <span className="flex items-center gap-1"><MapPin className="h-3 w-3" />{publication.neighborhood}, {publication.municipality}</span>
                  <span>{publication.advisorName}</span>
                  <span className="font-semibold text-foreground">
                    {publication.priceAmount?.toLocaleString("es-CO", { style: "currency", currency: publication.priceCurrency ?? "COP", maximumFractionDigits: 0 })}
                  </span>
                </div>
              </div>

              <div className="flex flex-wrap gap-2 lg:max-w-64 lg:justify-end">
                <button className="publication-action" onClick={() => setEditor(publication)} disabled={publication.status === "Withdrawn"}>
                  <Pencil className="h-4 w-4" /> Editar
                </button>
                {publication.status === "Draft" || publication.status === "Paused" ? (
                  <button className="publication-action publication-action-primary" onClick={() => act(publication, "publish")} disabled={changeStatus.isPending}>
                    <Send className="h-4 w-4" /> Publicar
                  </button>
                ) : null}
                {publication.status === "Published" && (
                  <button className="publication-action" onClick={() => act(publication, "pause")} disabled={changeStatus.isPending}>
                    <CirclePause className="h-4 w-4" /> Pausar
                  </button>
                )}
                {(publication.status === "Published" || publication.status === "Paused") && (
                  <button className="publication-action text-destructive" onClick={() => act(publication, "withdraw")} disabled={changeStatus.isPending}>
                    <Archive className="h-4 w-4" /> Retirar
                  </button>
                )}
                {publication.status === "Published" && (
                  <a className="publication-action" href={`${PUBLIC_SITE_URL}/inmuebles/${publication.slug}`} target="_blank" rel="noreferrer">
                    <ExternalLink className="h-4 w-4" /> Ver
                  </a>
                )}
              </div>
            </div>
          </article>
        ))}
      </section>

      {editor && (
        <PublicationEditor
          publication={editor === "new" ? null : editor}
          properties={options?.properties ?? []}
          advisors={options?.advisors ?? []}
          onClose={() => setEditor(null)}
        />
      )}
    </div>
  )
}

function Metric({ label, value, accent = false, warning = false }: { label: string; value: number; accent?: boolean; warning?: boolean }) {
  return (
    <div className={`crm-panel border-l-4 p-4 ${accent ? "border-l-success" : warning ? "border-l-warning" : "border-l-accent"}`}>
      <span className="text-[.67rem] font-bold uppercase tracking-[.15em] text-muted-foreground">{label}</span>
      <strong className="mt-1 block font-display text-3xl">{value}</strong>
    </div>
  )
}

function StateMessage({ text, danger = false }: { text: string; danger?: boolean }) {
  return <div className={`crm-panel p-8 text-center text-sm ${danger ? "text-destructive" : "text-muted-foreground"}`}>{text}</div>
}

function PublicationEditor({
  publication,
  properties,
  advisors,
  onClose,
}: {
  publication: PropertyPublicationAdmin | null
  properties: PublicationPropertyOption[]
  advisors: { id: string; name: string }[]
  onClose: () => void
}) {
  const user = useAuthStore((state) => state.user)
  const createPublication = useCreatePublication()
  const updatePublication = useUpdatePublication()
  const uploadPhoto = useUploadPublicationPhoto()
  const [form, setForm] = useState<PublicationForm>(() => publication ? fromPublication(publication) : EMPTY_FORM)
  const [file, setFile] = useState<File | null>(null)
  const [altText, setAltText] = useState("")
  const [photoCount, setPhotoCount] = useState(publication?.photos.length ?? 0)
  const mutation = publication ? updatePublication : createPublication
  const error = mutation.error ?? uploadPhoto.error
  const selectedProperty = useMemo(
    () => properties.find((item) => item.id === form.propertyId),
    [properties, form.propertyId]
  )

  function update(name: keyof PublicationForm, value: string) {
    setForm((current) => ({ ...current, [name]: value }))
  }

  function selectProperty(propertyId: string) {
    const property = properties.find((item) => item.id === propertyId)
    if (!property) return update("propertyId", propertyId)
    setForm((current) => ({
      ...current,
      propertyId,
      publicTitle: property.name,
      slug: toSlug(property.name),
      priceAmount: String(property.priceAmount),
      priceCurrency: property.priceCurrency,
      municipality: property.municipality,
      neighborhood: property.neighborhood ?? "",
    }))
  }

  async function submit(event: React.FormEvent) {
    event.preventDefault()
    const input = {
      ...toInput(form),
      advisorId: form.advisorId || advisors[0]?.id,
    }
    try {
      if (publication) await updatePublication.mutateAsync({ id: publication.id, data: input })
      else await createPublication.mutateAsync(input)
      onClose()
    } catch {
      // El formulario conserva los datos y muestra el error del backend.
    }
  }

  async function upload() {
    if (!publication || !file || !altText.trim()) return
    try {
      await uploadPhoto.mutateAsync({
        publicationId: publication.id,
        file,
        altText: altText.trim(),
        isCover: photoCount === 0,
      })
      setFile(null)
      setAltText("")
      setPhotoCount((count) => count + 1)
    } catch {
      // El archivo sigue seleccionado y el error queda visible.
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-foreground/55">
      <div className="h-full w-full max-w-2xl overflow-y-auto bg-card shadow-2xl">
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-border bg-card/95 px-5 py-4 backdrop-blur">
          <div>
            <span className="crm-eyebrow">{publication ? "Edición" : "Nuevo borrador"}</span>
            <h2 className="mt-1 font-display text-2xl">{publication?.publicTitle ?? "Preparar publicación"}</h2>
          </div>
          <button onClick={onClose} className="rounded-sm p-2 hover:bg-muted" aria-label="Cerrar"><X className="h-5 w-5" /></button>
        </div>

        <form onSubmit={submit} className="space-y-7 p-5 md:p-7">
          {!publication && (
            <section className="grid gap-4 sm:grid-cols-2">
              <Field label="Inmueble" wide>
                <select value={form.propertyId} onChange={(event) => selectProperty(event.target.value)} required className="publication-input">
                  <option value="">Seleccionar inmueble activo</option>
                  {properties.map((property) => <option key={property.id} value={property.id}>{property.name} · {property.operation}</option>)}
                </select>
              </Field>
              {user?.rol === "Administrador" && (
                <Field label="Asesor responsable" wide>
                  <select value={form.advisorId} onChange={(event) => update("advisorId", event.target.value)} required className="publication-input">
                    <option value="">Seleccionar asesor</option>
                    {advisors.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}
                  </select>
                </Field>
              )}
              {selectedProperty && <p className="sm:col-span-2 text-xs text-muted-foreground">{selectedProperty.propertyType} · {selectedProperty.operation}</p>}
            </section>
          )}

          <section className="grid gap-4 sm:grid-cols-2">
            <Field label="Título público" wide><input className="publication-input" value={form.publicTitle} onChange={(event) => update("publicTitle", event.target.value)} required /></Field>
            <Field label="Slug" wide><input className="publication-input font-mono text-sm" value={form.slug} onChange={(event) => update("slug", toSlug(event.target.value))} required /></Field>
            <Field label="Descripción pública" wide><textarea className="publication-input min-h-28 resize-y" value={form.publicDescription} onChange={(event) => update("publicDescription", event.target.value)} /></Field>
            <Field label="Precio"><input type="number" min="1" className="publication-input" value={form.priceAmount} onChange={(event) => update("priceAmount", event.target.value)} required /></Field>
            <Field label="Moneda"><input className="publication-input uppercase" maxLength={3} value={form.priceCurrency} onChange={(event) => update("priceCurrency", event.target.value.toUpperCase())} required /></Field>
          </section>

          <section>
            <h3 className="font-display text-xl">Ubicación aproximada</h3>
            <p className="mt-1 text-xs text-muted-foreground">La API redondea a tres decimales. Nunca escribas la dirección exacta.</p>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <Field label="Municipio"><input className="publication-input" value={form.municipality} onChange={(event) => update("municipality", event.target.value)} required /></Field>
              <Field label="Barrio o sector"><input className="publication-input" value={form.neighborhood} onChange={(event) => update("neighborhood", event.target.value)} required /></Field>
              <Field label="Latitud"><input type="number" step="0.000001" className="publication-input" value={form.approximateLatitude} onChange={(event) => update("approximateLatitude", event.target.value)} required /></Field>
              <Field label="Longitud"><input type="number" step="0.000001" className="publication-input" value={form.approximateLongitude} onChange={(event) => update("approximateLongitude", event.target.value)} required /></Field>
            </div>
          </section>

          {publication && publication.status !== "Withdrawn" && (
            <section className="rounded-md border border-border bg-background p-4">
              <div className="flex items-center gap-2"><ImagePlus className="h-4 w-4 text-accent" /><h3 className="font-semibold">Agregar foto</h3></div>
              <p className="mt-1 text-xs text-muted-foreground">{photoCount}/3 mínimas. La primera queda como portada.</p>
              <div className="mt-4 grid gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
                <Field label="Archivo"><input type="file" accept="image/jpeg,image/png,image/webp" onChange={(event) => setFile(event.target.files?.[0] ?? null)} className="publication-input text-xs" /></Field>
                <Field label="Texto alternativo"><input value={altText} onChange={(event) => setAltText(event.target.value)} className="publication-input" /></Field>
                <button type="button" onClick={upload} disabled={!file || !altText.trim() || uploadPhoto.isPending} className="crm-button">
                  {uploadPhoto.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <ImagePlus className="h-4 w-4" />} Subir
                </button>
              </div>
            </section>
          )}

          {error && <p role="alert" className="rounded-sm bg-destructive/10 p-3 text-sm text-destructive">{error.message}</p>}

          <div className="flex justify-end gap-2 border-t border-border pt-5">
            <button type="button" onClick={onClose} className="publication-action">Cancelar</button>
            <button type="submit" className="crm-button" disabled={mutation.isPending}>
              {mutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
              {publication ? "Guardar cambios" : "Crear borrador"}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

function Field({ label, wide = false, children }: { label: string; wide?: boolean; children: React.ReactNode }) {
  return <label className={wide ? "sm:col-span-2" : ""}><span className="text-xs font-bold uppercase tracking-[.08em] text-muted-foreground">{label}</span>{children}</label>
}
