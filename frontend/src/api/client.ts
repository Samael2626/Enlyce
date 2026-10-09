const API_BASE = import.meta.env.VITE_API_URL || "http://localhost:5019"

class ApiClient {
  private baseUrl: string

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl
  }

  resolveMediaUrl(url: string) {
    if (/^https?:\/\//i.test(url)) return url
    return `${this.baseUrl}${url.startsWith("/") ? url : `/${url}`}`
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
      const error = await response.json().catch(() => ({})) as {
        error?: string
        detail?: string
        title?: string
        message?: string
      }
      throw new Error(
        error.error || error.detail || error.title || error.message || `Error ${response.status}`
      )
    }

    if (response.status === 204) {
      return undefined as T
    }

    const text = await response.text()
    if (!text || text.trim() === "") {
      return undefined as T
    }

    return JSON.parse(text)
  }

  // Auth
  async login(email: string, password: string) {
    return this.request<{ rol: string; nombre: string }>(
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

  async logout() {
    return this.request<void>("/api/auth/logout", { method: "POST" })
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

  // Contactos
  async getContacts(filters: { q?: string; from?: string; to?: string; page?: number; pageSize?: number } = {}) {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== "") query.set(key, String(value))
    })
    return this.request<import("@/lib/types").PagedItems<import("@/lib/types").ContactSummary>>(`/api/contactos?${query}`)
  }

  async getContact(id: string) {
    return this.request<import("@/lib/types").ContactDetail>(`/api/contactos/${id}`)
  }

  async updateContact(id: string, data: { name: string; phone?: string }) {
    return this.request<import("@/lib/types").ContactSummary>(`/api/contactos/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    })
  }

  async createCommercialTask(data: import("@/lib/types").CreateCommercialTaskInput) {
    return this.request<import("@/lib/types").CommercialTask>("/api/tareas", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  async getCommercialTasks(filters: { from?: string; to?: string; contactId?: string; query?: string; status?: string; priority?: string; page?: number; pageSize?: number } = {}) {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== "") query.set(key, String(value))
    })
    return this.request<import("@/lib/types").PagedItems<import("@/lib/types").CommercialTask>>(`/api/tareas?${query}`)
  }

  async bulkCompleteTasks(ids: string[]) {
    return this.request<import("@/lib/types").BulkTaskResult>("/api/tareas/bulk/completar", { method: "POST", body: JSON.stringify({ ids }) })
  }

  async bulkCancelTasks(ids: string[]) {
    return this.request<import("@/lib/types").BulkTaskResult>("/api/tareas/bulk/cancelar", { method: "POST", body: JSON.stringify({ ids }) })
  }

  async getCommercialTaskAlerts(through: string) {
    const query = new URLSearchParams({ through })
    return this.request<import("@/lib/types").CommercialTask[]>(`/api/tareas/alertas?${query.toString()}`)
  }

  async completeCommercialTask(id: string) {
    return this.request<import("@/lib/types").CommercialTask>(`/api/tareas/${id}/completar`, { method: "PUT" })
  }

  async cancelCommercialTask(id: string) {
    return this.request<import("@/lib/types").CommercialTask>(`/api/tareas/${id}/cancelar`, { method: "PUT" })
  }

  async rescheduleCommercialTask(id: string, dueAt: string, reminderAt?: string) {
    return this.request<import("@/lib/types").CommercialTask>(`/api/tareas/${id}/reprogramar`, {
      method: "PUT",
      body: JSON.stringify({ dueAt, reminderAt }),
    })
  }

  async addCommercialTaskComment(id: string, comment: string) {
    return this.request(`/api/tareas/${id}/comentarios`, {
      method: "POST",
      body: JSON.stringify({ comment }),
    })
  }

  async getCommercialTaskHistory(id: string) {
    return this.request<import("@/lib/types").CommercialTaskEvent[]>(`/api/tareas/${id}/historial`)
  }

  async getCustomerDemands(contactId: string) {
    const query = new URLSearchParams({ ContactId: contactId })
    return this.request<import("@/lib/types").CustomerDemand[]>(`/api/demandas?${query.toString()}`)
  }

  async createCustomerDemand(data: import("@/lib/types").CreateCustomerDemandInput) {
    return this.request<import("@/lib/types").CustomerDemand>("/api/demandas", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  async getDemandMatches(demandId: string) {
    return this.request<import("@/lib/types").DemandPropertyMatch[]>(`/api/demandas/${demandId}/coincidencias`)
  }

  async linkDemandProperty(demandId: string, propertyId: string) {
    return this.request(`/api/demandas/${demandId}/inmuebles/${propertyId}`, { method: "POST" })
  }

  async setDemandPropertyStatus(demandId: string, propertyId: string, status: string) {
    return this.request(`/api/demandas/${demandId}/inmuebles/${propertyId}/estado`, {
      method: "PUT",
      body: JSON.stringify({ status }),
    })
  }

  // Pipeline
  async getPipeline(filters: { q?: string; etapa?: string; operacion?: string; asesorId?: string; desde?: string; hasta?: string; page?: number; pageSize?: number } = {}) {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== "") query.set(key, String(value))
    })
    return this.request<import("@/lib/types").PipelineResult>(`/api/pipeline?${query}`)
  }

  async bulkAssignLeads(leadIds: string[], advisorId: string, reason: string) {
    return this.request<{ results: { leadId: string; success: boolean; error?: string }[]; succeeded: number; failed: number }>("/api/pipeline/asignar-masivo", {
      method: "PUT", body: JSON.stringify({ leadIds, asesorId: advisorId, reason }),
    })
  }

  async getLeadHistory(leadId: string) {
    return this.request<import("@/lib/types").LeadHistoryItem[]>(`/api/pipeline/${leadId}/historial`)
  }

  async getAdvisors() {
    return this.request<import("@/lib/types").AdvisorOption[]>("/api/asesores")
  }

  async getFirstResponseMetrics(from: string, to: string) {
    const query = new URLSearchParams({ from, to })
    return this.request<import("@/lib/types").FirstResponseMetrics>(`/api/analytics/first-response?${query}`)
  }

  async getCrmReportMetrics(from: string, to: string) {
    const query = new URLSearchParams({ from, to })
    return this.request<import("@/lib/types").CrmReportMetrics>(`/api/analytics/crm-report?${query}`)
  }

  async downloadOpportunitiesCsv(from: string, to: string): Promise<Blob> {
    const query = new URLSearchParams({ from, to })
    const response = await fetch(`${this.baseUrl}/api/opportunities/export.csv?${query}`, {
      credentials: "include",
    })
    if (!response.ok) throw new Error(`Error ${response.status}`)
    return response.blob()
  }

  async moveLeadInPipeline(leadId: string, nuevaEtapa: string, reason = "Cambio desde pipeline") {
    return this.request<void>(`/api/pipeline/${leadId}/mover-etapa`, {
      method: "PUT",
      body: JSON.stringify({ nuevaEtapa, reason }),
    })
  }

  async assignLeadToAsesor(leadId: string, asesorId: string, reason: string) {
    return this.request<void>(`/api/pipeline/${leadId}/asignar`, {
      method: "PUT",
      body: JSON.stringify({ asesorId, reason }),
    })
  }

  async getLeadAssignmentHistory(leadId: string) {
    return this.request<import("@/lib/types").LeadAssignmentHistory[]>(`/api/pipeline/${leadId}/asignaciones`)
  }

  // Interacciones
  async getInteracciones(leadId: string) {
    return this.request<any[]>(`/api/interacciones/lead/${leadId}`)
  }

  async registrarInteraccion(data: {
    leadId: string
    asesorId: string
    tipo: string
    resumen?: string
  }) {
    return this.request<any>("/api/interacciones", {
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

  async rescheduleVisit(id: string, fechaProgramada: string) {
    return this.request<import("@/lib/types").Visita>(`/api/visitas/${id}/reprogramar`, {
      method: "PUT",
      body: JSON.stringify({ fechaProgramada }),
    })
  }

  async completeVisit(id: string, feedback?: string) {
    return this.request<import("@/lib/types").Visita>(`/api/visitas/${id}/realizada`, {
      method: "PUT",
      body: JSON.stringify({ feedback }),
    })
  }

  async cancelVisit(id: string) {
    return this.request<import("@/lib/types").Visita>(`/api/visitas/${id}/cancelar`, { method: "PUT" })
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

  // Publicaciones
  async getPublications() {
    return this.request<import("@/lib/types").PropertyPublicationAdmin[]>("/api/publicaciones")
  }

  async getPublicationOptions() {
    return this.request<import("@/lib/types").PropertyPublicationOptions>(
      "/api/publicaciones/opciones"
    )
  }

  async createPublication(data: import("@/lib/types").PropertyPublicationInput) {
    return this.request<{ id: string; status: string }>("/api/publicaciones", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  async updatePublication(
    id: string,
    data: import("@/lib/types").PropertyPublicationInput
  ) {
    return this.request<{ id: string; status: string }>(`/api/publicaciones/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    })
  }

  async changePublicationStatus(id: string, action: "publish" | "pause" | "withdraw") {
    return this.request<{ id: string; status: string }>(
      `/api/publicaciones/${id}/estado`,
      { method: "PUT", body: JSON.stringify({ action }) }
    )
  }

  async uploadPublicationPhoto(
    publicationId: string,
    file: File,
    altText: string,
    isCover: boolean
  ) {
    const data = new FormData()
    data.append("archivo", file)
    data.append("textoAlternativo", altText)
    data.append("esPortada", String(isCover))

    const response = await fetch(
      `${this.baseUrl}/api/publicaciones/${publicationId}/fotos`,
      { method: "POST", credentials: "include", body: data }
    )
    if (!response.ok) {
      const error = await response.json().catch(() => ({})) as { error?: string; detail?: string }
      throw new Error(error.error || error.detail || `Error ${response.status}`)
    }
    return response.json()
  }

  // Facturacion
  async getBillingPricing() {
    return this.request<import("@/lib/types").BillingPricing>("/api/billing/pricing")
  }

  async createCheckoutSession(data: import("@/lib/types").CreateCheckoutSessionInput) {
    return this.request<import("@/lib/types").CreateCheckoutSessionResponse>(
      "/api/billing/checkout-sessions",
      { method: "POST", body: JSON.stringify(data) }
    )
  }

  async getPaymentOrderStatus(reference: string) {
    return this.request<import("@/lib/types").PaymentOrderStatus>(
      `/api/billing/orders/${encodeURIComponent(reference)}`
    )
  }

  // Analitica web
  async getAnalyticsFunnel(days: number) {
    const to = new Date()
    const from = new Date(to)
    from.setUTCDate(from.getUTCDate() - days)
    const query = new URLSearchParams({ from: from.toISOString(), to: to.toISOString() })
    return this.request<import("@/lib/types").WebAnalyticsFunnel>(
      `/api/analytics/funnel?${query.toString()}`
    )
  }

  async getLeadDistributionRule() {
    return this.request<{ rule: import("@/lib/types").LeadDistributionRule }>("/api/configuracion/reparto-leads")
  }

  async setLeadDistributionRule(rule: import("@/lib/types").LeadDistributionRule) {
    return this.request<{ rule: import("@/lib/types").LeadDistributionRule }>("/api/configuracion/reparto-leads", {
      method: "PUT",
      body: JSON.stringify({ rule }),
    })
  }

  // Politica
  async getPoliticaActiva() {
    return this.request<any>("/api/politica/activa")
  }

  async consultarDatosLead(leadId: string) {
    return this.request<any>(`/api/datos-personales/${leadId}`)
  }

  async suprimirDatosLead(leadId: string) {
    return this.request<void>(`/api/datos-personales/${leadId}`, {
      method: "DELETE",
    })
  }
}

export const api = new ApiClient(API_BASE)
