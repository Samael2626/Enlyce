---
title: "Auditoría local de seguridad ENLYCE"
date: 2026-09-26
tags: [enlyce, seguridad, auditoria, owasp, web, api]
status: abierto
---

# Auditoría local de seguridad — ENLYCE

Relacionado: [[Enlyce-MOC]] · [[Checklist-Producto-Web-Enlyce]] · [[Arquitectura-Web-API-Enlyce]]

## Veredicto

ENLYCE no debe exponerse a Internet todavía. La autenticación y el control de acceso de las rutas privadas están razonablemente construidos, pero el flujo público de recontacto permite consultar datos de una oportunidad y modificar su información de propietario sin autenticación.

## Hallazgos

### Crítico — enumeración y modificación no autorizada de oportunidades

**Estado:** reproducido contra la API local con datos sintéticos.

1. `POST /api/leads` acepta un correo ya registrado.
2. La respuesta del recontacto entrega el identificador, nombre, correo, estado, fecha y metadatos de la oportunidad existente.
3. `PUT /api/leads/{id}/owner-details` es público y considera suficientes el identificador y ese mismo correo.
4. Un tercero que conozca el correo puede recuperar el identificador y sobrescribir detalles del propietario.

**Impacto:** exposición de datos personales, alteración de registros comerciales y contaminación del CRM. Corresponde a una falla de autorización sobre objetos, similar a BOLA/IDOR.

**Corrección exigida:** el recontacto público debe responder de forma opaca, sin identificadores ni datos existentes. El enriquecimiento necesita un token de continuación aleatorio, de un solo uso, corto y ligado a la captura recién creada; no debe usar correo + GUID como prueba de identidad.

### Alta — fuerza bruta y abuso sin límites

**Estado:** reproducido. Quince intentos consecutivos de login inválido respondieron `401`; ninguno respondió `429`.

No existe limitación específica para login, creación pública de oportunidades ni enriquecimiento de propietarios. Esto permite fuerza bruta, spam, crecimiento artificial de consentimientos y abuso combinado con el hallazgo crítico.

**Corrección exigida:** límites por IP y por identidad normalizada, respuesta `429`, ventana progresiva y telemetría. El webhook debe conservar la validación criptográfica y recibir protección de volumen independiente.

### Media — cabeceras defensivas ausentes

**Estado:** confirmado manualmente y mediante OWASP ZAP sobre el build de producción de Next.js.

Faltan CSP, protección anti-clickjacking, `X-Content-Type-Options`, `Referrer-Policy` y `Permissions-Policy`. Next.js además publica `X-Powered-By`; Kestrel publica `Server`.

**Impacto:** mayor superficie frente a XSS futuro, clickjacking y reconocimiento tecnológico.

### Media — transporte y sesión incompletos para producción

- `RequireHttpsMetadata` está desactivado sin condición de ambiente.
- No se observó redirección HTTPS ni HSTS en la API.
- El login crea una cookie `HttpOnly`, `Secure` y `SameSite=Strict`, pero también devuelve el JWT en el JSON aunque el frontend no lo utiliza.
- Cerrar sesión elimina la cookie, pero no revoca un JWT ya copiado. La vigencia configurada es de ocho horas.

**Corrección exigida:** HTTPS/HSTS en producción, no devolver el token al navegador, reducir vigencia y definir revocación o rotación según el modelo de sesión.

### Baja — huella tecnológica

Las respuestas revelan Next.js y Kestrel. No entrega acceso por sí sola, pero facilita reconocimiento automatizado.

## Controles que sí funcionaron

- Rutas privadas representativas devolvieron `401` sin sesión.
- CORS no autorizó el origen malicioso probado y sí reconoció el origen local permitido.
- Cookie de autenticación con `HttpOnly`, `Secure` y `SameSite=Strict`.
- Firma del webhook Wompi comparada en tiempo constante.
- Carga de imágenes autenticada, limitada y decodificada antes de persistir.
- Sin secretos reales encontrados en archivos versionables.
- `npm audit`: cero vulnerabilidades conocidas en CRM y sitio público.
- Auditoría NuGet: cero paquetes vulnerables conocidos.
- Payload XSS reflejado por filtros: codificado, no ejecutable.
- Payload de inyección SQL en catálogo: respondió normalmente; EF Core parametriza la consulta.

## Herramientas y evidencia

- Revisión manual de autenticación, autorización, CORS, endpoints públicos, medios y webhook.
- Pruebas HTTP contra `localhost:5019` y build Next.js en `localhost:3100`.
- OWASP ZAP Baseline: 602 URL, 0 fallos altos, 10 familias de advertencias.
- Suite .NET: 400 de 400 pruebas superadas (`--no-build --no-restore`).
- Reportes ZAP locales en `security-reports/`; son artefactos de diagnóstico, no producto final.

## Orden obligatorio

1. Cerrar la fuga y modificación del recontacto.
2. Agregar rate limiting a login y captación pública.
3. Añadir cabeceras, HTTPS/HSTS y endurecer sesión.
4. Crear pruebas de regresión ofensivas para cada corrección.
5. Repetir ZAP y pruebas manuales antes de staging público.

