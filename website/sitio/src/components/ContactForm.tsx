"use client";

import Link from "next/link";
import { useState } from "react";
import { ApiProblemError } from "@/lib/api/client";
import {
  OWNER_SERVICES,
  buildSourceWithCampaign,
  createLead,
  type CampaignParams,
  type OwnerServiceValue,
} from "@/lib/api/leads";

type Props = {
  // GUID ya resuelto por el server component; nunca el slug.
  publicationId?: string;
  propertySlug?: string;
  propertyTitle?: string;
  isOwnerInquiry: boolean;
  policyVersion?: string;
  campaign?: CampaignParams;
};

type Status =
  | { kind: "idle" }
  | { kind: "sending" }
  | { kind: "sent"; repeated: boolean }
  | { kind: "error"; messages: string[] };

const fieldClass =
  "w-full rounded border border-line bg-surface px-3 py-2 text-sm focus:border-accent";

export function ContactForm({
  publicationId,
  propertySlug,
  propertyTitle,
  isOwnerInquiry,
  policyVersion,
  campaign,
}: Props) {
  const [status, setStatus] = useState<Status>({ kind: "idle" });
  const [authorized, setAuthorized] = useState(false);
  const [service, setService] = useState<OwnerServiceValue>("vender");

  // El texto escrito no se pierde al fallar: se controla aqui y el formulario
  // nunca se desmonta entre intentos.
  const [form, setForm] = useState({ nombre: "", email: "", telefono: "" });

  const selected = OWNER_SERVICES.find((item) => item.value === service) ?? OWNER_SERVICES[0];
  const sending = status.kind === "sending";

  function buildSource(): string {
    const base = propertySlug
      ? `Website:${propertySlug}`
      : isOwnerInquiry
        ? `PropietarioWeb:${selected.label}`
        : "ContactoWeb";

    // El backend recorta si se pasa de 100: la campana cae antes que el inmueble.
    return campaign ? buildSourceWithCampaign(base, campaign) : base;
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!authorized || sending) return;

    setStatus({ kind: "sending" });

    try {
      const result = await createLead({
        nombre: form.nombre.trim(),
        email: form.email.trim(),
        telefono: form.telefono,
        fuente: buildSource(),
        tipoOperacion: isOwnerInquiry ? selected.operation : "Venta",
        ownerService: isOwnerInquiry ? selected.ownerService : null,
        publicationId: publicationId ?? null,
      });

      setStatus({ kind: "sent", repeated: result.esContactoRepetido });
    } catch (error) {
      const messages =
        error instanceof ApiProblemError
          ? error.messages
          : ["No pudimos enviar la solicitud. Revisa tu conexión e inténtalo de nuevo."];
      setStatus({ kind: "error", messages });
    }
  }

  if (status.kind === "sent") {
    return (
      <section className="rounded-sheet bg-surface p-6 shadow-card" aria-live="polite">
        <h2 className="mb-2 text-2xl">Solicitud recibida</h2>
        <p className="max-w-prose text-muted">
          {status.repeated
            ? "Ya te teníamos registrado, así que sumamos esta consulta a tu historial. Un asesor de L&C te contacta pronto."
            : "Un asesor de L&C te contacta pronto por el canal que nos dejaste."}
        </p>
        {propertyTitle && (
          <p className="mt-2 text-sm text-muted">Consulta sobre: {propertyTitle}</p>
        )}
        <Link href="/inmuebles" className="mt-4 inline-block text-accent underline">
          Seguir viendo inmuebles
        </Link>
      </section>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4 rounded-sheet bg-surface p-6 shadow-card">
      <div className="grid gap-4 sm:grid-cols-2">
        <label className="text-sm">
          <span className="mb-1 block text-muted">Nombre</span>
          <input
            className={fieldClass}
            required
            autoComplete="name"
            value={form.nombre}
            onChange={(event) => setForm({ ...form, nombre: event.target.value })}
          />
        </label>

        <label className="text-sm">
          <span className="mb-1 block text-muted">Correo electrónico</span>
          <input
            className={fieldClass}
            type="email"
            required
            autoComplete="email"
            value={form.email}
            onChange={(event) => setForm({ ...form, email: event.target.value })}
          />
        </label>

        <label className="text-sm">
          <span className="mb-1 block text-muted">Teléfono (opcional)</span>
          <input
            className={fieldClass}
            type="tel"
            autoComplete="tel"
            value={form.telefono}
            onChange={(event) => setForm({ ...form, telefono: event.target.value })}
          />
        </label>

        {isOwnerInquiry && (
          <label className="text-sm">
            <span className="mb-1 block text-muted">Qué necesitas</span>
            <select
              className={fieldClass}
              value={service}
              onChange={(event) => setService(event.target.value as OwnerServiceValue)}
            >
              {OWNER_SERVICES.map((item) => (
                <option key={item.value} value={item.value}>{item.label}</option>
              ))}
            </select>
          </label>
        )}
      </div>

      <label className="flex items-start gap-3 text-sm">
        <input
          type="checkbox"
          className="mt-1"
          checked={authorized}
          onChange={(event) => setAuthorized(event.target.checked)}
        />
        <span>
          Autorizo el tratamiento de mis datos personales conforme a la{" "}
          <Link href="/privacidad" className="text-accent underline">
            política de tratamiento de datos
          </Link>
          {policyVersion ? ` (versión ${policyVersion})` : ""} y a la Ley 1581 de 2012.
        </span>
      </label>

      {status.kind === "error" && (
        <div role="alert" className="rounded border border-accent/40 bg-accent/5 p-3 text-sm">
          <p className="mb-1 font-medium">No pudimos enviar la solicitud.</p>
          <ul className="list-inside list-disc text-muted">
            {status.messages.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </div>
      )}

      <button
        type="submit"
        disabled={!authorized || sending}
        className="rounded bg-accent px-5 py-2 text-surface hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-50"
      >
        {sending ? "Enviando…" : "Enviar solicitud"}
      </button>

      {!authorized && (
        <p className="text-sm text-muted">
          Marca la autorización para poder enviar. Sin ella no podemos registrar tu solicitud.
        </p>
      )}
    </form>
  );
}
