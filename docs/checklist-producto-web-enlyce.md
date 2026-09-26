---
title: "Checklist de producto y web ENLYCE"
date: 2026-09-26
tags: [enlyce, checklist, roadmap, website, crm]
status: activo
---

# Checklist de producto y web — ENLYCE

Relacionado: [[Enlyce-MOC]] · [[Plan-Web-Publica-LYC]] · [[Checklist-Enlace-CRM-Web]]

**Corte verificado:** 26 de septiembre de 2026
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

- [ ] Cargar inventario, fotografías, textos, teléfonos y enlaces sociales reales de L&C.
- [ ] Completar credenciales de Wompi sandbox y ejecutar un pago PSE de extremo a extremo.
- [ ] Definir dominio, hosting, PostgreSQL, almacenamiento de medios, correo y CDN.
- [ ] Configurar secretos, CORS, rate limiting, backups y recuperación.
- [ ] Revisión jurídica final de política de privacidad y textos de consentimiento.
- [ ] Smoke visual manual completo con API y PostgreSQL reales.
- [ ] Pruebas E2E del catálogo, ficha, favoritos, contacto, propietarios y pago.

### Prioridad P1 — estándar comercial

- [x] Ampliar la captación de propietarios con ciudad, barrio, tipo de inmueble y mensaje.
- [x] Guardar esos datos en campos estructurados del CRM, no incrustados en `Fuente`.
- [ ] Mostrar en la ficha de la oportunidad la ruta, campaña, publicación y servicio de origen.
- [ ] Crear brochure comercial descargable y versión web tipo flipbook.
- [ ] Añadir analítica de embudo: visita, búsqueda, favorito, formulario iniciado y conversión.
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

## Bloqueos externos

- [ ] L&C debe entregar fotografías e inventario publicable con autorización.
- [ ] L&C debe confirmar WhatsApp, Instagram, Facebook, correo, teléfono y horarios definitivos.
- [ ] Wompi debe suministrar llaves sandbox/producción y secreto de eventos.
- [ ] Responsable jurídico debe aprobar política, autorización y términos comerciales.

## Próxima secuencia recomendada

1. Probar Wompi/PSE en sandbox de extremo a extremo.
2. Sustituir contenido sintético por material real autorizado.
3. Crear E2E de los recorridos que generan dinero.
4. Desplegar staging y medir accesibilidad, rendimiento y conversión.
