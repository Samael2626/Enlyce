---
title: "Checklist de producto y web ENLYCE"
date: 2026-09-26
tags: [enlyce, checklist, roadmap, website, crm]
status: activo
---

# Checklist de producto y web — ENLYCE

Relacionado: [[Enlyce-MOC]] · [[Plan-Web-Publica-LYC]] · [[Checklist-Enlace-CRM-Web]]

**Corte verificado:** 27 de septiembre de 2026
**Alcance:** CRM privado, web pública, captación, publicaciones y facturación.

## Terminado

### CRM y núcleo comercial

- [x] Autenticación JWT mediante cookie HttpOnly y roles Administrador/Asesor.
- [x] Oportunidades con pipeline, asignación, interacciones, visitas y alertas.
- [x] Gestión de inmuebles, propietarios y asesores.
- [x] Consentimiento, política versionada, consulta, supresión y auditoría para Ley 1581.
- [x] Terminología visible cambiada de “lead” a “oportunidad”.
- [x] Administración de publicaciones: borrador, edición, fotos, publicación, pausa y retiro.
- [x] RBAC de publicaciones: administrador global y asesor limitado a sus registros.

### Web pública

- [x] Next.js 16 con portada, catálogo, ficha, zonas, favoritos, propietarios, contacto, privacidad y nosotros.
- [x] Catálogo con filtros persistidos en la URL, orden, paginación y estados vacíos.
- [x] Fichas con galería, información comercial y mapa aproximado sin revelar la dirección exacta.
- [x] Favoritos locales sin exigir cuenta.
- [x] Formularios conectados a `POST /api/leads` y convertidos en oportunidades del CRM.
- [x] Detección de recontactos sin duplicar una misma oportunidad.
- [x] Consentimiento obligatorio, canal web, IP y campaña UTM.
- [x] Página de propietarios rediseñada con servicios, proceso, fotografía real y formulario comercial.
- [x] Selección de vender, arrendar, administrar o valorar conservada al llegar al formulario.
- [x] Captura progresiva de propietarios: la oportunidad se guarda con datos mínimos antes de solicitar información adicional.
- [x] Enriquecimiento opcional con tipo de inmueble, ciudad, barrio, precio esperado, mensaje y canal preferido.
- [x] Directorio territorial ampliado a 25 municipios y barrios, con inventario filtrado, zonas relacionadas y captación contextual.
- [x] Alias `/propietario` redirige permanentemente a `/propietarios`.
- [x] Enlaces flotantes de Instagram, Facebook y WhatsApp.
- [x] SEO base: metadatos, canonical, Open Graph, sitemap, robots y páginas por zona.
- [x] Página 404 y estados de carga/error.

### Facturación ENLYCE SaaS

- [x] Cotizador del plan base, instalación y complementos.
- [x] Orden de pago persistida antes de abrir el checkout.
- [x] Integración de checkout y webhook firmados para Wompi/PSE.
- [x] Pantalla de retorno con estados pendiente, aprobado, rechazado, anulado y error.
- [x] Validaciones de monto, moneda, ambiente, firma e idempotencia del evento.

## Falta antes de una demo comercial seria

### Prioridad P0 — bloquea producción

- [x] Corregir la fuga crítica del recontacto: no revela la oportunidad existente y la continuación usa un token temporal de un solo uso.
- [x] Hacer atómica la deduplicación: 30 capturas simultáneas producen un solo registro activo.
- [x] Proteger login, captación pública, enriquecimiento y webhook con rate limiting independiente.
- [x] Limitar tamaño de cuerpos y campos públicos, validar filtros y reducir amplificación de logs.
- [x] Añadir CSP, anti-clickjacking, `nosniff`, `Referrer-Policy`, `Permissions-Policy`, HTTPS y HSTS.
- [x] Dejar de devolver el JWT en el JSON, reducir su vigencia y revocarlo al cerrar sesión.
- [x] Ejecutar regresión ofensiva local y ZAP API activo sin fallos confirmados altos o medios.
- [x] Reconstruir la web de producción local en `3100` y confirmar sus cabeceras con Nmap y Burp.
- [x] Limitar PostgreSQL 17 nativo a loopback y comprobar que `5432` no responde por la IP LAN.
- [x] Limitar las webs locales `3000/3100` y PostgreSQL Docker `5436` a `127.0.0.1`.
- [ ] Bloquear `TRACE` en el proxy de staging y verificar respuesta `405`; localmente no reflejó la solicitud.
- [ ] Crear staging de ENLYCE y repetir migraciones, smoke y ZAP sobre la configuración remota.
- [ ] Configurar secretos fuera del repositorio, backups y recuperación.
- [ ] Revisión jurídica final de política de privacidad y textos de consentimiento.
- [ ] Smoke visual manual completo con API y PostgreSQL reales.
- [ ] Pruebas E2E del catálogo, ficha, favoritos, contacto, propietarios y pago.

### Prioridad P1 — estándar comercial

- [x] Ampliar la captación de propietarios con ciudad, barrio, tipo de inmueble y mensaje.
- [x] Guardar esos datos en campos estructurados del CRM, no incrustados en `Fuente`.
- [x] Mostrar en la ficha de la oportunidad la ruta, campaña, publicación y servicio de origen.
- [x] Crear brochure comercial descargable y versión web tipo libro: 10 páginas A5, portada rígida, doble página, arrastre, sombras, pantalla completa y navegación por teclado.
- [ ] Sustituir las fotografías editoriales temporales del brochure por fotografías autorizadas del inventario real de L&C.
- [x] Añadir analítica de embudo: visita, búsqueda, favorito, formulario iniciado y conversión.
- [ ] Añadir datos estructurados JSON-LD para organización e inmuebles.
- [ ] Auditoría completa WCAG AA: teclado, foco, labels, contraste y lector de pantalla.
- [ ] Medir y corregir Core Web Vitals: LCP, CLS e INP.
- [ ] Correo de confirmación real y notificación inmediata al asesor responsable.
- [ ] Trazabilidad de cambios y estado de la suscripción después del pago aprobado.

### Prioridad P2 — crecimiento

- [ ] Alertas de nuevas propiedades y cambios de precio.
- [ ] Comparador de inmuebles.
- [ ] Búsquedas guardadas.
- [ ] Portal del propietario con estado de publicación y actividad comercial.
- [ ] Integración oficial de WhatsApp con conversaciones dentro del CRM.
- [ ] Reportes PDF/Excel y métricas de conversión.
- [ ] Exportación CSV de oportunidades.
- [ ] Automatización de seguimiento y SLA de respuesta.

## Aplazado para el cierre

- [ ] Cargar inventario, fotografías, textos, teléfonos y enlaces sociales reales de L&C.
- [ ] Completar credenciales de Wompi sandbox y ejecutar un pago PSE de extremo a extremo.
- [ ] Definir dominio, hosting, PostgreSQL, almacenamiento de medios, correo y CDN.

## Bloqueos externos

- [ ] L&C debe entregar fotografías e inventario publicable con autorización.
- [ ] L&C debe confirmar WhatsApp, Instagram, Facebook, correo, teléfono y horarios definitivos.
- [ ] Wompi debe suministrar llaves sandbox/producción y secreto de eventos.
- [ ] Responsable jurídico debe aprobar política, autorización y términos comerciales.

## Próxima secuencia recomendada

1. Crear E2E de los recorridos comerciales y de las regresiones de autorización.
2. Completar accesibilidad, datos estructurados y rendimiento.
3. Crear staging y validar secretos, backups restaurables, migraciones y ZAP remoto.
4. Al cierre: material real, Wompi sandbox y despliegue definitivo.

## Cobertura verificada al 9 de octubre de 2026

- `node website/funcional.test.mjs`: 26 pruebas de logica del sitio legacy, todas verdes.
- `PublicCatalogEndpointTests` y `CrmJourneyE2ETests` son pruebas HTTP con `WebApplicationFactory` + SQLite; no recorren navegador.
- El sitio Next no tiene runner E2E de navegador instalado. Pago tampoco tiene E2E; Wompi queda fuera sin credenciales sandbox.
- Siguen pendientes el smoke visual con PostgreSQL y las E2E de catalogo, ficha, favoritos, contacto, propietarios y pago.
