import type { components } from "./schema";
import { apiGetOrNull, apiPost } from "./client";

type Schemas = components["schemas"];

export type ActivePolicy = {
  version: string;
  fechaVigencia: string;
};

export type LeadResult = {
  id: string;
  esContactoRepetido: boolean;
};

// Servicios que ofrece L&C al propietario. `ownerService` solo existe en el
// dominio para Sell, Rent y Manage; Avaluar viaja sin el campo y se distingue
// por la fuente. `operation` respeta Lead.ValidateOwnerService: Sell exige
// Venta y el resto Arriendo.
export const OWNER_SERVICES = [
  { value: "vender", label: "Vender", ownerService: "Sell", operation: "Venta" },
  { value: "arrendar", label: "Arrendar", ownerService: "Rent", operation: "Arriendo" },
  { value: "administrar", label: "Administrar", ownerService: "Manage", operation: "Arriendo" },
  { value: "avaluar", label: "Avaluar", ownerService: null, operation: "Venta" },
] as const;

export type OwnerServiceValue = (typeof OWNER_SERVICES)[number]["value"];

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
