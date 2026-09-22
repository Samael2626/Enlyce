export const DEFAULT_API_BASE = 'http://localhost:5019';

export class ApiError extends Error {
  constructor(status, problem) {
    const normalized = normalizeProblem(problem);
    super(normalized.messages[0] || normalized.title);
    this.name = 'ApiError';
    this.status = status;
    this.problem = normalized;
  }
}

export function resolveApiBase(search = '') {
  const configured = new URLSearchParams(search).get('api');
  return (configured || DEFAULT_API_BASE).replace(/\/$/, '');
}

export function buildCatalogUrl(apiBase, filters) {
  const url = new URL('/api/public/inmuebles', `${apiBase.replace(/\/$/, '')}/`);

  for (const [key, value] of Object.entries(filters)) {
    if (value === '' || value === null || value === undefined) continue;
    url.searchParams.set(key, String(value));
  }

  return url.toString();
}

export function buildDetailUrl(apiBase, slug) {
  return `${apiBase.replace(/\/$/, '')}/api/public/inmuebles/${encodeURIComponent(slug)}`;
}

export const LEGACY_CHANNEL = 'funcional_legacy';

export function buildVisitLeadPayload({ name, email, phone, consent, property }) {
  return {
    nombre: name,
    email,
    telefono: phone,
    fuente: `Website:${property.slug}`,
    autorizacionDatos: consent,
    tipoOperacion: property.operation === 'Arriendo' ? 'Arriendo' : 'Venta',
    publicationId: property.id,
    canal: LEGACY_CHANNEL,
  };
}

export function normalizeProblem(problem) {
  const title = typeof problem?.title === 'string' ? problem.title : 'No pudimos completar la solicitud';
  const validationMessages = problem?.errors && typeof problem.errors === 'object'
    ? Object.values(problem.errors).flat().filter((message) => typeof message === 'string')
    : [];
  const detail = typeof problem?.detail === 'string' ? [problem.detail] : [];

  return {
    title,
    messages: validationMessages.length ? validationMessages : detail,
  };
}

async function readResponse(response) {
  const isJson = response.headers.get('content-type')?.includes('json');
  const body = isJson ? await response.json() : null;
  if (!response.ok) throw new ApiError(response.status, body);
  return body;
}

export class PublicCatalogClient {
  constructor(apiBase = DEFAULT_API_BASE, fetchImplementation = fetch) {
    this.apiBase = apiBase.replace(/\/$/, '');
    this.fetch = fetchImplementation.bind(globalThis);
  }

  async getProperties(filters, signal) {
    const response = await this.fetch(buildCatalogUrl(this.apiBase, filters), {
      headers: { Accept: 'application/json' },
      signal,
    });
    return readResponse(response);
  }

  async getProperty(slug, signal) {
    const response = await this.fetch(buildDetailUrl(this.apiBase, slug), {
      headers: { Accept: 'application/json' },
      signal,
    });
    return readResponse(response);
  }

  async createLead(payload, signal) {
    const response = await this.fetch(`${this.apiBase}/api/leads/`, {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
      signal,
    });
    return readResponse(response);
  }
}
