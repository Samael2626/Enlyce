import { useEffect, useState } from "react"
import { useAdvisors, useChangeInmuebleTrashState, useChangeInmueblesTrashState, useCreateInmueble, useInmueble, useInmuebles, usePropertyDemandCounts, useUpdateInmueble } from "@/hooks/useApi"
import { PropertyDemandsPanel } from "@/components/PropertyDemandsPanel"
import { TIPOS_INMUEBLE, MODALIDADES_INMUEBLE, ETIQUETAS_MODALIDAD_INMUEBLE } from "@/lib/constants"
import { api } from "@/api/client"
import { crmPropertySlug, downloadPropertySheet, propertyWhatsappUrl } from "@/lib/propertySheet"
import { useAuthStore } from "@/stores/authStore"
import { useSearchParams } from "react-router-dom"
import {
  Home,
  Plus,
  X,
  Loader2,
  Mail,
  Search,
} from "lucide-react"

const splitFeatures = (value: string) => value.split(",").map((item) => item.trim()).filter(Boolean)

export function InmueblesPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [showForm, setShowForm] = useState(false)
  const [page, setPage] = useState(1)
  const [editingId, setEditingId] = useState("")
  const [showTrash, setShowTrash] = useState(false)
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [draftFilters, setDraftFilters] = useState({
    search: "", codigoWasi: "", tipo: "", modalidad: "", estado: "", pais: "", departamento: "", ciudad: "", barrio: "", asesorResponsableId: "",
    moneda: "COP", precioDesde: "", precioHasta: "", areaDesde: "", areaHasta: "",
    habitaciones: "", banos: "", parqueaderos: "", construidoDesde: "", etiqueta: "", orden: "recent",
  })
  const [filters, setFilters] = useState<Record<string, string>>({ orden: "recent" })
  const [crossProperty, setCrossProperty] = useState<{ id: string; name: string } | null>(null)
  const [emailingInventory, setEmailingInventory] = useState(false)
  const [inventoryExportMessage, setInventoryExportMessage] = useState("")
  const [sheetError, setSheetError] = useState("")
  const [sheetLoading, setSheetLoading] = useState(false)
  const inventory = useInmuebles({
    page, ...filters, papelera: showTrash,
    precioDesde: filters.precioDesde ? Number(filters.precioDesde) : undefined,
    precioHasta: filters.precioHasta ? Number(filters.precioHasta) : undefined,
    areaDesde: filters.areaDesde ? Number(filters.areaDesde) : undefined,
    areaHasta: filters.areaHasta ? Number(filters.areaHasta) : undefined,
    habitaciones: filters.habitaciones ? Number(filters.habitaciones) : undefined,
    banos: filters.banos ? Number(filters.banos) : undefined,
    parqueaderos: filters.parqueaderos ? Number(filters.parqueaderos) : undefined,
    construidoDesde: filters.construidoDesde ? Number(filters.construidoDesde) : undefined,
  })
  const user = useAuthStore((state) => state.user)
  const advisorQuery = useAdvisors(user?.rol === "Administrador")
  const demandCounts = usePropertyDemandCounts(showTrash ? [] : inventory.data?.items.map((item) => item.id) ?? [])
  const createInmueble = useCreateInmueble()
  const updateInmueble = useUpdateInmueble()
  const changeTrashState = useChangeInmuebleTrashState()
  const changeInmueblesTrashState = useChangeInmueblesTrashState()
  const editingProperty = useInmueble(editingId)

  async function downloadSheet() {
    if (!editingId) return
    setSheetError("")
    setSheetLoading(true)
    try {
      await downloadPropertySheet(crmPropertySlug(editingId))
    } catch {
      setSheetError("La ficha pública de este inmueble no está disponible. Revisa su estado antes de compartirla.")
    } finally {
      setSheetLoading(false)
    }
  }

  useEffect(() => {
    const property = editingProperty.data
    if (!editingId || !property) return
    setForm({
      nombre: property.nombre, descripcion: property.descripcion, tipo: property.tipo, modalidad: property.modalidad,
      calle: property.calle, ciudad: property.ciudad, barrio: property.barrio ?? "", precio: String(property.precio),
      moneda: property.moneda, metrosCuadrados: String(property.metrosCuadrados), habitaciones: String(property.habitaciones),
      banos: String(property.banos), parqueaderos: String(property.parqueaderos), anioConstruccion: property.anioConstruccion == null ? "" : String(property.anioConstruccion), estadoFisico: property.estadoFisico ?? "", etiquetas: property.etiquetas?.join(", ") ?? "", propietarioId: property.propietarioId, asesorResponsableId: property.asesorResponsableId ?? "",
      caracteristicasInternas: property.caracteristicasInternas.join(", "),
      caracteristicasExternas: property.caracteristicasExternas.join(", "),
    })
  }, [editingId, editingProperty.data])

  const [form, setForm] = useState({
    nombre: "",
    descripcion: "",
    tipo: "Apartamento",
    modalidad: "Venta",
    calle: "",
    ciudad: "",
    barrio: "",
    precio: "",
    moneda: "COP",
    metrosCuadrados: "",
    habitaciones: "",
    banos: "",
    parqueaderos: "",
    anioConstruccion: "",
    estadoFisico: "",
    etiquetas: "",
    propietarioId: "",
    asesorResponsableId: "",
    caracteristicasInternas: "",
    caracteristicasExternas: "",
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    const data = {
        ...form,
        precio: parseFloat(form.precio) || 0,
        metrosCuadrados: parseFloat(form.metrosCuadrados) || 0,
        habitaciones: parseInt(form.habitaciones) || 0,
        banos: parseInt(form.banos) || 0,
        parqueaderos: parseInt(form.parqueaderos) || 0,
        anioConstruccion: form.anioConstruccion ? parseInt(form.anioConstruccion) : null,
        asesorResponsableId: form.asesorResponsableId || null,
        estadoFisico: form.estadoFisico.trim() || null,
        etiquetas: splitFeatures(form.etiquetas),
        caracteristicasInternas: splitFeatures(form.caracteristicasInternas),
        caracteristicasExternas: splitFeatures(form.caracteristicasExternas),
      }
    const onSuccess = () => {
      setShowForm(false)
      setEditingId("")
      setForm({ nombre: "", descripcion: "", tipo: "Apartamento", modalidad: "Venta", calle: "", ciudad: "", barrio: "", precio: "", moneda: "COP", metrosCuadrados: "", habitaciones: "", banos: "", parqueaderos: "", anioConstruccion: "", estadoFisico: "", etiquetas: "", propietarioId: "", asesorResponsableId: "", caracteristicasInternas: "", caracteristicasExternas: "" })
    }
    if (editingId) {
      updateInmueble.mutate({ id: editingId, data }, { onSuccess })
    } else {
      createInmueble.mutate(data, {
        onSuccess: () => {
          onSuccess()
        },
      })
    }
  }

  const beginEditing = async (id: string) => {
    setEditingId(id)
    setShowForm(true)
  }

  useEffect(() => {
    const editId = searchParams.get("edit")
    if (!editId) return
    setEditingId(editId)
    setShowForm(true)
  }, [searchParams])

  useEffect(() => {
    const editId = searchParams.get("edit")
    const property = editingProperty.data
    if (!editId || editingId !== editId || !property) return
    const canEdit = user?.rol === "Administrador" || property.asesorResponsableId === user?.id
    if (!canEdit) return
    const nextParams = new URLSearchParams(searchParams)
    nextParams.delete("edit")
    setSearchParams(nextParams, { replace: true })
  }, [editingId, editingProperty.data, searchParams, setSearchParams, user?.id, user?.rol])

  return (
    <div className="space-y-7">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Inventario</span>
          <h1 className="crm-page-title mt-2">{showTrash ? "Papelera de inmuebles" : "Inmuebles"}</h1>
          <p className="mt-3 text-sm text-muted-foreground">
            Catálogo de propiedades
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <button type="button" onClick={() => { setShowTrash(!showTrash); setShowForm(false); setEditingId(""); setSelectedIds([]); setPage(1) }} className="rounded border border-border px-3 py-2 text-sm font-semibold">
            {showTrash ? "Volver al inventario" : "Ver papelera"}
          </button>
          {!showTrash && <>
          <button
            type="button"
            disabled={emailingInventory}
            onClick={async () => {
              setEmailingInventory(true)
              setInventoryExportMessage("")
              try {
                const result = await api.emailPropertiesXlsx()
                setInventoryExportMessage(`Se enviaron ${result.count} inmuebles al correo de tu cuenta.`)
              } catch (error) {
                setInventoryExportMessage(error instanceof Error ? error.message : "No se pudo enviar el archivo.")
              } finally {
                setEmailingInventory(false)
              }
            }}
            className="inline-flex items-center gap-2 rounded border border-border px-3 py-2 text-sm font-semibold text-foreground disabled:opacity-50"
          >
            <Mail className="h-4 w-4" />
            {emailingInventory ? "Enviando…" : "Enviar inventario a mi correo"}
          </button>
          <button onClick={() => { setEditingId(""); setShowForm(true) }} className="crm-button">
            <Plus className="h-4 w-4" />
            Nuevo Inmueble
          </button>
          </>}
        </div>
      </div>
      {inventoryExportMessage && <p role="status" className="text-sm text-muted-foreground">{inventoryExportMessage}</p>}

      {!showTrash && <form onSubmit={(event) => {
        event.preventDefault()
        setPage(1)
        setSelectedIds([])
        setFilters(Object.fromEntries(Object.entries(draftFilters).filter(([key, value]) => key === "orden" || value !== "")))
      }} className="crm-panel space-y-4 p-4">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <input aria-label="Buscar inmueble" placeholder="Título, dirección o ciudad" value={draftFilters.search} onChange={(e) => setDraftFilters({ ...draftFilters, search: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          <input aria-label="Código Wasi" placeholder="Código Wasi" value={draftFilters.codigoWasi} onChange={(e) => setDraftFilters({ ...draftFilters, codigoWasi: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          <select aria-label="Tipo de inmueble" value={draftFilters.tipo} onChange={(e) => setDraftFilters({ ...draftFilters, tipo: e.target.value })} className="rounded-md border bg-background px-3 py-2"><option value="">Todos los tipos</option>{TIPOS_INMUEBLE.map((value) => <option key={value}>{value}</option>)}</select>
          <select aria-label="Modalidad" value={draftFilters.modalidad} onChange={(e) => setDraftFilters({ ...draftFilters, modalidad: e.target.value })} className="rounded-md border bg-background px-3 py-2"><option value="">Todas las modalidades</option>{MODALIDADES_INMUEBLE.map((value) => <option key={value} value={value}>{ETIQUETAS_MODALIDAD_INMUEBLE[value] ?? value}</option>)}</select>
          <select aria-label="Estado" value={draftFilters.estado} onChange={(e) => setDraftFilters({ ...draftFilters, estado: e.target.value })} className="rounded-md border bg-background px-3 py-2"><option value="">Todos los estados</option>{["Disponible", "Reservado", "Vendido", "Arrendado", "Inactivo"].map((value) => <option key={value}>{value}</option>)}</select>
          {(["pais", "departamento", "ciudad", "barrio"] as const).map((key) => <input key={key} aria-label={key} placeholder={key[0].toUpperCase() + key.slice(1)} value={draftFilters[key]} onChange={(e) => setDraftFilters({ ...draftFilters, [key]: e.target.value })} className="rounded-md border bg-background px-3 py-2" />)}
          <input aria-label="Precio mínimo" type="number" min="0" placeholder="Precio desde" value={draftFilters.precioDesde} onChange={(e) => setDraftFilters({ ...draftFilters, precioDesde: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          <input aria-label="Precio máximo" type="number" min="0" placeholder="Precio hasta" value={draftFilters.precioHasta} onChange={(e) => setDraftFilters({ ...draftFilters, precioHasta: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          <select aria-label="Moneda" value={draftFilters.moneda} onChange={(e) => setDraftFilters({ ...draftFilters, moneda: e.target.value })} className="rounded-md border bg-background px-3 py-2"><option value="COP">COP</option><option value="USD">USD</option></select>
          <input aria-label="Área mínima" type="number" min="0" placeholder="Área desde m²" value={draftFilters.areaDesde} onChange={(e) => setDraftFilters({ ...draftFilters, areaDesde: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          <input aria-label="Área máxima" type="number" min="0" placeholder="Área hasta m²" value={draftFilters.areaHasta} onChange={(e) => setDraftFilters({ ...draftFilters, areaHasta: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          {(["habitaciones", "banos", "parqueaderos"] as const).map((key) => <input key={key} aria-label={key} type="number" min="0" placeholder={`${key[0].toUpperCase()}${key.slice(1)} mín.`} value={draftFilters[key]} onChange={(e) => setDraftFilters({ ...draftFilters, [key]: e.target.value })} className="rounded-md border bg-background px-3 py-2" />)}
          <input aria-label="Construido desde el año" type="number" min="1" max="9999" placeholder="Construido desde el año" value={draftFilters.construidoDesde} onChange={(e) => setDraftFilters({ ...draftFilters, construidoDesde: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          <input aria-label="Etiqueta de inmueble" placeholder="Etiqueta" value={draftFilters.etiqueta} onChange={(e) => setDraftFilters({ ...draftFilters, etiqueta: e.target.value })} className="rounded-md border bg-background px-3 py-2" />
          {user?.rol === "Administrador" && <select aria-label="Encargado del inmueble" value={draftFilters.asesorResponsableId} onChange={(e) => setDraftFilters({ ...draftFilters, asesorResponsableId: e.target.value })} className="rounded-md border bg-background px-3 py-2"><option value="">Todos los encargados</option>{advisorQuery.data?.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}</select>}
          <select aria-label="Ordenar" value={draftFilters.orden} onChange={(e) => setDraftFilters({ ...draftFilters, orden: e.target.value })} className="rounded-md border bg-background px-3 py-2"><option value="recent">Más recientes</option><option value="oldest">Más antiguos</option><option value="priceAsc">Menor precio</option><option value="priceDesc">Mayor precio</option></select>
        </div>
        <div className="flex gap-2"><button className="crm-button" type="submit"><Search className="h-4 w-4" /> Buscar</button><button type="button" className="rounded border px-3 py-2 text-sm" onClick={() => { const empty = { search: "", codigoWasi: "", tipo: "", modalidad: "", estado: "", pais: "", departamento: "", ciudad: "", barrio: "", asesorResponsableId: "", moneda: "COP", precioDesde: "", precioHasta: "", areaDesde: "", areaHasta: "", habitaciones: "", banos: "", parqueaderos: "", construidoDesde: "", etiqueta: "", orden: "recent" }; setDraftFilters(empty); setFilters({ orden: "recent" }); setSelectedIds([]); setPage(1) }}>Limpiar</button></div>
      </form>}

      {/* Form modal */}
      {showForm && !showTrash && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/55 p-4">
          <div className="crm-panel max-h-[90vh] w-full max-w-2xl overflow-y-auto border-t-4 border-t-accent">
            <div className="flex items-center justify-between border-b border-border p-5">
              <h2 className="font-display text-2xl text-foreground">
                {editingId ? "Editar inmueble" : "Nuevo Inmueble"}
              </h2>
              <button
                onClick={() => { setEditingId(""); setShowForm(false) }}
                className="rounded-sm p-2 hover:bg-muted"
                aria-label="Cerrar formulario"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5 p-5 md:p-7">
              {sheetError && <p role="alert" className="text-sm text-destructive">{sheetError}</p>}
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                {editingProperty.isError && editingId && <p role="alert" className="text-sm text-destructive sm:col-span-2">No se pudo cargar el inmueble para editar.</p>}
                <div className="sm:col-span-2">
                  <label className="text-sm font-medium">Nombre</label>
                  <input
                    type="text"
                    value={form.nombre}
                    onChange={(e) =>
                      setForm({ ...form, nombre: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                    required
                  />
                </div>

                <label className="text-sm font-medium sm:col-span-2">Descripción
                  <textarea value={form.descripcion} onChange={(e) => setForm({ ...form, descripcion: e.target.value })} className="mt-1 w-full rounded-md border bg-background px-3 py-2" rows={4} />
                </label>

                <div>
                  <label className="text-sm font-medium">Tipo</label>
                  <select
                    value={form.tipo}
                    onChange={(e) =>
                      setForm({ ...form, tipo: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  >
                    {TIPOS_INMUEBLE.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="text-sm font-medium">Modalidad</label>
                  <select
                    value={form.modalidad}
                    onChange={(e) =>
                      setForm({ ...form, modalidad: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  >
                    {MODALIDADES_INMUEBLE.map((m) => (
                      <option key={m} value={m}>
                        {ETIQUETAS_MODALIDAD_INMUEBLE[m] ?? m}
                      </option>
                    ))}
                  </select>
                </div>

                {user?.rol === "Administrador" && <label className="text-sm font-medium">Encargado del inmueble
                  <select value={form.asesorResponsableId} onChange={(event) => setForm({ ...form, asesorResponsableId: event.target.value })} className="mt-1 w-full rounded-md border bg-background px-3 py-2">
                    <option value="">Sin asignar</option>{advisorQuery.data?.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}
                  </select>
                </label>}

                <div className="col-span-2">
                  <label className="text-sm font-medium">Direccion</label>
                  <input
                    type="text"
                    value={form.calle}
                    onChange={(e) =>
                      setForm({ ...form, calle: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                    placeholder="Calle, numero"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">Ciudad</label>
                  <input
                    type="text"
                    value={form.ciudad}
                    onChange={(e) =>
                      setForm({ ...form, ciudad: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">Barrio</label>
                  <input
                    type="text"
                    value={form.barrio}
                    onChange={(e) =>
                      setForm({ ...form, barrio: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">Precio</label>
                  <input
                    type="number"
                    value={form.precio}
                    onChange={(e) =>
                      setForm({ ...form, precio: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">m²</label>
                  <input
                    type="number"
                    value={form.metrosCuadrados}
                    onChange={(e) =>
                      setForm({ ...form, metrosCuadrados: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">Año de construcción</label>
                  <input type="number" min="1" max="9999" value={form.anioConstruccion} onChange={(e) => setForm({ ...form, anioConstruccion: e.target.value })} className="mt-1 w-full rounded-md border bg-background px-3 py-2" />
                </div>

                <label className="text-sm font-medium">Estado físico
                  <input maxLength={100} value={form.estadoFisico} onChange={(e) => setForm({ ...form, estadoFisico: e.target.value })} className="mt-1 w-full rounded-md border bg-background px-3 py-2" placeholder="Ej.: Usado" />
                </label>

                <label className="text-sm font-medium">Etiquetas (separa con coma)
                  <input maxLength={1600} value={form.etiquetas} onChange={(e) => setForm({ ...form, etiquetas: e.target.value })} className="mt-1 w-full rounded-md border bg-background px-3 py-2" placeholder="Destacado, Administración" />
                </label>

                <div>
                  <label className="text-sm font-medium">Habitaciones</label>
                  <input
                    type="number"
                    value={form.habitaciones}
                    onChange={(e) =>
                      setForm({ ...form, habitaciones: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">Baños</label>
                  <input
                    type="number"
                    value={form.banos}
                    onChange={(e) =>
                      setForm({ ...form, banos: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">Parqueaderos</label>
                  <input
                    type="number"
                    value={form.parqueaderos}
                    onChange={(e) =>
                      setForm({ ...form, parqueaderos: e.target.value })
                    }
                    className="w-full mt-1 px-3 py-2 border rounded-md bg-background"
                  />
                </div>
                <label className="text-sm font-medium">Características internas (separa con coma)
                  <input value={form.caracteristicasInternas} onChange={(e) => setForm({ ...form, caracteristicasInternas: e.target.value })} className="mt-1 w-full rounded-md border bg-background px-3 py-2" placeholder="Balcón, cocina integral" />
                </label>
                <label className="text-sm font-medium">Características externas (separa con coma)
                  <input value={form.caracteristicasExternas} onChange={(e) => setForm({ ...form, caracteristicasExternas: e.target.value })} className="mt-1 w-full rounded-md border bg-background px-3 py-2" placeholder="Piscina, portería" />
                </label>
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t">
                {editingId && editingProperty.data?.activo && (
                  <>
                    <button type="button" className="rounded border px-3 py-2 text-sm" disabled={sheetLoading} onClick={() => void downloadSheet()}>
                      Descargar ficha PDF
                    </button>
                    <a className="rounded border px-3 py-2 text-sm" href={propertyWhatsappUrl(editingProperty.data.nombre, crmPropertySlug(editingId))} target="_blank" rel="noopener noreferrer">
                      Compartir por WhatsApp
                    </a>
                  </>
                )}
                <button
                  type="button"
                  onClick={() => { setEditingId(""); setShowForm(false) }}
                  className="px-4 py-2 text-sm border rounded-md hover:bg-accent"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={createInmueble.isPending || updateInmueble.isPending || (Boolean(editingId) && (editingProperty.isLoading || editingProperty.isError))}
                  className="crm-button"
                >
                  {createInmueble.isPending && (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  )}
                  {editingId ? "Guardar cambios" : "Guardar"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {inventory.isLoading ? <div className="crm-panel p-8 text-center text-muted-foreground">Cargando inmuebles…</div> : inventory.isError ? <div role="alert" className="crm-panel p-8 text-center text-destructive">No se pudo cargar el inventario.</div> : inventory.data?.total === 0 ? (
        <div className="crm-panel border-t-4 border-t-accent py-16 text-center">
          <Home className="mx-auto mb-4 h-10 w-10 text-accent" />
          <h3 className="font-display text-2xl">{showTrash ? "Papelera vacía" : "Primer inmueble"}</h3>
          <p className="text-sm text-muted-foreground mt-1">{showTrash ? "Los inmuebles enviados a papelera aparecerán aquí." : "Registra tu primer inmueble para comenzar"}</p>
        </div>
      ) : (
        <div className="crm-panel overflow-x-auto">
          {changeInmueblesTrashState.isError && <p role="alert" className="p-3 text-sm text-destructive">No se aplicó la acción masiva. La selección puede haber cambiado.</p>}
          <div className="flex flex-wrap items-center gap-3 border-b p-3">
            <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={(inventory.data?.items.length ?? 0) > 0 && inventory.data!.items.every((item) => selectedIds.includes(item.id))} onChange={(event) => setSelectedIds(event.target.checked ? inventory.data?.items.map((item) => item.id) ?? [] : [])} /> Seleccionar página</label>
            <button className="rounded border px-3 py-2 text-sm disabled:opacity-50" disabled={!selectedIds.length || changeInmueblesTrashState.isPending} onClick={() => changeInmueblesTrashState.mutate({ ids: selectedIds, restore: showTrash }, { onSuccess: () => setSelectedIds([]) })}>
              {showTrash ? "Restaurar seleccionados" : "Enviar seleccionados a papelera"} ({selectedIds.length})
            </button>
          </div>
          <table className="w-full text-left text-sm">
            <thead><tr className="border-b text-muted-foreground"><th className="p-4"><span className="sr-only">Seleccionar</span></th><th className="p-4">Nombre</th><th className="p-4">Código Wasi</th><th className="p-4">Ubicación</th><th className="p-4">Encargado</th><th className="p-4">Precio</th><th className="p-4">Tipo</th><th className="p-4">Modalidad</th><th className="p-4">Estado</th><th className="p-4">Publicación</th><th className="p-4"><span className="sr-only">Acciones</span></th></tr></thead>
            <tbody>{inventory.data?.items.map((item) => <tr key={item.id} className="border-b last:border-0"><td className="p-4"><input type="checkbox" aria-label={`Seleccionar ${item.nombre}`} checked={selectedIds.includes(item.id)} onChange={(event) => setSelectedIds((current) => event.target.checked ? [...current, item.id] : current.filter((id) => id !== item.id))} /></td><td className="p-4 font-medium"><div>{item.nombre}</div>{Boolean(item.etiquetas?.length) && <div className="mt-1 flex flex-wrap gap-1">{item.etiquetas?.map((tag) => <span key={tag} className="rounded bg-muted px-1.5 py-0.5 text-xs font-normal">{tag}</span>)}</div>}</td><td className="p-4">{item.externalRef?.match(/^wasi:(.*?):(?:venta|arriendo|permuta|sin-precio)$/i)?.[1] ?? "—"}</td><td className="p-4">{[item.barrio, item.ciudad].filter(Boolean).join(", ")}</td><td className="p-4">{item.asesorResponsableNombre ?? "Sin asignar"}</td><td className="p-4">{item.modalidad === "Permuta" && item.precio === 0 ? "Valor por definir" : new Intl.NumberFormat("es-CO", { style: "currency", currency: item.moneda }).format(item.precio)}</td><td className="p-4">{item.tipo}</td><td className="p-4">{ETIQUETAS_MODALIDAD_INMUEBLE[item.modalidad] ?? item.modalidad}</td><td className="p-4">{item.estado}</td><td className="p-4">{item.tienePublicacion ? "Sí" : "No"}</td><td className="space-y-2 p-4">{showTrash ? <button className="crm-button" disabled={changeTrashState.isPending} onClick={() => changeTrashState.mutate({ id: item.id, restore: true }, { onSuccess: () => setSelectedIds((current) => current.filter((id) => id !== item.id)) })}>Restaurar</button> : <><button className="rounded border px-3 py-1" onClick={() => void beginEditing(item.id)}>Editar</button><button className="rounded border px-3 py-1" disabled={changeTrashState.isPending} onClick={() => changeTrashState.mutate({ id: item.id, restore: false }, { onSuccess: () => setSelectedIds((current) => current.filter((id) => id !== item.id)) })}>Enviar a papelera</button><button className="crm-button" onClick={() => setCrossProperty({ id: item.id, name: item.nombre })}>Clientes posibles{demandCounts.data ? ` (${demandCounts.data[item.id] ?? 0})` : ""}</button></>}</td></tr>)}</tbody>
          </table>
          <div className="flex items-center justify-between p-4"><span className="text-sm text-muted-foreground">{inventory.data?.total} inmuebles</span><div className="flex gap-2"><button className="crm-button" disabled={page <= 1} onClick={() => { setSelectedIds([]); setPage(page - 1) }}>Anterior</button><button className="crm-button" disabled={page * (inventory.data?.pageSize ?? 20) >= (inventory.data?.total ?? 0)} onClick={() => { setSelectedIds([]); setPage(page + 1) }}>Siguiente</button></div></div>
        </div>
      )}
      {crossProperty && <PropertyDemandsPanel propertyId={crossProperty.id} propertyName={crossProperty.name} onClose={() => setCrossProperty(null)} />}
    </div>
  )
}
