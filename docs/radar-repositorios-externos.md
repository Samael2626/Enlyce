# Radar de repositorios externos para Enlyce

**Corte:** 14 de octubre de 2026  
**Alcance:** producto Enlyce completo: CRM inmobiliario, Wasi, web, integraciones, finanzas y operacion.  
**Uso:** aprender patrones y evaluar componentes. Un repositorio no se incorpora por aparecer aqui: revisar codigo, licencia, seguridad, actividad, dependencias y compatibilidad antes de adoptarlo.

## Mapa por necesidad

| Necesidad de Enlyce | Repositorios para estudiar | Uso previsto y cautela |
|---|---|---|
| Modelo CRM, vistas y flujos configurables | [Twenty](https://github.com/twentyhq/twenty), [Frappe CRM](https://github.com/frappe/crm) | Referencia de objetos, vistas, actividades, embudos y extensibilidad. Ambos usan AGPL-3.0; estudiar ideas, no copiar codigo sin resolver licencia. Stack distinto. |
| Flujo inmobiliario integral | [Zala CRM](https://github.com/Jonathanz4344/BigRealEstate), [OpenGate](https://github.com/CodeVoyager15/OpenGate), [Open House CRM](https://github.com/lucsedirae/open-house-crm) | Buscar patrones para propiedades, equipos, campañas y captacion. Zala declara CSV y Google; es proyecto academico. OpenGate esta sin actividad reciente. Open House CRM es antiguo y GPL-3.0. Verificar licencias antes de reutilizar cualquier codigo. |
| Leads, permisos por asesor y visitas | [EstatePulse](https://github.com/Rakeinn/RealEstateCRM) | Ejemplo en ASP.NET Core con aislamiento por agente, seguimiento y visitas; README declara MIT, pero no se encontro archivo LICENSE en la ruta esperada. Incluye credenciales de demo en README: no ejecutarlo ni reutilizarlas. Referencia de ideas solamente hasta auditar el repo. |
| Importacion CSV con vista previa y mapeo | [react-csv-importer](https://github.com/beamworks/react-csv-importer), [CSV Importer de sadmann7](https://github.com/sadmann7/csv-importer) | Referencias UI para cargar, previsualizar, mapear columnas y procesar por partes. Revisar mantenimiento, licencia y compatibilidad con React/Vite antes de dependencia. El segundo documenta esas funciones; usa Next.js y UploadThing. |
| Lectura y escritura XLS/XLSX | [ExcelDataReader](https://github.com/ExcelDataReader/ExcelDataReader), [ClosedXML](https://github.com/ClosedXML/ClosedXML) | Ambos MIT y activos. ExcelDataReader lee formatos Excel antiguos y actuales; ClosedXML manipula XLSX/XLSM. Evaluar limites de archivo, formulas, macros, memoria y datos invalidos para importacion masiva. |
| Agenda por equipo | [ilamy-calendar](https://github.com/kcsujeet/ilamy-calendar) | MIT; ofrece vistas dia/semana/mes, recursos, arrastrar y soltar y recurrencia. Candidato para prototipo UI, falta validar accesibilidad, zona horaria, tamaño y encaje con la agenda existente. |
| Sitio inmobiliario y CMS | [Piranha CMS](https://github.com/PiranhaCMS/piranha.core), [Orchard Core](https://github.com/OrchardCMS/OrchardCore) | Piranha es MIT; Orchard Core es BSD-3-Clause y modular/multiempresa. Comparar editor, dominios, SEO, formularios y catalogo; no reemplazar el sitio propio sin prueba de costo y encaje. |
| Procesos en segundo plano | [Hangfire](https://github.com/HangfireIO/Hangfire) | Referencia para importaciones, sincronizaciones, reintentos y tareas programadas .NET. Revisar licencia Community/Pro, persistencia, operacion y fallos antes de elegirlo. |
| Correo saliente y registro de actividad | [MailKit](https://github.com/jstedfast/MailKit) | MIT; cliente SMTP/IMAP multiplataforma .NET. Evaluar solo transporte; Enlyce necesita ademas consentimiento, plantillas, idempotencia y eventos de entrega. |
| ERP y procesos financieros inmobiliarios | [ERPNext](https://github.com/frappe/erpnext) | Referencia amplia para contratos, pagos, cartera y contabilidad. GPL-3.0; no copiar codigo sin resolver licencia. Sus reglas no sustituyen validacion contable colombiana ni DIAN. |
| Red entre inmobiliarias | [BrokerOS](https://github.com/trybrokeros/BrokerOS), [Zala CRM](https://github.com/Jonathanz4344/BigRealEstate) | Inspeccionar invitaciones, equipos, colaboracion y ofertas compartidas. BrokerOS es muy reciente: validar madurez, licencia y seguridad antes de darle peso. |
| Mapa e inventario geoespacial | [Leaflet](https://github.com/Leaflet/Leaflet) | Enlyce ya usa Leaflet en Radar. Revisar solo para funcionalidades de mapa, capas, marcadores y rendimiento; no duplicar la libreria sin necesidad. |
| Auth, API, persistencia y componentes web | [ASP.NET Core](https://github.com/dotnet/aspnetcore), [React](https://github.com/facebook/react), [Vite](https://github.com/vitejs/vite), [EF Core](https://github.com/dotnet/efcore) | Fuentes principales para el stack actual. Primero documentacion oficial y ejemplos versionados; evitar dependencias de terceros para capacidades ya cubiertas por el framework. |
| Seguridad de aplicacion | [OWASP Cheat Sheet Series](https://github.com/OWASP/CheatSheetSeries), [ASP.NET Core](https://github.com/dotnet/aspnetcore) | Referencias para sesiones, autorizacion, cargas de archivos, cabeceras y manejo de secretos. Aplicar contra los flujos reales de Enlyce; confirmar version y licencias antes de usar material/codigo. |
| Observabilidad y operacion | [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet), [MailKit](https://github.com/jstedfast/MailKit) | OpenTelemetry para trazas/metricas/logs; MailKit para transporte de correo. Evaluar exportadores y servicios compatibles con la infraestructura actual antes de incorporar. |
| App movil | [Expo](https://github.com/expo/expo) | MIT; candidato si se decide app React Native. Enlyce no tiene app movil identificada; primero confirmar prioridades, autenticacion movil y modo offline. |

## Necesidades de producto incluidas

Este radar alimenta el plan completo, no reemplaza el checklist Wasi:

- **CRM:** clientes independientes, demandas, embudos, actividades, llamadas/correos, permisos, historial, duplicados e importacion/exportacion.
- **Inmuebles:** ficha, inventario, mapa, busqueda, edicion, vinculaciones, cruces, archivos, publicaciones y sincronizacion con fuentes/portales.
- **Red inmobiliaria:** empresas, usuarios, invitaciones, acuerdos, comisiones, visibilidad de inventario y trazabilidad entre organizaciones.
- **Agenda y comunicacion:** tareas, visitas, calendario de equipo, correo, WhatsApp oficial, alertas y registro de entrega/consentimiento.
- **Web y captacion:** catalogo, sitio por empresa, dominios, formularios, SEO, contenidos, analitica y atribucion de origen.
- **Reportes y operacion:** metricas conciliables, exportacion, auditoria, seguridad, respaldo/restauracion, observabilidad, pagos y administracion de cuenta.
- **Finanzas inmobiliarias posteriores al CRM:** contratos de arriendo, canones, pagos, cartera, liquidacion a propietarios, contabilidad y facturacion electronica con validacion profesional/DIAN.
- **Movil y colaboracion:** experiencia movil, chat, notificaciones y soporte cuando el alcance del negocio lo confirme.

## Decisiones de uso

1. La prioridad funcional acordada sigue siendo **Importar clientes**; este radar abarca lo que viene antes y despues.
2. Preferir paquetes pequeños compatibles con ASP.NET Core 10, React/Vite y PostgreSQL. No migrar Enlyce a otro framework para obtener una sola funcion.
3. Antes de incorporar codigo: inspeccionar LICENSE real, dependencias, actividad, seguridad, pruebas, arquitectura y costo de operar. README o etiqueta de GitHub no bastan como aprobacion de licencia.
4. No ejecutar proyectos ajenos con credenciales, datos reales o acceso a servicios; primero leer codigo y configuracion.
5. Actualizar este archivo cuando cambie la prioridad o aparezca una referencia mejor. Registrar que modulo resuelve y que evidencia respalda la eleccion.

## Evidencia de auditoria de Enlyce

- Auditoria backend: importacion de clientes y red multiempresa faltan; cruces, agenda y reportes son parciales; el historial de cambios de vinculos cliente-inmueble falta.
- Auditoria frontend: importacion de clientes no existe en UI; red, usuarios, foro y chat real faltan; clientes, inmuebles, agenda y reportes son parciales.
- Graphify encontro modulos existentes de importacion Wasi, contactos, demandas, tareas, visitas, analitica, exportacion CSV, autenticacion, pagos Wompi, publicaciones y analitica web.
- Checklist funcional Wasi: [checklist-paridad-wasi-enlyce.md](checklist-paridad-wasi-enlyce.md). Hoja de ruta CRM y finanzas: [roadmap-crm-inmobiliario.md](roadmap-crm-inmobiliario.md).

