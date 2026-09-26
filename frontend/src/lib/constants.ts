export const ETAPAS_PIPELINE = {
  VENTA: [
    "Lead nuevo",
    "Contactado",
    "Cualificado",
    "Visita agendada",
    "Visita realizada",
    "Oferta/Negociacion",
    "Bajo contrato",
    "Cerrado ganado",
    "Cerrado perdido",
  ],
  ARRIENDO: [
    "Lead nuevo",
    "Contactado",
    "Cualificado",
    "Visita agendada",
    "Visita realizada",
    "Postulacion",
    "Aprobacion propietario",
    "Cerrado ganado",
    "Cerrado perdido",
  ],
} as const

export const ETIQUETAS_PIPELINE: Record<string, string> = {
  "Lead nuevo": "Oportunidad nueva",
  Contactado: "Contactado",
  Cualificado: "Cualificado",
  "Visita agendada": "Visita agendada",
  "Visita realizada": "Visita realizada",
  "Oferta/Negociacion": "Oferta",
  "Bajo contrato": "Bajo contrato",
  Postulacion: "Postulación",
  "Aprobacion propietario": "Aprobación",
  "Cerrado ganado": "Cerrado ganado",
  "Cerrado perdido": "Cerrado perdido",
}

export const TIPO_OPERACION = {
  VENTA: "Venta",
  ARRIENDO: "Arriendo",
} as const

export const TIPOS_INMUEBLE = [
  "Apartamento",
  "Casa",
  "Local",
  "Lote",
  "Oficina",
  "Bodega",
  "Finca",
] as const

export const MODALIDADES_INMUEBLE = [
  "Venta",
  "Arriendo",
  "VentaYArriendo",
] as const

export const ESTADOS_INMUEBLE = [
  "Disponible",
  "Reservado",
  "Vendido",
  "Arrendado",
  "Inactivo",
] as const

export const TIPOS_INTERACCION = [
  "Llamada",
  "Email",
  "WhatsApp",
  "Visita",
  "Reunión",
] as const

export const ESTADOS_VISITA = [
  "Programada",
  "Realizada",
  "Cancelada",
] as const

export const NAV_ITEMS = [
  { title: "Panel", href: "/dashboard", icon: "LayoutDashboard" },
  { title: "Pipeline", href: "/pipeline", icon: "Activity" },
  { title: "Oportunidades", href: "/leads", icon: "Users" },
  { title: "Inmuebles", href: "/inmuebles", icon: "Home" },
  { title: "Calendario", href: "/calendario", icon: "Calendar" },
  { title: "Chat", href: "/chat", icon: "MessageSquare" },
  { title: "Reportes", href: "/reportes", icon: "BarChart3" },
  { title: "Alertas", href: "/alertas", icon: "Bell" },
  { title: "Plan y facturación", href: "/facturacion", icon: "CreditCard", adminOnly: true },
  { title: "Configuración", href: "/config", icon: "Settings" },
] as const
