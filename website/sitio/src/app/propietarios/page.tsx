import type { Metadata } from "next";
import { ContactForm } from "@/components/ContactForm";
import { getActivePolicy } from "@/lib/api/leads";

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

export default async function PropietariosPage() {
  const policy = await getActivePolicy();

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

      <section className="space-y-5" aria-labelledby="owner-contact-title">
        <div>
          <h2 id="owner-contact-title" className="mb-2 text-2xl">Sube tu propiedad</h2>
          <p className="mb-4 max-w-prose text-muted">
            Registra tus datos y el servicio que necesitas. La solicitud entra a ENLYCE como una
            oportunidad y un asesor continúa contigo la publicación del inmueble.
          </p>
        </div>
        <ContactForm isOwnerInquiry policyVersion={policy?.version} />
      </section>
    </div>
  );
}
