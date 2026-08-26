# -*- coding: utf-8 -*-
"""Aplica acentos a texto legible por humanos (documentos y presentacion)."""
import re

PARES = {
    "Medellin": "Medellín",
    "informacion": "información", "autorizacion": "autorización",
    "politica": "política", "politicas": "políticas",
    "auditoria": "auditoría", "interaccion": "interacción",
    "interacciones": "interacciones", "transicion": "transición",
    "asignacion": "asignación", "captacion": "captación",
    "gestion": "gestión", "actualizacion": "actualización",
    "aplicacion": "aplicación", "ningun": "ningún",
    "telefono": "teléfono", "codigo": "código", "area": "área",
    "logica": "lógica", "unico": "único", "unica": "única",
    "economica": "económica", "academico": "académico", "academica": "académica",
    "automatica": "automática", "automatico": "automático",
    "organizacion": "organización", "contrasena": "contraseña",
    "contrasenas": "contraseñas", "dueno": "dueño", "duenos": "dueños",
    "espanol": "español", "pequenas": "pequeñas", "pequeno": "pequeño",
    "pequenos": "pequeños", "diseno": "diseño", "disenado": "diseñado",
    "disenada": "diseñada", "mas": "más", "dias": "días", "segun": "según",
    "tambien": "también", "ademas": "además", "asi": "así",
    "despues": "después", "numero": "número", "metrica": "métrica",
    "metricas": "métricas", "tecnico": "técnico", "tecnica": "técnica",
    "tecnicas": "técnicas", "rapido": "rápido", "rapida": "rápida",
    "facil": "fácil", "version": "versión", "opcion": "opción",
    "seccion": "sección", "conversion": "conversión", "evolucion": "evolución",
    "implantacion": "implantación", "migracion": "migración",
    "capacitacion": "capacitación", "suscripcion": "suscripción",
    "integracion": "integración", "notificacion": "notificación",
    "configuracion": "configuración", "administracion": "administración",
    "autenticacion": "autenticación", "restriccion": "restricción",
    "descripcion": "descripción", "conclusion": "conclusión",
    "solucion": "solución", "presentacion": "presentación",
    "demostracion": "demostración", "reunion": "reunión", "sesion": "sesión",
    "razon": "razón", "atencion": "atención", "relacion": "relación",
    "direccion": "dirección", "operacion": "operación",
    "documentacion": "documentación", "validacion": "validación",
    "produccion": "producción", "seleccion": "selección",
    "estan": "están", "sera": "será", "seran": "serán",
    "compania": "compañía", "franquicia": "franquicia",
    "anos": "años", "manana": "mañana", "sanchez": "sanchez",
    "vitrina": "vitrina", "credito": "crédito", "movil": "móvil",
    "hexagonal": "hexagonal", "estandar": "estándar",
    "trafico": "tráfico", "practica": "práctica", "practico": "práctico",
    "generacion": "generación", "creacion": "creación",
    "revocacion": "revocación", "supresion": "supresión",
    "rectificacion": "rectificación", "finalidad": "finalidad",
    "linea": "línea", "maximo": "máximo", "minimo": "mínimo",
    "ultimo": "último", "ultima": "última", "proximo": "próximo",
    "proxima": "próxima", "critico": "crítico", "critica": "crítica",
    "publico": "público", "publica": "pública", "publicas": "públicas",
    "basico": "básico", "basica": "básica",
    "medellin": "Medellín", "continuacion": "continuación",
    "calculo": "cálculo", "catalogo": "catálogo",
    "electronico": "electrónico", "electronica": "electrónica",
    "peticion": "petición", "fisicamente": "físicamente",
    "modulo": "módulo", "modulos": "módulos",
    "ingenieria": "ingeniería", "raiz": "raíz",
    "negociacion": "negociación", "tecnologia": "tecnología",
    "analisis": "análisis", "exito": "éxito", "asesoria": "asesoría",
    "ejecucion": "ejecución", "definicion": "definición",
    "decision": "decisión", "vision": "visión", "mision": "misión",
    "sancion": "sanción", "situacion": "situación",
    "proteccion": "protección", "adopcion": "adopción",
    "friccion": "fricción", "prueba": "prueba", "duenos": "dueños",
    "escalable": "escalable", "portatil": "portátil",
    "estadistica": "estadística", "grafico": "gráfico",
    "graficos": "gráficos", "periodo": "período",
    "accion": "acción", "comunicacion": "comunicación",
    "conversacion": "conversación", "estimacion": "estimación",
    "facturacion": "facturación", "fundacion": "fundación",
    "interrupcion": "interrupción", "intervencion": "intervención",
    "proyeccion": "proyección", "perdida": "pérdida",
    "nucleo": "núcleo", "asesoria": "asesoría", "energia": "energía",
}

PARES = {k.lower(): v for k, v in PARES.items()}

def _rep(m):
    w = m.group(0)
    low = w.lower()
    if low not in PARES:
        return w
    fix = PARES[low]
    if w[0].isupper():
        fix = fix[0].upper() + fix[1:]
    if w.isupper():
        fix = fix.upper()
    return fix

_RX = re.compile(r"[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+")

def fix(t):
    if not isinstance(t, str):
        return t
    return _RX.sub(_rep, t)
