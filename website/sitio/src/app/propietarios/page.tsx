import type { Metadata } from "next";
import Link from "next/link";
import { ContactForm } from "@/components/ContactForm";
import {
  findOwnerService,
  getActivePolicy,
  type OwnerServiceValue,
} from "@/lib/api/leads";

export const metadata: Metadata = {
  title: "Publica tu inmueble con L&C",
  description:
    "Venta, arriendo, administración o valoración de inmuebles en Medellín y el Valle de Aburrá con acompañamiento de L&C.",
};

const services = [
  {
    value: "vender",
    title: "Vender",
    body: "Definimos una salida al mercado con precio sustentado, presentación cuidada y filtro de compradores.",
  },
  {
    value: "arrendar",
    title: "Arrendar",
    body: "Buscamos un arrendatario viable y acompañamos el estudio, la póliza y la formalización.",
  },
  {
    value: "administrar",
    title: "Administrar",
    body: "Centralizamos el recaudo, las novedades del inmueble y el seguimiento que necesita el propietario.",
  },
  {
    value: "avaluar",
    title: "Valorar",
    body: "Revisamos ubicación, características y referencias del sector para orientar la decisión comercial.",
  },
] as const;

const process = [
  ["01", "Conocemos el inmueble", "Nos cuentas dónde está, qué necesitas y cómo podemos contactarte."],
  ["02", "Trazamos la estrategia", "Un asesor revisa el caso y define contigo el servicio y los siguientes pasos."],
  ["03", "Preparamos la salida", "Solo después de validar la información se crea la ficha y se autoriza su publicación."],
] as const;

type PageProps = {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

function first(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

export default async function PropietariosPage({ searchParams }: PageProps) {
  const [policy, params] = await Promise.all([getActivePolicy(), searchParams]);
  const initialService = findOwnerService(first(params.servicio))?.value as
    | OwnerServiceValue
    | undefined;

  return (
    <div className="owners-page">
      <section className="owners-hero" aria-labelledby="owners-title">
        <div className="owners-hero-copy">
          <p className="section-kicker">Para propietarios</p>
          <h1 id="owners-title">Tu propiedad, bien representada.</h1>
          <p>
            Vender, arrendar o administrar empieza por entender bien el activo. En L&amp;C
            estudiamos tu caso y lo convertimos en una oportunidad trazable dentro de ENLYCE.
          </p>
          <div className="owners-hero-actions">
            <Link href="#solicitud" className="owners-primary-action">
              Quiero hablar de mi inmueble
            </Link>
            <Link href="#servicios" className="owners-secondary-action">
              Ver servicios
            </Link>
          </div>
          <dl className="owners-proof">
            <div>
              <dt>Respuesta humana</dt>
              <dd>Un asesor continúa tu solicitud.</dd>
            </div>
            <div>
              <dt>Decisión informada</dt>
              <dd>Revisamos antes de publicar.</dd>
            </div>
            <div>
              <dt>Datos protegidos</dt>
              <dd>Consentimiento y trazabilidad.</dd>
            </div>
          </dl>
        </div>

        <div className="owners-hero-visual" role="img" aria-label="Vista urbana de Medellín">
          <blockquote>
            <span>Conocimiento local</span>
            Medellín y Valle de Aburrá
          </blockquote>
        </div>
      </section>

      <section id="servicios" className="owners-services" aria-labelledby="services-title">
        <header>
          <p className="section-kicker">Una decisión, cuatro caminos</p>
          <h2 id="services-title">¿Qué quieres hacer con tu propiedad?</h2>
          <p>
            Elige el punto de partida. No estás contratando todavía: estás abriendo una
            conversación para evaluar el inmueble y la ruta adecuada.
          </p>
        </header>

        <ol>
          {services.map((service, index) => (
            <li key={service.value}>
              <span aria-hidden="true">0{index + 1}</span>
              <div>
                <h3>{service.title}</h3>
                <p>{service.body}</p>
              </div>
              <Link href={`/propietarios?servicio=${service.value}#solicitud`}>
                Elegir {service.title.toLocaleLowerCase("es-CO")}
              </Link>
            </li>
          ))}
        </ol>
      </section>

      <section className="owners-process" aria-labelledby="process-title">
        <div className="owners-process-heading">
          <p className="section-kicker">Así empieza</p>
          <h2 id="process-title">Del primer mensaje a una propiedad lista para salir.</h2>
        </div>
        <ol>
          {process.map(([number, title, body]) => (
            <li key={number}>
              <span>{number}</span>
              <h3>{title}</h3>
              <p>{body}</p>
            </li>
          ))}
        </ol>
      </section>

      <section id="solicitud" className="owners-contact" aria-labelledby="owner-contact-title">
        <div className="owners-contact-intro">
          <p className="section-kicker">Primer contacto</p>
          <h2 id="owner-contact-title">Cuéntanos qué tienes en mente.</h2>
          <p className="owners-contact-note">
            Este formulario no publica automáticamente tu dirección ni compromete el inmueble.
          </p>
        </div>
        <ContactForm
          isOwnerInquiry
          initialOwnerService={initialService}
          policyVersion={policy?.version}
        />
      </section>
    </div>
  );
}
