import type { Metadata } from "next";
import Link from "next/link";
import { ContactForm } from "@/components/ContactForm";
import { PropertySearchPanel } from "@/components/PropertySearchPanel";
import { getPropertyBySlug } from "@/lib/api/catalog";
import { findOwnerService, getActivePolicy, type CampaignParams } from "@/lib/api/leads";

export const metadata: Metadata = {
  title: "Contacto",
  description:
    "Habla con un asesor de L&C Propiedad Raíz sobre compra, arriendo o publicación de tu inmueble.",
};

type PageProps = { searchParams: Promise<Record<string, string | string[] | undefined>> };

function first(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

export default async function ContactoPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const slug = first(params.inmueble);
  const reason = first(params.motivo);
  const isOwnerInquiry = reason === "propietario" || findOwnerService(first(params.servicio)) !== undefined;

  // El enlace de la ficha trae el slug, pero el CRM espera el GUID de la
  // publicacion. Se resuelve aqui; si el slug no existe se ignora y el
  // formulario sigue funcionando como contacto general.
  const property = slug ? await getPropertyBySlug(slug) : null;
  const policy = await getActivePolicy();

  // Campana: mismo patron que ?inmueble= y ?motivo=. No se guarda como entidad
  // propia todavia, solo se anexa a la fuente del lead.
  const campaign: CampaignParams = {
    source: first(params.utm_source),
    medium: first(params.utm_medium),
    campaign: first(params.utm_campaign),
  };

  return (
    <div className="mx-auto max-w-[82rem] space-y-10 px-4 py-10">
      <header className="max-w-3xl space-y-2">
        <h1 className="text-4xl">Contacto</h1>
        <p className="text-muted">
          {isOwnerInquiry
            ? "Cuéntanos qué inmueble tienes y qué necesitas."
            : "Escríbenos y te responde un asesor, no un robot."}
        </p>
      </header>

      <PropertySearchPanel />

      <div className="max-w-3xl space-y-8">

      {property ? (
        <p className="rounded-sheet bg-surface p-4 text-sm shadow-card">
          Consulta sobre{" "}
          <Link href={`/inmuebles/${property.slug}`} className="text-accent underline">
            {property.publicTitle}
          </Link>{" "}
          — {property.location.neighborhood}, {property.location.municipality}
        </p>
      ) : (
        slug && (
          <p className="rounded-sheet border border-line p-4 text-sm text-muted">
            El inmueble que buscabas ya no está publicado. Puedes escribirnos igual y te
            proponemos alternativas.
          </p>
        )
      )}

      <ContactForm
        publicationId={property?.id}
        propertySlug={property?.slug}
        propertyTitle={property?.publicTitle}
        isOwnerInquiry={isOwnerInquiry}
        policyVersion={policy?.version}
        campaign={campaign}
      />
      </div>
    </div>
  );
}
