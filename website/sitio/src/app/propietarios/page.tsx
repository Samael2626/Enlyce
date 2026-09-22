import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Publica tu inmueble con L&C",
  description:
    "Venta, arriendo, administración o avalúo de tu inmueble en Medellín y el Valle de Aburrá, con acompañamiento de un asesor de L&C.",
};

const services = [
  { title: "Vender", body: "Avalúo comercial, fotografía, publicación y filtro de compradores reales." },
  { title: "Arrendar", body: "Estudio del arrendatario, póliza y contrato conforme a la normativa vigente." },
  { title: "Administrar", body: "Recaudo del canon, mantenimiento y reporte mensual al propietario." },
  { title: "Avaluar", body: "Precio sustentado con comparables de la zona, sin compromiso de venta." },
];

export default function PropietariosPage() {
  return (
    <div className="mx-auto max-w-5xl space-y-10 px-4 py-10">
      <header className="space-y-3">
        <h1 className="text-4xl">Tu inmueble, en manos de alguien que responde</h1>
        <p className="max-w-prose text-lg text-muted">
          Trabajamos con inventario propio y pocos inmuebles a la vez. Cada propietario tiene un
          asesor con nombre, no un formulario que se pierde.
        </p>
      </header>

      <ul className="grid gap-4 sm:grid-cols-2">
        {services.map((service) => (
          <li key={service.title} className="rounded-sheet bg-surface p-5 shadow-card">
            <h2 className="text-xl">{service.title}</h2>
            <p className="mt-1 text-sm text-muted">{service.body}</p>
          </li>
        ))}
      </ul>

      <section className="rounded-sheet bg-surface p-6 shadow-card">
        <h2 className="mb-2 text-2xl">Hablemos</h2>
        <p className="mb-4 max-w-prose text-muted">
          Cuéntanos qué inmueble tienes y qué necesitas. Te respondemos con un diagnóstico
          honesto, aunque la respuesta sea que todavía no es el momento de vender.
        </p>
        <Link
          href="/contacto?motivo=propietario"
          className="inline-block rounded bg-accent px-5 py-2 text-surface hover:bg-accent-strong"
        >
          Contactar a un asesor
        </Link>
      </section>
    </div>
  );
}
