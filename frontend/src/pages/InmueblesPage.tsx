import { useState } from "react"
import { useCreateInmueble } from "@/hooks/useApi"
import { TIPOS_INMUEBLE, MODALIDADES_INMUEBLE } from "@/lib/constants"
import {
  Home,
  Plus,
  X,
  Loader2,
} from "lucide-react"

export function InmueblesPage() {
  const [showForm, setShowForm] = useState(false)
  const createInmueble = useCreateInmueble()

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
    propietarioId: "",
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    createInmueble.mutate(
      {
        ...form,
        precio: parseFloat(form.precio) || 0,
        metrosCuadrados: parseFloat(form.metrosCuadrados) || 0,
        habitaciones: parseInt(form.habitaciones) || 0,
        banos: parseInt(form.banos) || 0,
        parqueaderos: parseInt(form.parqueaderos) || 0,
      },
      {
        onSuccess: () => {
          setShowForm(false)
          setForm({
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
            propietarioId: "",
          })
        },
      }
    )
  }

  return (
    <div className="space-y-7">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <span className="crm-eyebrow">Inventario</span>
          <h1 className="crm-page-title mt-2">Inmuebles</h1>
          <p className="mt-3 text-sm text-muted-foreground">
            Catálogo de propiedades
          </p>
        </div>
        <button
          onClick={() => setShowForm(true)}
          className="crm-button"
        >
          <Plus className="h-4 w-4" />
          Nuevo Inmueble
        </button>
      </div>

      {/* Form modal */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/55 p-4">
          <div className="crm-panel max-h-[90vh] w-full max-w-2xl overflow-y-auto border-t-4 border-t-accent">
            <div className="flex items-center justify-between border-b border-border p-5">
              <h2 className="font-display text-2xl text-foreground">
                Nuevo Inmueble
              </h2>
              <button
                onClick={() => setShowForm(false)}
                className="rounded-sm p-2 hover:bg-muted"
                aria-label="Cerrar formulario"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5 p-5 md:p-7">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
                        {m}
                      </option>
                    ))}
                  </select>
                </div>

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
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t">
                <button
                  type="button"
                  onClick={() => setShowForm(false)}
                  className="px-4 py-2 text-sm border rounded-md hover:bg-accent"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={createInmueble.isPending}
                  className="crm-button"
                >
                  {createInmueble.isPending && (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  )}
                  Guardar
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Empty state */}
      <div className="crm-panel border-t-4 border-t-accent py-16 text-center">
        <Home className="mx-auto mb-4 h-10 w-10 text-accent" />
        <h3 className="font-display text-2xl">Primer inmueble</h3>
        <p className="text-sm text-muted-foreground mt-1">
          Registra tu primer inmueble para comenzar
        </p>
      </div>
    </div>
  )
}
