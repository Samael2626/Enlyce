"use client";

import Link from "next/link";
import { useState } from "react";
import { ApiProblemError } from "@/lib/api/client";
import {
  CONTACT_CHANNELS,
  OWNER_SERVICES,
  OWNER_PROPERTY_TYPES,
  buildSourceWithCampaign,
  createLead,
  enrichOwnerInquiry,
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
  initialOwnerService?: OwnerServiceValue;
};

type Status =
  | { kind: "idle" }
  | { kind: "sending" }
  | { kind: "captured"; leadId: string; repeated: boolean }
  | { kind: "error"; messages: string[] };

type DetailsStatus =
  | { kind: "idle" }
  | { kind: "sending" }
  | { kind: "sent" }
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
  initialOwnerService,
}: Props) {
  const [status, setStatus] = useState<Status>({ kind: "idle" });
  const [authorized, setAuthorized] = useState(false);
  const [service, setService] = useState<OwnerServiceValue>(initialOwnerService ?? "vender");
  const [detailsStatus, setDetailsStatus] = useState<DetailsStatus>({ kind: "idle" });

  // El texto escrito no se pierde al fallar: se controla aqui y el formulario
  // nunca se desmonta entre intentos.
  const [form, setForm] = useState({ nombre: "", email: "", telefono: "" });
  const [ownerDetails, setOwnerDetails] = useState({
    propertyType: "Apartment",
    city: "Medellín",
    neighborhood: "",
    expectedPrice: "",
    message: "",
    preferredContactChannel: "WhatsApp",
  });

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

      setStatus({
        kind: "captured",
        leadId: result.id,
        repeated: result.esContactoRepetido,
      });
    } catch (error) {
      const messages =
        error instanceof ApiProblemError
          ? error.messages
          : ["No pudimos enviar la solicitud. Revisa tu conexión e inténtalo de nuevo."];
      setStatus({ kind: "error", messages });
    }
  }

  async function handleDetailsSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (status.kind !== "captured" || detailsStatus.kind === "sending") return;

    setDetailsStatus({ kind: "sending" });

    try {
      await enrichOwnerInquiry(status.leadId, {
        email: form.email.trim(),
        propertyType: ownerDetails.propertyType,
        city: ownerDetails.city.trim(),
        neighborhood: ownerDetails.neighborhood.trim() || undefined,
        expectedPrice: ownerDetails.expectedPrice
          ? Number(ownerDetails.expectedPrice)
          : undefined,
        message: ownerDetails.message.trim() || undefined,
        preferredContactChannel: ownerDetails.preferredContactChannel,
      });
      setDetailsStatus({ kind: "sent" });
    } catch (error) {
      const messages =
        error instanceof ApiProblemError
          ? error.messages
          : ["No pudimos guardar los datos del inmueble. Inténtalo de nuevo."];
      setDetailsStatus({ kind: "error", messages });
    }
  }

  if (status.kind === "captured" && isOwnerInquiry) {
    return (
      <section className="contact-form-success owner-capture-progress" aria-live="polite">
        <header className="owner-capture-confirmation">
          <p className="section-kicker">Paso 1 guardado</p>
          <h2>Ya podemos contactarte.</h2>
          <p>
            {status.repeated
              ? "Reconocimos tu correo y añadimos esta solicitud a tu historial."
              : "Tu oportunidad ya está en ENLYCE."}{" "}
            Puedes cerrar esta página o añadir contexto para que el asesor llegue mejor preparado.
          </p>
        </header>

        {detailsStatus.kind === "sent" ? (
          <div className="owner-details-complete">
            <p className="section-kicker">Información completa</p>
            <h3>El asesor ya tiene el contexto del inmueble.</h3>
            <p>Te contactaremos por {CONTACT_CHANNELS.find((item) => item.value === ownerDetails.preferredContactChannel)?.label.toLocaleLowerCase("es-CO")}.</p>
          </div>
        ) : (
          <form onSubmit={handleDetailsSubmit} className="owner-details-form">
            <div className="owner-details-heading">
              <span>02</span>
              <div>
                <h3>Cuéntanos sobre el inmueble</h3>
                <p>Opcional. Completarlo toma menos de un minuto.</p>
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <label className="text-sm">
                <span className="mb-1 block text-muted">Tipo de inmueble</span>
                <select
                  className={fieldClass}
                  value={ownerDetails.propertyType}
                  onChange={(event) => setOwnerDetails({ ...ownerDetails, propertyType: event.target.value })}
                >
                  {OWNER_PROPERTY_TYPES.map((item) => (
                    <option key={item.value} value={item.value}>{item.label}</option>
                  ))}
                </select>
              </label>

              <label className="text-sm">
                <span className="mb-1 block text-muted">Ciudad</span>
                <input
                  className={fieldClass}
                  required
                  maxLength={100}
                  value={ownerDetails.city}
                  onChange={(event) => setOwnerDetails({ ...ownerDetails, city: event.target.value })}
                />
              </label>

              <label className="text-sm">
                <span className="mb-1 block text-muted">Barrio o sector (opcional)</span>
                <input
                  className={fieldClass}
                  maxLength={100}
                  placeholder="Laureles, El Poblado…"
                  value={ownerDetails.neighborhood}
                  onChange={(event) => setOwnerDetails({ ...ownerDetails, neighborhood: event.target.value })}
                />
              </label>

              <label className="text-sm">
                <span className="mb-1 block text-muted">Precio esperado (opcional)</span>
                <input
                  className={fieldClass}
                  type="number"
                  min="1"
                  step="100000"
                  inputMode="numeric"
                  placeholder="450000000"
                  value={ownerDetails.expectedPrice}
                  onChange={(event) => setOwnerDetails({ ...ownerDetails, expectedPrice: event.target.value })}
                />
              </label>

              <label className="text-sm sm:col-span-2">
                <span className="mb-1 block text-muted">¿Cómo prefieres que te contactemos?</span>
                <select
                  className={fieldClass}
                  value={ownerDetails.preferredContactChannel}
                  onChange={(event) => setOwnerDetails({ ...ownerDetails, preferredContactChannel: event.target.value })}
                >
                  {CONTACT_CHANNELS.map((item) => (
                    <option key={item.value} value={item.value}>{item.label}</option>
                  ))}
                </select>
              </label>

              <label className="text-sm sm:col-span-2">
                <span className="mb-1 block text-muted">Algo que debamos saber (opcional)</span>
                <textarea
                  className={`${fieldClass} min-h-28 resize-y`}
                  maxLength={2000}
                  placeholder="Estado del inmueble, tiempos, características importantes…"
                  value={ownerDetails.message}
                  onChange={(event) => setOwnerDetails({ ...ownerDetails, message: event.target.value })}
                />
              </label>
            </div>

            {detailsStatus.kind === "error" && (
              <div role="alert" className="rounded border border-accent/40 bg-accent/5 p-3 text-sm">
                <p className="mb-1 font-medium">No pudimos guardar estos datos.</p>
                <ul className="list-inside list-disc text-muted">
                  {detailsStatus.messages.map((message) => <li key={message}>{message}</li>)}
                </ul>
              </div>
            )}

            <button type="submit" disabled={detailsStatus.kind === "sending"}>
              {detailsStatus.kind === "sending" ? "Guardando…" : "Guardar datos del inmueble"}
            </button>
          </form>
        )}
      </section>
    );
  }

  if (status.kind === "captured") {
    return (
      <section className="contact-form-success" aria-live="polite">
        <p className="section-kicker">Registro confirmado</p>
        <h2>Solicitud recibida</h2>
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
    <form
      onSubmit={handleSubmit}
      className={isOwnerInquiry ? "contact-form contact-form-owner" : "contact-form"}
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <label className="text-sm">
          <span className="mb-1 block text-muted">Nombre</span>
          <input
            className={fieldClass}
            required
            autoComplete="name"
            placeholder="Nombre y apellido"
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
            placeholder="nombre@correo.com"
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
            placeholder="300 000 0000"
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
        {sending ? "Enviando…" : isOwnerInquiry ? "Guardar y continuar" : "Enviar solicitud"}
      </button>

      {!authorized && (
        <p className="text-sm text-muted">
          Marca la autorización para poder enviar. Sin ella no podemos registrar tu solicitud.
        </p>
      )}
    </form>
  );
}
