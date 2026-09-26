import type { components } from "./schema";

type Schemas = components["schemas"];

// Swashbuckle genera todo opcional y anulable. Strict deja los campos
// obligatorios y sin null para que las paginas no vayan sembradas de guardas.
type Strict<T> = { [K in keyof T]-?: NonNullable<T[K]> };

// Las respuestas generadas desde Swashbuckle llegan con todo opcional. Se
// estrechan aqui una sola vez para que las paginas trabajen con datos firmes.
export type PropertyListItem = Strict<
  Pick<
    Schemas["PublicPropertyListItemResponse"],
    | "id"
    | "slug"
    | "publicTitle"
    | "propertyType"
    | "operation"
    | "areaSquareMeters"
    | "bedrooms"
    | "bathrooms"
    | "parkingSpaces"
    | "publishedAt"
  >
> & {
  price: Strict<Schemas["PublicMoneyResponse"]>;
  location: Strict<Schemas["PublicLocationResponse"]>;
  coverPhoto?: Strict<Schemas["PublicCoverPhotoResponse"]> | null;
};

export type PropertyPage = {
  page: number;
  pageSize: number;
  total: number;
  items: PropertyListItem[];
};

export type PropertyPhoto = Strict<Schemas["PublicPhotoResponse"]>;

export type PropertyDetail = Strict<
  Pick<
    Schemas["PublicPropertyDetailResponse"],
    "id" | "slug" | "publicTitle" | "publicDescription" | "propertyType" | "operation" | "publishedAt"
  >
> & {
  price: Strict<Schemas["PublicMoneyResponse"]>;
  administrationFee?: Strict<Schemas["PublicMoneyResponse"]> | null;
  location: Strict<Schemas["PublicLocationResponse"]>;
  features: Strict<Omit<Schemas["PublicPropertyFeaturesResponse"], "stratum">> & {
    stratum?: number | null;
  };
  photos: PropertyPhoto[];
  advisor: Strict<Omit<Schemas["PublicAdvisorResponse"], "publicPhone">> & {
    publicPhone?: string | null;
  };
};

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

const baseUrl = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5019").replace(/\/$/, "");

export function resolveMediaUrl(url: string): string {
  try {
    const parsed = new URL(url);
    return parsed.pathname.startsWith("/media/")
      ? `${baseUrl}${parsed.pathname}${parsed.search}`
      : url;
  } catch {
    return url;
  }
}

type RequestOptions = {
  // Revalidacion ISR. El catalogo ya manda Cache-Control public,max-age=300.
  revalidate?: number;
};

export async function apiGet<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: { Accept: "application/json" },
    next: { revalidate: options.revalidate ?? 300 },
  });

  if (!response.ok) {
    // Fallar ruidoso: la pagina decide si es 404 o error, nunca se traga.
    throw new ApiError(response.status, `GET ${path} respondio ${response.status}`);
  }

  return (await response.json()) as T;
}

export async function apiGetOrNull<T>(path: string, options: RequestOptions = {}): Promise<T | null> {
  try {
    return await apiGet<T>(path, options);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

// Mismo criterio que normalizeProblem en website/funcional/js/api.js: los
// errores de validacion del backend mandan sobre el detail, y si no hay
// ninguno se muestra un mensaje generico en vez de un volcado tecnico.
export type ProblemDetails = {
  title?: string | null;
  detail?: string | null;
  errors?: Record<string, string[]> | null;
};

export function normalizeProblem(problem: unknown): { title: string; messages: string[] } {
  const source = (problem ?? {}) as ProblemDetails;
  const title =
    typeof source.title === "string" && source.title.trim()
      ? source.title
      : "No pudimos completar la solicitud";

  const validationMessages =
    source.errors && typeof source.errors === "object"
      ? Object.values(source.errors)
          .flat()
          .filter((message): message is string => typeof message === "string")
      : [];

  const detail = typeof source.detail === "string" ? [source.detail] : [];

  return { title, messages: validationMessages.length ? validationMessages : detail };
}

export class ApiProblemError extends Error {
  readonly messages: string[];

  constructor(
    readonly status: number,
    problem: unknown,
  ) {
    const normalized = normalizeProblem(problem);
    super(normalized.messages[0] ?? normalized.title);
    this.name = "ApiProblemError";
    this.messages = normalized.messages.length ? normalized.messages : [normalized.title];
  }
}

export async function apiPost<TBody, TResult>(path: string, body: TBody): Promise<TResult> {
  const response = await fetch(`${baseUrl}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(body),
  });

  const isJson = response.headers.get("content-type")?.includes("json") ?? false;
  const payload: unknown = isJson ? await response.json() : null;

  if (!response.ok) throw new ApiProblemError(response.status, payload);

  return payload as TResult;
}

export async function apiPut<TBody, TResult>(path: string, body: TBody): Promise<TResult> {
  const response = await fetch(`${baseUrl}${path}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(body),
  });

  const isJson = response.headers.get("content-type")?.includes("json") ?? false;
  const payload: unknown = isJson ? await response.json() : null;

  if (!response.ok) throw new ApiProblemError(response.status, payload);

  return payload as TResult;
}

export { baseUrl as apiBaseUrl };
