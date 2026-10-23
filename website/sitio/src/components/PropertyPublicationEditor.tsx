"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";

const apiUrl = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5019").replace(/\/$/, "");

type Publication = {
  id: string;
  advisorId: string;
  slug: string;
  publicTitle: string;
  publicDescription: string;
  priceAmount: number | null;
  priceCurrency: string | null;
  municipality: string | null;
  neighborhood: string | null;
  approximateLatitude: number | null;
  approximateLongitude: number | null;
};

type User = { id?: string; rol?: string };
type Props = { publicationId: string };

export function PropertyPublicationEditor({ publicationId }: Props) {
  const router = useRouter();
  const [publication, setPublication] = useState<Publication | null>(null);
  const [allowed, setAllowed] = useState(false);
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

    void Promise.all([
      fetch(`${apiUrl}/api/auth/me`, options),
      fetch(`${apiUrl}/api/publicaciones`, options),
    ]).then(async ([userResponse, publicationsResponse]) => {
      if (!userResponse.ok || !publicationsResponse.ok) return;
      const user = await userResponse.json() as User;
      const publications = await publicationsResponse.json() as Publication[];
      const current = publications.find((item) => item.id === publicationId);
      const canEdit = user.rol === "Administrador" ||
        (user.rol === "Asesor" && user.id === current?.advisorId);
      if (!current || !canEdit) return;
      setPublication(current);
      setAllowed(true);
    }).catch(() => {
      if (!controller.signal.aborted) setAllowed(false);
    });

    return () => controller.abort();
  }, [publicationId]);

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!publication) return;
    const data = new FormData(event.currentTarget);
    const priceAmount = Number(data.get("priceAmount"));
    const latitude = Number(data.get("latitude"));
    const longitude = Number(data.get("longitude"));
    if (!Number.isFinite(priceAmount) || priceAmount <= 0) {
      setError("Ingresa un precio mayor que cero.");
      return;
    }
    if (!Number.isFinite(latitude) || !Number.isFinite(longitude) ||
        Math.abs(latitude) > 90 || Math.abs(longitude) > 180) {
      setError("Revisa las coordenadas de la zona aproximada.");
      return;
    }

    setSaving(true);
    setError("");
    setSaved(false);
    try {
      const update = {
        slug: String(data.get("slug") ?? "").trim(),
        publicTitle: String(data.get("publicTitle") ?? "").trim(),
        publicDescription: String(data.get("publicDescription") ?? "").trim(),
        priceAmount,
        priceCurrency: String(data.get("priceCurrency") ?? "COP").trim().toUpperCase(),
        municipality: String(data.get("municipality") ?? "").trim(),
        neighborhood: String(data.get("neighborhood") ?? "").trim(),
        approximateLatitude: latitude,
        approximateLongitude: longitude,
      };
      const response = await fetch(`${apiUrl}/api/publicaciones/${publication.id}`, {
        method: "PUT",
        credentials: "include",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify(update),
      });
      if (!response.ok) throw new Error(response.status === 403
        ? "No tienes permiso para editar esta publicación."
        : "No se guardaron los cambios. Revisa los campos e inténtalo otra vez.");
      const savedPublication = await response.json() as { slug?: string };
      const savedSlug = savedPublication.slug || publication.slug;
      setPublication({ ...publication, ...update, slug: savedSlug });
      setRevision((value) => value + 1);
      setSaved(true);
      if (savedSlug !== publication.slug) {
        router.replace(`/inmuebles/${encodeURIComponent(savedSlug)}`);
      } else {
        router.refresh();
      }
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "No se pudo guardar la publicación.");
    } finally {
      setSaving(false);
    }
  }

  if (!allowed || !publication) return null;

  const fieldClass = "mt-1 block w-full rounded border border-line bg-white px-3 py-2 text-sm text-ink";
  return (
    <section className="rounded-sheet border border-line bg-surface p-4 shadow-card">
      <button
        type="button"
        aria-expanded={open}
        onClick={() => setOpen((value) => !value)}
        className="font-semibold text-accent-strong underline"
      >
        {open ? "Cerrar edición de ficha pública" : "Editar ficha pública desde la web"}
      </button>
      {open && <form key={revision} onSubmit={save} onChange={() => { setSaved(false); setError(""); }} className="mt-4 grid gap-4 md:grid-cols-2">
        <label className="text-sm font-semibold">Título público
          <input className={fieldClass} name="publicTitle" defaultValue={publication.publicTitle} maxLength={200} required />
        </label>
        <label className="text-sm font-semibold">Slug de la ficha
          <input className={fieldClass} name="slug" defaultValue={publication.slug} maxLength={200} required />
        </label>
        <label className="text-sm font-semibold md:col-span-2">Descripción pública
          <textarea className={fieldClass} name="publicDescription" defaultValue={publication.publicDescription} rows={5} maxLength={4000} />
        </label>
        <label className="text-sm font-semibold">Precio
          <input className={fieldClass} name="priceAmount" type="number" min="1" step="1" defaultValue={publication.priceAmount ?? ""} required />
        </label>
        <label className="text-sm font-semibold">Moneda (código ISO)
          <input className={fieldClass} name="priceCurrency" defaultValue={publication.priceCurrency ?? "COP"} maxLength={3} minLength={3} required />
        </label>
        <label className="text-sm font-semibold">Municipio
          <input className={fieldClass} name="municipality" defaultValue={publication.municipality ?? ""} maxLength={100} required />
        </label>
        <label className="text-sm font-semibold">Barrio o sector
          <input className={fieldClass} name="neighborhood" defaultValue={publication.neighborhood ?? ""} maxLength={100} required />
        </label>
        <label className="text-sm font-semibold">Latitud aproximada
          <input className={fieldClass} name="latitude" type="number" step="any" defaultValue={publication.approximateLatitude ?? ""} required />
        </label>
        <label className="text-sm font-semibold">Longitud aproximada
          <input className={fieldClass} name="longitude" type="number" step="any" defaultValue={publication.approximateLongitude ?? ""} required />
        </label>
        <p className="text-xs text-muted md:col-span-2">Edita solo la información pública. Los datos comerciales y privados del inmueble se cambian en el CRM.</p>
        {error && <p role="alert" className="text-sm text-red-700 md:col-span-2">{error}</p>}
        {saved && <p role="status" className="text-sm text-green-800 md:col-span-2">Cambios guardados. La ficha se actualiza al instante; el listado puede tardar hasta cinco minutos.</p>}
        <button type="submit" disabled={saving} className="min-h-11 rounded bg-ink px-5 py-2 font-semibold text-white disabled:opacity-50 md:col-span-2">
          {saving ? "Guardando…" : "Guardar ficha pública"}
        </button>
      </form>}
    </section>
  );
}
