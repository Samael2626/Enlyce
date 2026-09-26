import type { components } from "./schema";
import { apiGetOrNull, apiPost, apiPut } from "./client";

type Schemas = components["schemas"];

export type ActivePolicy = {
  version: string;
  fechaVigencia: string;
};

export type LeadResult = {
  id: string;
  esContactoRepetido: boolean;
};

// Servicios que ofrece L&C al propietario. Todos viajan estructurados; la
// fuente conserva atribucion comercial, nunca semantica de dominio.
export const OWNER_SERVICES = [
  { value: "vender", label: "Vender", ownerService: "Sell", operation: "Venta" },
  { value: "arrendar", label: "Arrendar", ownerService: "Rent", operation: "Arriendo" },
  { value: "administrar", label: "Administrar", ownerService: "Manage", operation: "Arriendo" },
  { value: "avaluar", label: "Avaluar", ownerService: "Valuation", operation: "Venta" },
] as const;

export type OwnerServiceValue = (typeof OWNER_SERVICES)[number]["value"];

export const OWNER_PROPERTY_TYPES = [
  { value: "Apartment", label: "Apartamento" },
  { value: "House", label: "Casa" },
  { value: "CommercialSpace", label: "Local comercial" },
  { value: "Office", label: "Oficina" },
  { value: "Lot", label: "Lote" },
  { value: "CountryHouse", label: "Casa campestre" },
  { value: "Other", label: "Otro" },
] as const;

export const CONTACT_CHANNELS = [
  { value: "WhatsApp", label: "WhatsApp" },
  { value: "Phone", label: "Llamada" },
  { value: "Email", label: "Correo electrónico" },
] as const;

export function findOwnerService(value: string | undefined) {
  return OWNER_SERVICES.find((service) => service.value === value);
}

export async function getActivePolicy(): Promise<ActivePolicy | null> {
  const policy = await apiGetOrNull<Schemas["ObtenerPoliticaActivaResponse"]>(
    "/api/politica/activa",
    { revalidate: 3600 },
  );

  if (!policy?.version || !policy.fechaVigencia) return null;
  return { version: policy.version, fechaVigencia: policy.fechaVigencia };
}

// Canal declarado por este frontend. El backend audita lo que llegue y cae a
// "desconocido" si no llega nada; no adivina el origen.
export const SITE_CHANNEL = "sitio_web";

export const UTM_KEYS = ["utm_source", "utm_medium", "utm_campaign"] as const;

// Mismo separador que ya usa BuildSource en CreateLeadHandler para el marcador
// PublicationReview:, para no inventar un segundo formato en la misma columna.
export type CampaignParams = { source?: string; medium?: string; campaign?: string };

export function buildSourceWithCampaign(base: string, utm: CampaignParams): string {
  // Se limpian los separadores del propio formato para que la fuente siga
  // siendo parseable aunque la campana venga con barras o pipes.
  const parts = [utm.source, utm.medium, utm.campaign].map(
    (part) => part?.trim().replace(/[|/]/g, "-") ?? "",
  );

  while (parts.length > 0 && parts[parts.length - 1] === "") parts.pop();
  if (parts.length === 0) return base;

  return `${base}|utm=${parts.join("/")}`;
}

export type CreateLeadInput = {
  nombre: string;
  email: string;
  telefono?: string;
  fuente: string;
  tipoOperacion: string;
  ownerService?: string | null;
  publicationId?: string | null;
};

export async function createLead(input: CreateLeadInput): Promise<LeadResult> {
  const body: Schemas["CreateLeadRequest"] = {
    nombre: input.nombre,
    email: input.email,
    telefono: input.telefono?.trim() ? input.telefono.trim() : null,
    fuente: input.fuente,
    // El dominio exige autorizacion para crear el lead; el formulario no deja
    // enviar sin marcarla, asi que aqui siempre viaja en true.
    autorizacionDatos: true,
    tipoOperacion: input.tipoOperacion,
    ownerService: input.ownerService ?? null,
    publicationId: input.publicationId ?? null,
    canal: SITE_CHANNEL,
  };

  const result = await apiPost<Schemas["CreateLeadRequest"], Schemas["CreateLeadResponse"]>(
    "/api/leads",
    body,
  );

  return {
    id: result.id ?? "",
    esContactoRepetido: result.esContactoRepetido ?? false,
  };
}

export type EnrichOwnerInquiryInput = {
  email: string;
  propertyType: string;
  city: string;
  neighborhood?: string;
  expectedPrice?: number;
  message?: string;
  preferredContactChannel: string;
};

export async function enrichOwnerInquiry(
  leadId: string,
  input: EnrichOwnerInquiryInput,
): Promise<void> {
  await apiPut<EnrichOwnerInquiryInput, unknown>(
    `/api/leads/${encodeURIComponent(leadId)}/owner-details`,
    input,
  );
}
