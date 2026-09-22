import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Contacto",
  description: "Habla con un asesor de L&C Propiedad Raíz sobre compra, arriendo o publicación de tu inmueble.",
};

type PageProps = { searchParams: Promise<Record<string, string | string[] | undefined>> };

function first(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

export default async function ContactoPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const property = first(params.inmueble);
  const reason = first(params.motivo);

  return (
    <div className="mx-auto max-w-3xl space-y-8 px-4 py-10">
      <header className="space-y-2">
        <h1 className="text-4xl">Contacto</h1>
        <p className="text-muted">
          {reason === "propietario"
            ? "Quieres publicar o avaluar tu inmueble."
            : "Escríbenos y te responde un asesor, no un robot."}
        </p>
      </header>

      {property && (
        <p className="rounded-sheet bg-surface p-4 text-sm shadow-card">
          Consulta sobre el inmueble{" "}
          <Link href={`/inmuebles/${property}`} className="text-accent underline">
            {property}
          </Link>
        </p>
      )}

      {/* El formulario que crea el lead en el CRM llega en el bloque 6 del plan. */}
      <section className="rounded-sheet border border-line p-6">
        <h2 className="mb-2 text-2xl">Formulario en construcción</h2>
        <p className="max-w-prose text-muted">
          El envío de solicitudes al CRM, con la autorización de tratamiento de datos de la Ley
          1581, es el siguiente paso del plan. Mientras tanto, el laboratorio en{" "}
          <code className="rounded bg-surface px-1">website/funcional</code> ya crea leads reales.
        </p>
      </section>
    </div>
  );
}
