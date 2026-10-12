"use client";

import Link from "next/link";
import { useState } from "react";
import { ApiProblemError } from "@/lib/api/client";
import { trackFunnelEvent } from "@/lib/analytics";
import { createLead } from "@/lib/api/leads";

type Props = { query: string; summary: string; operation?: string };

type Status = { kind: "idle" } | { kind: "sending" } | { kind: "done" } | { kind: "error"; message: string };

// La alerta entra al CRM como lead: la ruta guarda los criterios exactos y el
// asesor los ve al abrir la oportunidad.
export function SearchAlertForm({ query, summary, operation }: Props) {
  const [open, setOpen] = useState(false);
  const [status, setStatus] = useState<Status>({ kind: "idle" });

  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setStatus({ kind: "sending" });
    try {
      await createLead({
        nombre: String(data.get("nombre") ?? "").trim(),
        email: String(data.get("email") ?? "").trim(),
        telefono: String(data.get("telefono") ?? ""),
        fuente: "alerta_busqueda",
        tipoOperacion: operation === "Arriendo" ? "Arriendo" : "Venta",
        sourceRoute: `/inmuebles${query}`.slice(0, 500),
      });
      trackFunnelEvent("conversion");
      setStatus({ kind: "done" });
    } catch (error) {
      const message = error instanceof ApiProblemError
        ? error.messages.join(" ")
        : "No pudimos guardar la alerta. Intenta de nuevo o escríbenos por WhatsApp.";
      setStatus({ kind: "error", message });
    }
  };

  if (status.kind === "done") {
    return (
      <div className="search-alert" role="status">
        <strong>Alerta creada.</strong>
        <p>Un asesor te contactará cuando entre un inmueble que encaje con: {summary}.</p>
      </div>
    );
  }

  return (
    <div className="search-alert">
      <div className="search-alert-head">
        <div>
          <strong>¿No encuentras lo que buscas?</strong>
          <p>Te avisamos cuando entre algo así: {summary}.</p>
        </div>
        {!open && <button type="button" onClick={() => setOpen(true)}>Crear alerta</button>}
      </div>
      {open && (
        <form onSubmit={submit} className="search-alert-form">
          <label><span>Nombre</span><input name="nombre" required maxLength={200} autoComplete="name" /></label>
          <label><span>Correo</span><input name="email" type="email" required maxLength={320} autoComplete="email" /></label>
          <label><span>WhatsApp (opcional)</span><input name="telefono" type="tel" maxLength={20} autoComplete="tel" /></label>
          <label className="search-alert-consent">
            <input type="checkbox" required />
            <span>
              Autorizo el tratamiento de mis datos según la{" "}
              <Link href="/privacidad" target="_blank">política de privacidad</Link>.
            </span>
          </label>
          {status.kind === "error" && <p role="alert" className="search-alert-error">{status.message}</p>}
          <button type="submit" disabled={status.kind === "sending"}>
            {status.kind === "sending" ? "Guardando…" : "Avisarme"}
          </button>
        </form>
      )}
    </div>
  );
}
