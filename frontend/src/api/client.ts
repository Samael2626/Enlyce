const API_BASE = import.meta.env.VITE_API_URL ?? (import.meta.env.DEV ? "http://localhost:5019" : window.location.origin)

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
    const headers = new Headers(options.headers)
    if (!(options.body instanceof FormData) && !headers.has("Content-Type"))
      headers.set("Content-Type", "application/json")
    const response = await fetch(url, {
      credentials: "include",
      headers,
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

  async previewClientImport(file: File, nameColumn?: number, surnameColumn?: number, emailColumn?: number,
    phoneColumn?: number, canReceiveEmailColumn?: number, customerTypeColumn?: number,
    customerStatusColumn?: number, addressColumn?: number, countryColumn?: number,
    departmentColumn?: number, cityColumn?: number, landlineColumn?: number, dateOfBirthColumn?: number,
    identificationNumberColumn?: number, referredByColumn?: number, acquisitionMediumColumn?: number,
    additionalDataColumn?: number, customerLabelsColumn?: number, customerFunnelColumn?: number) {
    const body = new FormData()
    body.set("archivo", file)
    if (nameColumn !== undefined) body.set("nameColumn", String(nameColumn))
    if (surnameColumn !== undefined) body.set("surnameColumn", String(surnameColumn))
    if (emailColumn !== undefined) body.set("emailColumn", String(emailColumn))
    if (phoneColumn !== undefined) body.set("phoneColumn", String(phoneColumn))
    if (canReceiveEmailColumn !== undefined) body.set("canReceiveEmailColumn", String(canReceiveEmailColumn))
    if (customerTypeColumn !== undefined) body.set("customerTypeColumn", String(customerTypeColumn))
    if (customerStatusColumn !== undefined) body.set("customerStatusColumn", String(customerStatusColumn))
    if (addressColumn !== undefined) body.set("addressColumn", String(addressColumn))
    if (countryColumn !== undefined) body.set("countryColumn", String(countryColumn))
    if (departmentColumn !== undefined) body.set("departmentColumn", String(departmentColumn))
    if (cityColumn !== undefined) body.set("cityColumn", String(cityColumn))
    if (landlineColumn !== undefined) body.set("landlineColumn", String(landlineColumn))
    if (dateOfBirthColumn !== undefined) body.set("dateOfBirthColumn", String(dateOfBirthColumn))
    if (identificationNumberColumn !== undefined) body.set("identificationNumberColumn", String(identificationNumberColumn))
    if (referredByColumn !== undefined) body.set("referredByColumn", String(referredByColumn))
    if (acquisitionMediumColumn !== undefined) body.set("acquisitionMediumColumn", String(acquisitionMediumColumn))
    if (additionalDataColumn !== undefined) body.set("additionalDataColumn", String(additionalDataColumn))
    if (customerLabelsColumn !== undefined) body.set("customerLabelsColumn", String(customerLabelsColumn))
    if (customerFunnelColumn !== undefined) body.set("customerFunnelColumn", String(customerFunnelColumn))
    return this.request<{
      totalRows: number
      validRows: number
      invalidRows: number
      duplicateRows: number
      sourceColumns: string[]
      rows: Array<{ rowNumber: number; name: string | null; surname: string | null; email: string | null;
        phone: string | null; canReceiveAutomatedEmail: boolean; mappedValues: Record<string, string | null>;
        error: string | null; duplicate: boolean }>
    }>("/api/import/clientes/previsualizar", { method: "POST", body })
  }

  async confirmClientImport(file: File, nameColumn: number, importRequestId: string,
    surnameColumn?: number, emailColumn?: number, phoneColumn?: number, canReceiveEmailColumn?: number,
    advisorId?: string, customerTypeColumn?: number, customerStatusColumn?: number, addressColumn?: number,
    countryColumn?: number, departmentColumn?: number, cityColumn?: number, landlineColumn?: number,
    dateOfBirthColumn?: number, identificationNumberColumn?: number, referredByColumn?: number,
    acquisitionMediumColumn?: number, additionalDataColumn?: number, customerLabelsColumn?: number,
    customerFunnelColumn?: number) {
    const body = new FormData()
    body.set("archivo", file)
    body.set("nameColumn", String(nameColumn))
    body.set("importRequestId", importRequestId)
    if (surnameColumn !== undefined) body.set("surnameColumn", String(surnameColumn))
    if (emailColumn !== undefined) body.set("emailColumn", String(emailColumn))
    if (phoneColumn !== undefined) body.set("phoneColumn", String(phoneColumn))
    if (canReceiveEmailColumn !== undefined) body.set("canReceiveEmailColumn", String(canReceiveEmailColumn))
    if (advisorId) body.set("advisorId", advisorId)
    if (customerTypeColumn !== undefined) body.set("customerTypeColumn", String(customerTypeColumn))
    if (customerStatusColumn !== undefined) body.set("customerStatusColumn", String(customerStatusColumn))
    if (addressColumn !== undefined) body.set("addressColumn", String(addressColumn))
    if (countryColumn !== undefined) body.set("countryColumn", String(countryColumn))
    if (departmentColumn !== undefined) body.set("departmentColumn", String(departmentColumn))
    if (cityColumn !== undefined) body.set("cityColumn", String(cityColumn))
    if (landlineColumn !== undefined) body.set("landlineColumn", String(landlineColumn))
    if (dateOfBirthColumn !== undefined) body.set("dateOfBirthColumn", String(dateOfBirthColumn))
    if (identificationNumberColumn !== undefined) body.set("identificationNumberColumn", String(identificationNumberColumn))
    if (referredByColumn !== undefined) body.set("referredByColumn", String(referredByColumn))
    if (acquisitionMediumColumn !== undefined) body.set("acquisitionMediumColumn", String(acquisitionMediumColumn))
    if (additionalDataColumn !== undefined) body.set("additionalDataColumn", String(additionalDataColumn))
    if (customerLabelsColumn !== undefined) body.set("customerLabelsColumn", String(customerLabelsColumn))
    if (customerFunnelColumn !== undefined) body.set("customerFunnelColumn", String(customerFunnelColumn))
    return this.request<{
      totalRows: number
      importedRows: number
      invalidRows: number
      duplicateRows: number
      rows: Array<{ rowNumber: number; name: string | null; surname: string | null; email: string | null;
        phone: string | null; canReceiveAutomatedEmail: boolean; error: string | null; duplicate: boolean }>
      errors: Array<{ rowNumber: number; error: string }>
      importedContacts: Array<{ rowNumber: number; contactId: string }>
    }>("/api/import/clientes/confirmar", { method: "POST", body })
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
  async createContact(data: { name: string; surname?: string; email?: string; phone?: string; landline?: string;
    customerType?: string; customerTypes?: string[]; customerLabels?: string[]; customerStatus?: string; customerFunnel?: string; address?: string; country?: string; department?: string;
    city?: string; dateOfBirth?: string; identificationNumber?: string; referredBy?: string;
    acquisitionMedium?: string; additionalData?: string; advisorId?: string }) {
    return this.request<import("@/lib/types").ContactSummary>("/api/contactos", {
      method: "POST", body: JSON.stringify(data),
    })
  }

  async getContacts(filters: { q?: string; from?: string; to?: string; customerType?: string;
    customerStatus?: string; customerFunnel?: string; customerLabel?: string; advisorId?: string; page?: number; pageSize?: number } = {}) {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== "") query.set(key, String(value))
    })
    return this.request<import("@/lib/types").PagedItems<import("@/lib/types").ContactSummary>>(`/api/contactos?${query}`)
  }

  async getContact(id: string) {
    return this.request<import("@/lib/types").ContactDetail>(`/api/contactos/${id}`)
  }

  async setContactEmailPermission(id: string, allowed: boolean, evidenceReference: string | null) {
    return this.request<{ id: string; canReceiveAutomatedEmail: boolean }>(`/api/contactos/${id}/permiso-correo`, {
      method: "PUT", body: JSON.stringify({ allowed, source: "Manual", evidenceReference }),
    })
  }

  async getContactEmailPermissionHistory(id: string) {
    return this.request<Array<{ allowed: boolean; source: string; evidenceReference: string | null; actorId: string; occurredAt: string }>>(
      `/api/contactos/${id}/permiso-correo/historial`,
    )
  }

  async updateContact(id: string, data: { name: string; surname?: string; phone?: string; customerType?: string; customerTypes?: string[]; customerLabels?: string[];
    customerStatus?: string; customerFunnel?: string; address?: string; country?: string; department?: string; city?: string;
    landline?: string; dateOfBirth?: string; identificationNumber?: string; referredBy?: string;
    acquisitionMedium?: string; additionalData?: string }) {
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

  async getCustomerDemandCross(filters: { q?: string; customerType?: string; customerStatus?: string; customerFunnel?: string;
    advisorId?: string; contactFrom?: string; contactTo?: string; demandFrom?: string; demandTo?: string;
    page?: number; pageSize?: number }) {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== "") query.set(key, String(value))
    })
    return this.request<import("@/lib/types").CustomerDemandCrossPage>(`/api/demandas/cruce?${query}`)
  }

  async createCustomerDemand(data: import("@/lib/types").CreateCustomerDemandInput) {
    return this.request<import("@/lib/types").CustomerDemand>("/api/demandas", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  async updateCustomerDemand(id: string, data: import("@/lib/types").UpdateCustomerDemandInput) {
    return this.request<import("@/lib/types").CustomerDemand>(`/api/demandas/${id}`, {
      method: "PUT", body: JSON.stringify(data),
    })
  }

  async deleteCustomerDemand(id: string) {
    return this.request<void>(`/api/demandas/${id}`, { method: "DELETE" })
  }

  async getDemandMatches(demandId: string) {
    return this.request<import("@/lib/types").DemandPropertyMatch[]>(`/api/demandas/${demandId}/coincidencias`)
  }

  async getPropertyDemandMatches(propertyId: string) {
    return this.request<import("@/lib/types").PropertyDemandMatch[]>(`/api/inmuebles/${propertyId}/demandas`)
  }

  async getPropertyDemandCounts(propertyIds: string[]) {
    const query = new URLSearchParams(propertyIds.map((id) => ["ids", id]))
    return this.request<Record<string, number>>(`/api/inmuebles/clientes-posibles?${query.toString()}`)
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

  async getInteractions(filters: { type?: string; query?: string; advisorId?: string; from?: string; to?: string; page?: number; pageSize?: number } = {}) {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== "") query.set(key, String(value))
    })
    return this.request<{ total: number; items: { id: string; leadId: string; contactId: string | null; advisorId: string; type: string; summary: string | null; date: string; clientName: string }[] }>(`/api/interacciones?${query}`)
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

  async downloadContactsCsv(filters: { q?: string; from?: string; to?: string; customerType?: string;
    customerStatus?: string; customerFunnel?: string; customerLabel?: string; advisorId?: string }): Promise<Blob> {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value) query.set(key, value)
    })
    const response = await fetch(`${this.baseUrl}/api/contactos/export.csv?${query}`, { credentials: "include" })
    if (!response.ok) {
      const error = await response.json().catch(() => ({})) as { error?: string; detail?: string }
      throw new Error(error.error ?? error.detail ?? `Error ${response.status}`)
    }
    return response.blob()
  }

  async emailContactsCsv(filters: { q?: string; from?: string; to?: string; customerType?: string;
    customerStatus?: string; customerFunnel?: string; customerLabel?: string; advisorId?: string }) {
    const query = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => {
      if (value) query.set(key, value)
    })
    return this.request<{ sent: boolean; count: number }>(`/api/contactos/exportar-correo?${query}`, { method: "POST" })
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

  async getLeadSlaAlerts() {
    return this.request<import("@/lib/types").LeadSlaAlertsResponse>("/api/alertas/sla")
  }

  // Inmuebles
  async createInmueble(data: any) {
    return this.request<any>("/api/inmuebles", {
      method: "POST",
      body: JSON.stringify(data),
    })
  }

  async updateInmueble(id: string, data: any) {
    return this.request<any>(`/api/inmuebles/${id}`, { method: "PUT", body: JSON.stringify(data) })
  }

  async changeInmuebleTrashState(id: string, restore: boolean) {
    return this.request<void>(`/api/inmuebles/${id}/${restore ? "restaurar" : "papelera"}`, { method: "POST" })
  }

  async getInmuebles(filters: {
    page?: number; pageSize?: number; search?: string; modalidad?: string; estado?: string; tipo?: string;
    pais?: string; departamento?: string; ciudad?: string; barrio?: string; moneda?: string;
    precioDesde?: number; precioHasta?: number; areaDesde?: number; areaHasta?: number;
    habitaciones?: number; banos?: number; parqueaderos?: number; orden?: string; papelera?: boolean;
  } = {}) {
    const params = new URLSearchParams()
    Object.entries(filters).forEach(([key, value]) => { if (value !== undefined && value !== "") params.set(key, String(value)) })
    return this.request<{ items: { id: string; nombre: string; tipo: string; modalidad: string; estado: string; tienePublicacion: boolean; ciudad: string; barrio?: string; moneda: string; precio: number; metrosCuadrados: number; habitaciones: number; banos: number; parqueaderos: number }[]; page: number; pageSize: number; total: number }>(`/api/inmuebles?${params}`)
  }

  async emailPropertiesXlsx() {
    return this.request<{ sent: boolean; count: number }>("/api/inmuebles/exportar-correo", { method: "POST" })
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

  async getLeadSlaRules() {
    return this.request<{ rules: import("@/lib/types").LeadSlaRule[] }>("/api/configuracion/sla-leads")
  }

  async getCustomerCatalogs() {
    return this.request<{ types: string[]; labels: string[]; funnels: string[]; acquisitionMedia: string[] }>(
      "/api/configuracion/catalogos-clientes")
  }

  async addCustomerCatalogItem(catalog: string, name: string) {
    return this.request<{ catalog: string; name: string }>("/api/configuracion/catalogos-clientes", {
      method: "POST", body: JSON.stringify({ catalog, name }),
    })
  }

  async upsertLeadSlaRule(rule: import("@/lib/types").UpsertLeadSlaRule) {
    return this.request<void>("/api/configuracion/sla-leads", {
      method: "PUT",
      body: JSON.stringify(rule),
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
