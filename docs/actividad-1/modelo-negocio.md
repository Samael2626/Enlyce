# Enlyce — Documento de Modelo de Negocio

**Proyecto:** Enlyce — CRM Inmobiliario  
**Cliente ancla:** L&C Propiedad Raiz (LYC)  
**Fecha:** Agosto 2026  
**Curso:** Ingenieria de Software  

---

## 1. Descripcion del Problema

Las inmobiliarias pequenas y medianas en Colombia (1-8 asesores, menos de 50 inmuebles activos) operan sin herramientas de gestion comercial adecuadas. El caso de L&C Propiedad Raiz (LYC) es representativo:

- **Sin CRM multi-agente:** LYC usa Wasi (plan gratuito, 1 usuario). No hay manera de asignar leads a asesores especificos ni medir productividad individual.
- **Sin pipeline de ventas:** No existe un flujo estructurado de seguimiento (contacto -> visita -> propuesta -> cierre). Los leads se pierden entre WhatsApp, llamadas y notas dispersas.
- **Sin asignacion por asesor:** Cuando un cliente escribe por WhatsApp, cualquier asesor puede responder, generando duplicidad, conflictos y mala experiencia.
- **Cumplimiento normativo manual:** La Ley 1581/2012 de proteccion de datos personales exige autorizacion previa, politica de tratamiento y registro en el RNBD. LYC no tiene automatizacion de estos procesos.

Los competidores actuales (Wasi, Tokko, Witei) no resuelven这些问题 de forma integral para el segmento de micro-inmobiliarias colombianas.

---

## 2. Solucion Propuesta

**Enlyce** es un CRM inmobiliario web diseno especifico para inmobiliarias colombianas pequenas. Proporciona:

### Funcionalidades Principales

| Modulo | Descripcion |
|--------|-------------|
| **Pipeline de ventas** | Tablero Kanban con etapas configurables (Contacto, Visita, Propuesta, Negociacion, Cierre) |
| **Gestion de leads** | Registro centralizado con fuente, historial de interacciones y estado |
| **Asignacion por asesor** | Distribucion automatica o manual de leads entre el equipo |
| **Dashboard de productividad** | Metricas por asesor: leads atendidos, conversiones, tiempo de respuesta |
| **Cumplimiento Ley 1581** | Flujo automatizado de autorizacion de tratamiento, politica visible, registro RNBD |
| **Gestion de inmuebles** | Catalogo de propiedades con fotos, precio, ubicacion y estado |
| **Notificaciones** | Alertas por lead nuevo, seguimiento pendiente, cambio de estado |

### Diferenciadores Tecnicos

- **Listo en 1 dia:** Onboarding simplificado. La inmobiliaria opera en menos de 24 horas.
- **Multi-espanol:** Interfaz 100% en espanol colombiano, sin traducciones literales.
- **Multi-agente nativo:** Diseado desde el dia 1 para equipos, no para usuarios individuales.
- **Cumplimiento normativo integrado:** No es un add-on, es parte del flujo core.

---

## 3. Modelo de Ingresos

### Estructura de Precios

| Componente | Precio (COP/mes) | Descripcion |
|------------|-------------------|-------------|
| **Suscripcion base (equipo)** | $89.000 | Hasta 3 asesores. Incluye pipeline, leads, dashboard basico |
| **Asesor adicional** | $19.000/cada | Por asesor extra (maximo 8 total) |
| **Setup inicial** | $150.000 (unico) | Configuracion inicial, migracion de datos, capacitacion (2h) |
| **Add-on: WhatsApp API** | $45.000 | Integracion con WhatsApp Business API (envio/recepcion) |
| **Add-on: Portal web inmobiliario** | $35.000 | Sitio web publico con catalogo de propiedades |
| **Add-on: Reportes avanzados** | $25.000 | Exportacion PDF/Excel, metricas historicas, graficos |

### Escenario Tipico (LYC)

| Concepto | Valor |
|----------|-------|
| Suscripcion base (3 asesores) | $89.000 |
| 1 asesor adicional | $19.000 |
| Setup inicial (dividido en 3 meses) | $50.000 |
| **Total mensual estimado** | **$158.000** |

**Comparacion:** Wasi gratis (1 usuario, sin pipeline) vs Tokko $313.000-$667.000/mes vs Enlyce ~$158.000/mes (multi-agente, pipeline, cumplimiento).

### Proyeccion de Ingresos (Semestre 1)

| Mes | Clientes | MRR (COP) | Setup | Total |
|-----|----------|-----------|-------|-------|
| 1 | 1 (LYC) | $158.000 | $150.000 | $308.000 |
| 2 | 2 | $316.000 | $150.000 | $466.000 |
| 3 | 3 | $474.000 | $150.000 | $624.000 |
| 4 | 5 | $790.000 | $300.000 | $1.090.000 |
| 5 | 7 | $1.106.000 | $300.000 | $1.406.000 |
| 6 | 10 | $1.580.000 | $450.000 | $2.030.000 |

---

## 4. Segmento Objetivo

### Perfil del Cliente Ideal (ICP)

| Atributo | Descripcion |
|----------|-------------|
| **Tipo** | Inmobiliaria independiente o franchise pequena |
| **Tamano** | 1-8 asesores comerciales |
| **Inmuebles activos** | Menos de 50 |
| **Ubicacion** | Medellin y area metropolitana (fase 1), Colombia (fase 2) |
| **Herramienta actual** | Wasi gratis, Excel, WhatsApp sin organizar |
| **Facturacion anual** | $100M - $2.000M COP |
| **Dolor principal** | Leads perdidos, cero visibilidad de pipeline, sin cumplimiento Ley 1581 |

### Segmentos Secundarios

1. **Asesores independientes** que quieren escalar a equipo pequeno
2. **Franchises de grandes inmobiliarias** que necesitan autonomia local
3. **Constructoras pequenas** con venta directa de.units nuevos

---

## 5. Ventaja Competitiva

| Factor | Enlyce | Wasi | Tokko | Witei |
|--------|--------|------|-------|-------|
| **Idioma** | Espanol colombiano nativo | Espanol | Espanol | Espanol (neutral) |
| **Multi-agente** | Si (core del diseño) | No (1 usuario) | Si (pero costoso) | Si |
| **Tiempo de setup** | < 24 horas | Inmediato (gratis) | 1-2 semanas | Dias |
| **Cumplimiento Ley 1581** | Integrado | No | Parcial | No |
| **Precio (equipo 4 asesores)** | ~$158.000/mes | Gratis (limitado) | $313k-$667k/mes | EUR 25-65/mes |
| **Pipeline Kanban** | Si | No | Si | Si |
| **Soporte en espanol** | Si (Colombia) | Si | Si | No |

### Ventajas Clave

1. **Natividad cultural:** Disenado para el mercado colombiano, no traducido de otro mercado.
2. **Precio just-right:** Ni gratis (sin valor) ni costoso (inaccesible). El punto medio para micro-inmobiliarias.
3. **Velocidad de adopcion:** Setup en 1 dia reduce la friccion de cambio.
4. **Cumplimiento normativo:** Unica solucion que integra Ley 1581 desde el inicio, no como add-on.
5. **Arquitectura moderna:** ASP.NET Core 10 + Clean Architecture = escalable, mantenible, testeable.

---

## 6. Fuera de Alcance (Semestre 1)

| Elemento | Razon de exclusion |
|----------|-------------------|
| **Pasarela de pagos** | Complejidad regulatoria adicional (Ley 1258/2008, Puente Financiero). Fase 2. |
| **Contratos digitales** | Requiere integracion con firmas digitales (e签宝, DocuSign). Fase 2. |
| **App movil nativa** | React Native o Flutter demanda stack adicional. PWA como alternativa temporal. |
| **Integracion portales** |Ciencuadras, Properati requieren acuerdos comerciales y APIs privadas. Fase 3. |
| **Multi-idioma** | El mercado target es hispanohablante. Inglés/pt-BR en fase futura. |
| **Multi-tenant completo** | Por ahora, una instancia por cliente. Shared infrastructure en fase 2. |

---

## 7. Analisis Competitivo Detallado

### Wasi

- **Modelo:** Freemium. Plan gratis (1 usuario) y planes pagos ($120k COP/mes).
- **Fortalezas:** Gratis, rapido de empezar, app movil, integracion con portales.
- **Debilidades:** Sin pipeline visual, sin multi-agente real, sin cumplimiento Ley 1581, limitado en personalizacion.
- **Target:** Asesores individuales, no equipos.

### Tokko

- **Modelo:** Suscripcion mensual ($313k-$667k COP/mes segun plan).
- **Fortalezas:** Pipeline completo, multi-agente, integraciones, app movil.
- **Debilidades:** Costo elevado para micro-inmobiliarias, setup largo, interfaz compleja.
- **Target:** Inmobiliarias medianas-grandes (10+ asesores).

### Witei

- **Modelo:** Suscripcion EUR 25-65/mes (~$120k-$310k COP).
- **Fortalezas:** Precio competitivo, interfaz moderna, portales.
- **Debilidades:** No esta en Colombia, soporte en espanol limitado, sin cumplimiento Ley 1581 local.
- **Target:** Mercado europeo, expansion.latam incipiente.

### Zoho CRM (alternativa general)

- **Modelo:** $14-$52 USD/usuario/mes.
- **Fortalezas:** Extremely customizable, integraciones masivas, escala.
- **Debilidades:** No especializado en inmobiliario, requiere configuracion pesada, soporte en ingles.
- **Target:** Empresas de cualquier sector que necesiten CRM generico.

---

## 8. Estimacion de Costos Operativos (Semestre 1)

| Categoria | Costo mensual (COP) | Notas |
|-----------|---------------------|-------|
| **Hosting (Azure/AWS)** | $120.000 - $200.000 | App Service basico + PostgreSQL managed |
| **Dominio .com** | $4.000/mes (promedio) | ~$50.000/anual |
| **WhatsApp Business API** | $0 (por mensaje) | Meta cobra por conversacion: ~$0.05-0.15 USD |
| **SSL/TLS** | $0 | Let's Encrypt gratuito |
| **Email transaccional** | $0 - $15.000 | SendGrid free tier (100k emails/mes) |
| **Monitoreo** | $0 - $20.000 | Sentry free tier + Azure Monitor basico |
| **Total estimado** | **$125.000 - $240.000** | Escala con numero de clientes |

### Costos de Desarrollo (Sunk Cost — Estudiante)

| Recurso | Costo |
|---------|-------|
| Tiempo del desarrollador | 6h/semana x 16 semanas = 96h |
| Herramientas | Visual Studio Community (gratis), GitHub (gratis) |
| Licencias | Ninguna requerida (open source stack) |

---

## 9. Roadmap del Semestre (16 semanas)

### Fase 1: Fundacion (Semanas 1-4)

- [ ] Diseno de dominio: entidades Core (Lead, Property, Agent, PipelineStage)
- [ ] Arquitectura Clean: Domain + Application + Infrastructure + Api
- [ ] EF Core + PostgreSQL: esquema inicial, migraciones
- [ ] CRUD basico de leads e inmuebles
- [ ] Autenticacion JWT basica
- [ ] Tests unitarios de dominio

### Fase 2: Pipeline y Asignacion (Semanas 5-8)

- [ ] Pipeline Kanban (arrastre entre etapas)
- [ ] Asignacion de leads a asesores
- [ ] Dashboard basico (leads por asesor, conversion)
- [ ] Filtros y busqueda
- [ ] Tests de integracion

### Fase 3: Cumplimiento y Notificaciones (Semanas 9-12)

- [ ] Flujo de autorizacion Ley 1581 (checkbox + registro)
- [ ] Politica de tratamiento visible
- [ ] Registro RNBD (exportacion de datos)
- [ ] Notificaciones por email (lead nuevo, seguimiento)
- [ ] Frontend React: dashboard + pipeline + leads

### Fase 4: Pulido y Entrega (Semanas 13-16)

- [ ] UI/UX: responsive design, dark mode opcional
- [ ] Performance: indices, cachear queries lentas
- [ ] Documentacion: API docs (Swagger), usuario final
- [ ] Deploy: Azure App Service o Railway
- [ ] Demo con LYC: datos reales, feedback
- [ ] Presentacion final

---

## 10. Metricas de Exito

| Metrica | Target Semestre 1 |
|---------|-------------------|
| **LYC operando en Enlyce** | Si (cliente ancla activo) |
| **Leads registrados en LYC** | 50+ |
| **Tiempo de respuesta promedio** | < 2 horas |
| **Cobertura Ley 1581** | 100% de leads con autorizacion |
| **Tests passing** | > 80% cobertura en Domain |
| **Uptime** | > 99% |

---

*Documento generado como parte de la Actividad 1 del proyecto Enlyce — Ingenieria de Software.*
