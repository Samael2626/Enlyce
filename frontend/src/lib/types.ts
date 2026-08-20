// Auth
export interface LoginRequest {
  correo: string
  password: string
}

export interface LoginResponse {
  token: string
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
}

// Pipeline
export interface PipelineResponse {
  etapas: EtapaPipelineDto[]
  leads: LeadDto[]
  total: number
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
