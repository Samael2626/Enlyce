const API_BASE = import.meta.env.VITE_API_URL || "http://localhost:5050"

class ApiClient {
  private baseUrl: string

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`
    const response = await fetch(url, {
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
      ...options,
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({
        message: `Error ${response.status}`,
      }))
      throw new Error(error.message || `Error ${response.status}`)
    }

    if (response.status === 204) {
      return undefined as T
    }

    return response.json()
  }

  // Auth
  async login(email: string, password: string) {
    return this.request<{ token: string; rol: string; nombre: string }>(
      "/api/auth/login",
      {
        method: "POST",
        body: JSON.stringify({ correo: email, password }),
      }
    )
  }

  async getMe() {
    return this.request<{ id: string; email: string; nombre: string; rol: string }>(
      "/api/auth/me"
    )
  }

  // Leads
  async createLead(data: {
    nombre: string
    email: string
    telefono?: string
    fuente?: string
    autorizacionDatos: boolean
    tipoOperacion?: string
  }) {
    return this.request<{ id: string; nombre: string; email: string; estado: string; fechaCreacion: string }>(
      "/api/leads",
      { method: "POST", body: JSON.stringify(data) }
    )
  }

  async getLeadById(id: string) {
    return this.request<any>(`/api/leads/${id}`)
  }

  // Pipeline
  async getPipeline() {
    return this.request<any>("/api/pipeline")
  }

  async moveLeadInPipeline(leadId: string, nuevaEtapa: string) {
    return this.request<void>(`/api/pipeline/leads/${leadId}/mover-etapa`, {
      method: "PUT",
      body: JSON.stringify({ nuevaEtapa }),
    })
  }

  async assignLeadToAsesor(leadId: string, asesorId: string) {
    return this.request<void>(`/api/pipeline/leads/${leadId}/asignar`, {
      method: "PUT",
      body: JSON.stringify({ asesorId }),
    })
  }

  // Interacciones
  async getInteracciones(leadId: string) {
    return this.request<any[]>(`/api/leads/${leadId}/interacciones`)
  }

  async registrarInteraccion(data: {
    leadId: string
    asesorId: string
    tipo: string
    resumen?: string
  }) {
    return this.request<any>("/api/leads/interacciones", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  // Visitas
  async getVisitas() {
    return this.request<any[]>("/api/visitas")
  }

  async getVisitaById(id: string) {
    return this.request<any>(`/api/visitas/${id}`)
  }

  async registrarVisita(data: {
    leadId: string
    inmuebleId: string
    asesorId: string
    fechaProgramada: string
  }) {
    return this.request<any>("/api/visitas", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  // Alertas
  async getAlertas() {
    return this.request<any>("/api/alertas")
  }

  // Inmuebles
  async createInmueble(data: any) {
    return this.request<any>("/api/inmuebles", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  async getInmuebleById(id: string) {
    return this.request<any>(`/api/inmuebles/${id}`)
  }

  // Politica
  async getPoliticaActiva() {
    return this.request<any>("/api/ley1581/politica")
  }

  async consultarDatosLead(leadId: string) {
    return this.request<any>(`/api/ley1581/leads/${leadId}/datos`)
  }

  async revocarConsentimiento(leadId: string) {
    return this.request<void>(`/api/ley1581/leads/${leadId}/revocar`, {
      method: "PUT",
    })
  }
}

export const api = new ApiClient(API_BASE)
