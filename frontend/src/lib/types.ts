// Auth
export interface LoginRequest {
  correo: string
  password: string
}

export interface LoginResponse {
  rol: string
  nombre: string
}

export interface MeResponse {
  id: string
  email: string
  nombre: string
  rol: string
}

// Leads
export interface CreateLeadRequest {
  nombre: string
  email: string
  telefono?: string
  fuente?: string
  autorizacionDatos: boolean
  tipoOperacion?: string
}

export interface CreateLeadResponse {
  id: string
  nombre: string
  email: string
  estado: string
  fechaCreacion: string
}

export interface GetLeadByIdResponse {
  id: string
  nombre: string
  email: string
  telefono?: string
  fuente: string
  estado: string
  motivoCierre: string
  fechaCreacion: string
  fechaUltimoContacto?: string
  autorizacionDatos: boolean
  activo: boolean
  etapaPipeline: string
  tipoOperacion: string
  ownerService?: string
  publicationId?: string
  ownerPropertyType?: string
  ownerPropertyCity?: string
  ownerPropertyNeighborhood?: string
  ownerExpectedPrice?: number
  ownerPropertyMessage?: string
  ownerPreferredContactChannel?: string
}

export interface ContactSummary {
  id: string
  name: string
  email: string
  phone?: string
  createdAt: string
  updatedAt: string
}

export interface ContactInteraction {
  id: string
  type: string
  summary?: string
  date: string
  advisorId: string
}

export interface ContactVisit {
  id: string
  propertyId: string
  scheduledAt: string
  completedAt?: string
  status: string
  feedback?: string
}

export interface ContactOpportunity {
  id: string
  operation: string
  stage: string
  source: string
  publicationId?: string
  createdAt: string
  advisorId?: string
  interactions: ContactInteraction[]
  visits: ContactVisit[]
}

export interface ContactDetail {
  contact: ContactSummary
  opportunities: ContactOpportunity[]
  tasks: ContactTask[]
}

export interface ContactTask {
  id: string
  leadId?: string
  advisorId: string
  type: string
  title: string
  description?: string
  dueAt: string
  reminderAt?: string
  priority: string
  status: string
  completedAt?: string
}

export interface CommercialTask extends ContactTask {
  contactId: string
  createdAt: string
  updatedAt: string
}

export interface CreateCommercialTaskInput {
  contactId: string
  leadId?: string
  advisorId?: string
  type: string
  title: string
  description?: string
  dueAt: string
  reminderAt?: string
  priority: string
}

export interface CommercialTaskEvent {
  id: string
  taskId: string
  actorId: string
  action: string
  comment?: string
  occurredAt: string
}

export interface CustomerDemand {
  id: string
  contactId: string
  leadId?: string
  operation: string
  propertyType?: string
  city: string
  neighborhood?: string
  minimumPrice?: number
  maximumPrice?: number
  bedrooms?: number
  bathrooms?: number
  parkingSpaces?: number
  notes?: string
  createdAt: string
  updatedAt: string
}

export interface DemandPropertyMatch {
  propertyId: string
  name: string
  propertyType: string
  operation: string
  city: string
  neighborhood?: string
  price: number
  currency: string
  bedrooms: number
  bathrooms: number
  parkingSpaces: number
  relationshipStatus?: string
}

export interface CreateCustomerDemandInput {
  contactId: string
  leadId?: string
  operation: string
  propertyType?: string
  city: string
  neighborhood?: string
  minimumPrice?: number
  maximumPrice?: number
  bedrooms?: number
  bathrooms?: number
  parkingSpaces?: number
  notes?: string
}

// Pipeline
export interface PipelineResponse {
  etapas: EtapaPipelineDto[]
  leads: LeadDto[]
  total: number
}

export interface FirstResponseMetric {
  name: string
  respondedLeads: number
  averageHours: number
}

export interface FirstResponseMetrics {
  from: string
  to: string
  respondedLeads: number
  averageHours: number | null
  byAdvisor: FirstResponseMetric[]
  bySource: FirstResponseMetric[]
}

export interface CrmReportMetric {
  name: string
  leads: number
  respondedLeads: number
  averageResponseHours: number | null
  visits: number
  wonLeads: number
  lostLeads: number
}

export interface CrmReportMetrics {
  from: string
  to: string
  leads: number
  respondedLeads: number
  visits: number
  wonLeads: number
  lostLeads: number
  byAdvisor: CrmReportMetric[]
  byStage: CrmReportMetric[]
  bySource: CrmReportMetric[]
  byCampaign: CrmReportMetric[]
  byOperation: CrmReportMetric[]
}

export interface EtapaPipelineDto {
  nombre: string
  etiqueta: string
  leadCount: number
}

export interface LeadDto {
  id: string
  nombre: string
  email: string
  telefono?: string
  fuente: string
  etapaPipeline: string
  tipoOperacion: string
  interaccionesCount: number
  fechaUltimaInteraccion?: string
  fechaCreacion: string
  asesorNombre?: string
  fechaPrimerContacto?: string
}

export interface LeadAssignmentHistory {
  id: string
  previousAdvisorId?: string
  previousAdvisorName?: string
  newAdvisorId: string
  newAdvisorName?: string
  changedByAdvisorId?: string
  changedByName?: string
  reason: string
  source: string
  changedAt: string
}

export interface MoverEtapaRequest {
  nuevaEtapa: string
}

export interface AsignarLeadRequest {
  asesorId: string
}

// Interacciones
export interface Interaccion {
  id: string
  leadId: string
  asesorId: string
  tipo: string
  resumen?: string
  fecha: string
}

export interface RegistrarInteraccionRequest {
  leadId: string
  asesorId: string
  tipo: string
  resumen?: string
}

// Visitas
export interface Visita {
  id: string
  leadId: string
  inmuebleId: string
  asesorId: string
  fechaProgramada: string
  fechaRealizada?: string
  feedback?: string
  estado: string
}

export interface RegistrarVisitaRequest {
  leadId: string
  inmuebleId: string
  asesorId: string
  fechaProgramada: string
}

// Alertas
export interface AlertasResponse {
  leads: AlertaLeadDto[]
  total: number
}

export interface AlertaLeadDto {
  id: string
  nombre: string
  email: string
  etapaPipeline: string
  fechaUltimaActividad: string
  diasSinActividad: number
}

// Inmuebles
export interface CreateInmuebleRequest {
  nombre: string
  descripcion?: string
  tipo: string
  modalidad: string
  calle: string
  ciudad: string
  barrio?: string
  precio: number
  moneda: string
  metrosCuadrados: number
  habitaciones: number
  banos: number
  parqueaderos: number
  propietarioId: string
}

export interface GetInmuebleByIdResponse {
  id: string
  nombre: string
  descripcion: string
  tipo: string
  modalidad: string
  estado: string
  direccion: string
  precio: number
  moneda: string
  metrosCuadrados: number
  habitaciones: number
  banos: number
  parqueaderos: number
  propietarioId: string
  fechaCreacion: string
  activo: boolean
}

// Publicaciones
export interface PublicationPhoto {
  url: string
  altText: string
  order: number
  isCover: boolean
}

export interface PropertyPublicationAdmin {
  id: string
  propertyId: string
  propertyName: string
  propertyType: string
  operation: string
  advisorId: string
  advisorName: string
  slug: string
  publicTitle: string
  publicDescription: string
  status: "Draft" | "Published" | "Paused" | "Withdrawn"
  priceAmount?: number
  priceCurrency?: string
  municipality?: string
  neighborhood?: string
  approximateLatitude?: number
  approximateLongitude?: number
  createdAt: string
  publishedAt?: string
  photos: PublicationPhoto[]
}

export interface PublicationPropertyOption {
  id: string
  name: string
  propertyType: string
  operation: string
  municipality: string
  neighborhood?: string
  priceAmount: number
  priceCurrency: string
}

export interface PublicationAdvisorOption {
  id: string
  name: string
}

export interface PropertyPublicationOptions {
  properties: PublicationPropertyOption[]
  advisors: PublicationAdvisorOption[]
}

export interface PropertyPublicationInput {
  propertyId?: string
  advisorId?: string
  slug: string
  publicTitle: string
  publicDescription?: string
  priceAmount: number
  priceCurrency: string
  municipality: string
  neighborhood: string
  approximateLatitude: number
  approximateLongitude: number
}

// Facturacion
export interface BillingPricing {
  basePlanInCents: number
  additionalAdvisorInCents: number
  setupInCents: number
  whatsAppInCents: number
  portalInCents: number
  advancedReportsInCents: number
  maximumAdditionalAdvisors: number
}

export interface CreateCheckoutSessionInput {
  companyName: string
  taxId: string
  customerEmail: string
  additionalAdvisors: number
  includeSetup: boolean
  includeWhatsApp: boolean
  includePortal: boolean
  includeAdvancedReports: boolean
}

export interface PaymentCheckoutData {
  checkoutUrl: string
  publicKey: string
  currency: string
  amountInCents: number
  reference: string
  integritySignature: string
  redirectUrl: string
  customerEmail: string
}

export interface CreateCheckoutSessionResponse {
  orderId: string
  checkout: PaymentCheckoutData
}

export interface PaymentOrderStatus {
  reference: string
  amountInCents: number
  currency: string
  status: "Pending" | "Approved" | "Declined" | "Voided" | "Error"
}

// Analitica web
export interface WebAnalyticsStep {
  event: "page_view" | "search" | "favorite" | "form_started" | "conversion"
  uniqueSessions: number
  totalEvents: number
  rateFromVisits: number
}

export interface WebAnalyticsFunnel {
  from: string
  to: string
  steps: WebAnalyticsStep[]
}

export type LeadDistributionRule = "LeastOpenLeads" | "RoundRobin"

// Politica
export interface ObtenerPoliticaActivaResponse {
  id: string
  version: string
  textoCompleto: string
  fechaVigencia: string
}

// Datos Personales
export interface ConsultarDatosLeadResponse {
  id: string
  nombre: string
  email: { value: string }
  telefono: { value: string } | null
  autorizacionDatos: boolean
  consentimiento?: {
    id: string
    fecha: string
    versionPolitica: string
    metodo: string
  }
}
