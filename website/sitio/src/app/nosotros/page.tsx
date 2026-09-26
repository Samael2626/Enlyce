import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Quiénes somos",
  description:
    "Conoce el propósito, los servicios y la forma de trabajar de L&C Propiedad Raíz en Medellín.",
};

const values = [
  "Honestidad",
  "Confianza",
  "Responsabilidad",
  "Ética",
  "Excelencia",
  "Calidez humana",
];

const services = [
  "Arrendamiento de inmuebles",
  "Administración de inmuebles",
  "Compra y venta",
  "Gestión jurídica y documental",
  "Acompañamiento a propietarios e inversionistas",
];

export default function NosotrosPage() {
  return (
    <div className="about-page">
      <section className="about-hero">
        <div>
          <p className="section-kicker">L&amp;C Propiedad Raíz · Medellín</p>
          <h1>Tu patrimonio merece dirección.</h1>
        </div>
        <p>
          Acompañamos decisiones de compra, venta, arrendamiento y administración con
          criterio profesional, trato cercano y conocimiento del mercado local.
        </p>
      </section>

      <section className="about-purpose" aria-labelledby="purpose-title">
        <div>
          <p className="section-kicker">Nuestro propósito</p>
          <h2 id="purpose-title">Relaciones inmobiliarias que duran.</h2>
        </div>
        <div className="about-copy">
          <p>
            Protegemos, gestionamos y proyectamos el patrimonio de nuestros clientes con
            procesos claros y acompañamiento humano.
          </p>
          <p>
            Trabajamos para que cada inmueble tenga una estrategia comercial real, no una
            publicación abandonada en un portal.
          </p>
        </div>
      </section>

      <section className="about-values" aria-labelledby="values-title">
        <p className="section-kicker">Cómo trabajamos</p>
        <h2 id="values-title">Criterio antes que ruido.</h2>
        <ul>
          {values.map((value, index) => (
            <li key={value}><span>{String(index + 1).padStart(2, "0")}</span>{value}</li>
          ))}
        </ul>
      </section>

      <section id="servicios" className="about-services" aria-labelledby="services-title">
        <div>
          <p className="section-kicker">Servicio integral</p>
          <h2 id="services-title">Una visión completa del inmueble.</h2>
        </div>
        <ol>
          {services.map((service, index) => (
            <li key={service}><span>{String(index + 1).padStart(2, "0")}</span>{service}</li>
          ))}
        </ol>
      </section>

      <section className="about-cta">
        <div>
          <p className="section-kicker">Conversemos</p>
          <h2>¿Qué necesitas hacer con tu propiedad?</h2>
        </div>
        <div className="flex flex-wrap gap-3">
          <Link href="/propietarios" className="about-cta-primary">Soy propietario</Link>
          <Link href="/inmuebles" className="about-cta-secondary">Ver inmuebles</Link>
        </div>
      </section>
    </div>
  );
}
