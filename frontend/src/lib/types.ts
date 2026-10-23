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
  sourceRoute?: string
  utmCampaign?: string
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
  surname?: string | null
  email: string | null
  canReceiveAutomatedEmail: boolean
  customerType: string | null
  customerTypes: string[]
  customerLabels: string[]
  customerStatus: string
  customerFunnel: string
  address: string | null
  country: string
  department: string | null
  city: string | null
  phone?: string
  createdAt: string
  updatedAt: string
}

export interface PagedItems<T> {
  total: number
  items: T[]
  page?: number
  pageSize?: number
}

export interface PipelineResult {
  etapas: { nombre: string; etiqueta: string; leadCount: number }[]
  leads: LeadDto[]
  total: number
  page: number
  pageSize: number
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

export interface LeadHistoryItem {
  id: string
  type: string
  from: string
  to: string
  actor: string
  reason: string
  occurredAt: string
}

export interface AdvisorOption {
  id: string
  name: string
}

export interface BulkTaskResult {
  results: { id: string; result: string }[]
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
  landline?: string | null
  dateOfBirth?: string | null
  identificationNumber?: string | null
  referredBy?: string | null
  acquisitionMedium?: string | null
  additionalData?: string | null
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
  country: string
  department?: string
  neighborhood?: string
  minimumPrice?: number
  maximumPrice?: number
  currency: string
  minimumArea?: number
  maximumArea?: number
  bedrooms?: number
  bathrooms?: number
  parkingSpaces?: number
  internalFeatures: string[]
  externalFeatures: string[]
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
  country: string
  department?: string
  neighborhood?: string
  price: number
  currency: string
  bedrooms: number
  bathrooms: number
  parkingSpaces: number
  relationshipStatus?: string
}

export interface PropertyDemandMatch {
  demand: CustomerDemand
  contactName: string
  relationshipStatus?: string
}

export interface CreateCustomerDemandInput {
  contactId: string
  leadId?: string
  operation: string
  propertyType?: string
  city: string
  country?: string
  department?: string
  neighborhood?: string
  minimumPrice?: number
  maximumPrice?: number
  currency?: string
  minimumArea?: number
  maximumArea?: number
  bedrooms?: number
  bathrooms?: number
  parkingSpaces?: number
  internalFeatures?: string[]
  externalFeatures?: string[]
  notes?: string
}

export type UpdateCustomerDemandInput = Omit<CreateCustomerDemandInput, "contactId" | "leadId">

export interface CustomerDemandCrossPage {
  total: number
  page: number
  pageSize: number
  items: {
    demand: CustomerDemand
    contactName: string
    contactSurname?: string | null
    customerType?: string | null
    customerStatus: string
    customerFunnel: string
    contactCreatedAt: string
    ownPropertyMatches: number
  }[]
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

export type LeadSlaAlertKind = "FirstResponse" | "Inactivity"

export interface LeadSlaAlert {
  leadId: string
  nombre: string
  email: string
  operationType: "Venta" | "Arriendo"
  sourceKey: string
  stage: string
  kind: LeadSlaAlertKind
  startedAtUtc: string
  dueAtUtc: string
  overdueMinutes: number
}

export interface LeadSlaAlertsResponse {
  alerts: LeadSlaAlert[]
  total: number
}

// Inmuebles
export interface CreateInmuebleRequest {
  nombre: string
  descripcion?: string
  tipo: string
  modalidad: string
  calle: string
  ciudad: string
  pais?: string
  departamento?: string
  barrio?: string
  precio: number
  moneda: string
  metrosCuadrados: number
  habitaciones: number
  banos: number
  parqueaderos: number
  propietarioId: string
  caracteristicasInternas?: string[]
  caracteristicasExternas?: string[]
}

export interface GetInmuebleByIdResponse {
  id: string
  nombre: string
  descripcion: string
  tipo: string
  modalidad: string
  estado: string
  direccion: string
  pais: string
  departamento?: string | null
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
  statusHistory: PaymentOrderStatusChange[]
}

export interface PaymentOrderStatusChange {
  previousStatus: PaymentOrderStatus["status"]
  newStatus: PaymentOrderStatus["status"]
  occurredAtUtc: string
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

export interface InventoryAnalytics {
  totalProperties: number
  byPropertyType: { name: string; count: number }[]
  byOperation: { name: string; count: number }[]
  byTypeAndOperation: { name: string; sale: number; rent: number; transfer: number; total: number }[]
}

export interface RealizedDealReportPage {
  total: number
  page: number
  pageSize: number
  totalsByCurrency: { currency: string; count: number; grossAmount: number; commissionAmount: number }[]
  items: {
    id: string
    propertyName: string
    propertyType: string
    operation: "Sale" | "Rent"
    closedOn: string
    advisorName: string
    clientName: string
    grossAmount: number
    commissionAmount: number
    currency: string
    advisorId: string
    recordedAtUtc: string
  }[]
}

export interface AdvisorPerformanceReport {
  from: string
  through: string
  previousFrom: string
  previousThrough: string
  advisors: {
    advisorId: string
    advisorName: string
    interactions: number
    previousInteractions: number
    visits: number
    previousVisits: number
    sales: number
    previousSales: number
    rentals: number
    previousRentals: number
    newClients: number
    previousNewClients: number
    newProperties: number
    previousNewProperties: number
  }[]
}

export interface AdvisorActivityDetail {
  advisorId: string
  from: string
  through: string
  interactions: { date: string; type: string; summary?: string | null; clientName: string }[]
  visits: { scheduledAt: string; completedAt?: string | null; status: string; feedback?: string | null; clientName: string; propertyName: string }[]
  deals: { closedOn: string; operation: string; propertyName: string; clientName: string; grossAmount: number; commissionAmount: number; currency: string }[]
}

export interface OwnerPerformanceReport {
  from: string
  through: string
  owners: {
    ownerId: string
    ownerName: string
    pageVisits: number
    calls: number
    emails: number
    events: number
    visits: number
    receivedContacts: number
  }[]
}

export interface SentEmailReportPage {
  total: number
  page: number
  pageSize: number
  items: {
    id: string
    attemptedAt: string
    recipientAddress: string
    subject: string
    description: string
    senderName: string
    status: "AcceptedBySmtp" | "Failed" | "NotConfigured"
    failureCode: string | null
  }[]
}

export interface WebPropertyVisitsReport {
  from: string
  through: string
  properties: { publicationId: string; title: string; visits: number }[]
}

export interface CreateRealizedDealInput {
  propertyId: string
  contactId: string
  leadId?: string | null
  advisorId?: string | null
  operation: "Sale" | "Rent"
  grossAmount: number
  commissionAmount: number
  currency: string
  closedOn: string
}

export interface UpdateRealizedDealInput {
  grossAmount: number
  commissionAmount: number
  currency: string
  closedOn: string
}

export type LeadDistributionRule = "LeastOpenLeads" | "RoundRobin"

export interface LeadSlaRule {
  sourceKey: string
  operationType: "*" | "Venta" | "Arriendo"
  firstResponseMinutes: number
  inactivityDays: number | null
  enabled: boolean
  updatedAtUtc: string
}

export type UpsertLeadSlaRule = Omit<LeadSlaRule, "updatedAtUtc">

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
