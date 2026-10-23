"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";

const apiUrl = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5019").replace(/\/$/, "");

type Inventory = {
  id: string;
  nombre: string;
  descripcion: string;
  tipo: string;
  modalidad: string;
  calle: string;
  ciudad: string;
  barrio: string | null;
  pais: string;
  departamento: string | null;
  precio: number;
  moneda: string;
  metrosCuadrados: number;
  habitaciones: number;
  banos: number;
  parqueaderos: number;
  caracteristicasInternas: string[];
  caracteristicasExternas: string[];
  anioConstruccion: number | null;
  estadoFisico: string | null;
  etiquetas: string[] | null;
  asesorResponsableId: string | null;
  visibleEnSitio: boolean;
};

type User = { id?: string; rol?: string };
type Props = { propertyId: string };

const propertyTypes = ["Apartamento", "Casa", "Local", "Lote", "Oficina", "Bodega", "Finca"];
const operations = ["Venta", "Arriendo", "VentaYArriendo", "Permuta"];

function listValue(value: string[] | null | undefined) {
  return value?.join(", ") ?? "";
}

function parseList(value: FormDataEntryValue | null) {
  return String(value ?? "").split(",").map((item) => item.trim()).filter(Boolean);
}

export function PropertyInventoryEditor({ propertyId }: Props) {
  const router = useRouter();
  const [inventory, setInventory] = useState<Inventory | null>(null);
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [saved, setSaved] = useState(false);
  const [revision, setRevision] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    const options: RequestInit = {
      credentials: "include",
      cache: "no-store",
      signal: controller.signal,
      headers: { Accept: "application/json" },
    };
    void fetch(`${apiUrl}/api/auth/me`, options).then(async (userResponse) => {
      if (!userResponse.ok) return;
      const user = await userResponse.json() as User;
      const response = await fetch(`${apiUrl}/api/inmuebles/${encodeURIComponent(propertyId)}`, options);
      if (!response.ok) return;
      const property = await response.json() as Inventory;
      if (user.rol === "Administrador" ||
          (user.rol === "Asesor" && user.id && user.id === property.asesorResponsableId)) {
        setInventory(property);
      }
    }).catch(() => {
      if (!controller.signal.aborted) setInventory(null);
    });
    return () => controller.abort();
  }, [propertyId]);

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!inventory) return;
    const data = new FormData(event.currentTarget);
    const internalFeatures = parseList(data.get("caracteristicasInternas"));
    const externalFeatures = parseList(data.get("caracteristicasExternas"));
    const labels = parseList(data.get("etiquetas"));
    const updated = {
      nombre: String(data.get("nombre") ?? "").trim(),
      descripcion: String(data.get("descripcion") ?? "").trim(),
      tipo: String(data.get("tipo") ?? ""),
      modalidad: String(data.get("modalidad") ?? ""),
      calle: inventory.calle,
      ciudad: inventory.ciudad,
      barrio: inventory.barrio,
      pais: inventory.pais,
      departamento: inventory.departamento,
      precio: inventory.precio,
      moneda: inventory.moneda,
      metrosCuadrados: Number(data.get("metrosCuadrados")),
      habitaciones: Number(data.get("habitaciones")),
      banos: Number(data.get("banos")),
      parqueaderos: Number(data.get("parqueaderos")),
      caracteristicasInternas: internalFeatures,
      caracteristicasExternas: externalFeatures,
      anioConstruccion: data.get("anioConstruccion")
        ? Number(data.get("anioConstruccion"))
        : null,
      estadoFisico: String(data.get("estadoFisico") ?? "").trim() || null,
      etiquetas: labels,
      asesorResponsableId: inventory.asesorResponsableId,
      visibleEnSitio: data.get("visibleEnSitio") === "on",
    };

    if ([updated.metrosCuadrados, updated.habitaciones, updated.banos, updated.parqueaderos]
      .some((value) => !Number.isInteger(value) || value < 0) ||
      !Number.isInteger(updated.metrosCuadrados) || updated.metrosCuadrados < 1) {
      setError("Revisa el área y las cantidades: deben ser números enteros válidos.");
      return;
    }
    if ([...internalFeatures, ...externalFeatures, ...labels]
      .some((value) => value.length > 50) ||
      [internalFeatures, externalFeatures, labels].some((values) => values.length > 30)) {
      setError("Cada lista admite hasta 30 elementos de 50 caracteres.");
      return;
    }

    setSaving(true);
    setError("");
    setSaved(false);
    try {
      const response = await fetch(`${apiUrl}/api/inmuebles/${encodeURIComponent(propertyId)}`, {
        method: "PUT",
        credentials: "include",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify(updated),
      });
      if (!response.ok) throw new Error(response.status === 403 || response.status === 404
        ? "No tienes permiso para editar este inmueble."
        : "No se guardaron los cambios. Revisa los campos e inténtalo otra vez.");
      setInventory({ ...inventory, ...updated });
      setRevision((value) => value + 1);
      setSaved(true);
      router.refresh();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "No se pudo guardar el inmueble.");
    } finally {
      setSaving(false);
    }
  }

  if (!inventory) return null;

  const fieldClass = "mt-1 block w-full rounded border border-line bg-white px-3 py-2 text-sm text-ink";
  return (
    <section className="rounded-sheet border border-line bg-surface p-4 shadow-card">
      <button
        type="button"
        aria-expanded={open}
        onClick={() => setOpen((value) => !value)}
        className="font-semibold text-accent-strong underline"
      >
        {open ? "Cerrar edición de datos del inmueble" : "Editar datos del inmueble"}
      </button>
      {open && <form
        key={revision}
        onSubmit={save}
        onChange={() => { setSaved(false); setError(""); }}
        className="mt-4 grid gap-4 md:grid-cols-2"
      >
        <label className="text-sm font-semibold">Nombre interno
          <input className={fieldClass} name="nombre" defaultValue={inventory.nombre} maxLength={200} required />
        </label>
        <label className="text-sm font-semibold">Tipo
          <select className={fieldClass} name="tipo" defaultValue={inventory.tipo}>
            {propertyTypes.map((type) => <option key={type} value={type}>{type}</option>)}
          </select>
        </label>
        <label className="text-sm font-semibold">Operación
          <select className={fieldClass} name="modalidad" defaultValue={inventory.modalidad}>
            {operations.map((operation) => <option key={operation} value={operation}>
              {operation === "VentaYArriendo" ? "Venta y arriendo" : operation}
            </option>)}
          </select>
        </label>
        <label className="text-sm font-semibold">Área (m²)
          <input className={fieldClass} name="metrosCuadrados" type="number" min="1" step="1" defaultValue={inventory.metrosCuadrados} required />
        </label>
        <label className="text-sm font-semibold">Habitaciones
          <input className={fieldClass} name="habitaciones" type="number" min="0" step="1" defaultValue={inventory.habitaciones} required />
        </label>
        <label className="text-sm font-semibold">Baños
          <input className={fieldClass} name="banos" type="number" min="0" step="1" defaultValue={inventory.banos} required />
        </label>
        <label className="text-sm font-semibold">Parqueaderos
          <input className={fieldClass} name="parqueaderos" type="number" min="0" step="1" defaultValue={inventory.parqueaderos} required />
        </label>
        <label className="text-sm font-semibold">Año de construcción
          <input className={fieldClass} name="anioConstruccion" type="number" min="1" max="9999" step="1" defaultValue={inventory.anioConstruccion ?? ""} />
        </label>
        <label className="text-sm font-semibold">Estado físico
          <input className={fieldClass} name="estadoFisico" defaultValue={inventory.estadoFisico ?? ""} maxLength={100} />
        </label>
        <label className="text-sm font-semibold md:col-span-2">Descripción interna
          <textarea className={fieldClass} name="descripcion" rows={4} defaultValue={inventory.descripcion} maxLength={2000} />
        </label>
        <label className="text-sm font-semibold md:col-span-2">Características internas (separadas por coma)
          <input className={fieldClass} name="caracteristicasInternas" defaultValue={listValue(inventory.caracteristicasInternas)} />
        </label>
        <label className="text-sm font-semibold md:col-span-2">Características externas (separadas por coma)
          <input className={fieldClass} name="caracteristicasExternas" defaultValue={listValue(inventory.caracteristicasExternas)} />
        </label>
        <label className="text-sm font-semibold md:col-span-2">Etiquetas (separadas por coma)
          <input className={fieldClass} name="etiquetas" defaultValue={listValue(inventory.etiquetas)} />
        </label>
        <label className="flex items-center gap-2 text-sm font-semibold md:col-span-2">
          <input name="visibleEnSitio" type="checkbox" defaultChecked={inventory.visibleEnSitio} />
          Mostrar inmueble en el sitio público
        </label>
        <p className="text-xs text-muted md:col-span-2">
          La dirección exacta, el propietario, el precio y la ubicación publicada no se muestran aquí. La ficha pública conserva su editor separado; fotos y disponibilidad se gestionan en el CRM.
        </p>
        {error && <p role="alert" className="text-sm text-red-700 md:col-span-2">{error}</p>}
        {saved && <p role="status" className="text-sm text-green-800 md:col-span-2">Datos del inmueble guardados.</p>}
        <button type="submit" disabled={saving} className="min-h-11 rounded bg-ink px-5 py-2 font-semibold text-white disabled:opacity-50 md:col-span-2">
          {saving ? "Guardando…" : "Guardar datos del inmueble"}
        </button>
      </form>}
    </section>
  );
}
