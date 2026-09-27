---
name: enlyce-cybersecurity
description: Audita, endurece y verifica la seguridad de ENLYCE (ASP.NET Core, React/Next.js y PostgreSQL). Usar ante vulnerabilidades, pentesting autorizado, OWASP, autenticacion, autorizacion, secretos, dependencias, ZAP o preparacion para desplegar.
---

# Ciberseguridad de ENLYCE

Trabaja con evidencia reproducible. Nunca declares que ENLYCE es "seguro" o que "todos los fallos" estan resueltos: separa hallazgos corregidos, pruebas superadas, riesgos residuales y superficies no examinadas.

## Limites

- Ataca solo entornos que Samuel haya puesto explicitamente en alcance. Usa por defecto PostgreSQL desechable y hosts locales.
- Antes de escanear un host remoto, confirma propietario, ambiente y alcance. No ataques produccion, terceros ni una URL inferida.
- No guardes secretos, JWT, cookies, datos personales o payloads sensibles en Git, vault, logs o reportes compartidos.
- Preserva cambios ajenos. Incluye en el commit solo archivos propios y nunca publiques con pruebas rojas.

## Fuentes

- `AGENTS.md` y `CLAUDE.md`: reglas vigentes.
- `docs/seguridad/auditoria-local-2026-09-26.md`: evidencia y riesgo residual.
- `docs/checklist-producto-web-enlyce.md`: estado de salida comercial.
- `tools/security/README.md` y `tools/security/strong-audit.mjs`: regresion ofensiva local.

No uses checkpoints antiguos como estado actual sin contrastarlos con codigo y pruebas.

## Flujo

1. Define actor, activo, entrada, frontera de confianza e impacto.
2. Reproduce el fallo antes de editar cuando sea seguro. Conserva evidencia minima sin datos sensibles.
3. Revisa Domain, Application, Infrastructure, API y clientes. No parches solo la UI.
4. Corrige en la capa que impone la regla y agrega defensa en profundidad para concurrencia, autorizacion o abuso.
5. Agrega una prueba de regresion. En carreras, verifica el estado persistido, no solo el HTTP.
6. Ejecuta suites afectadas, auditorias de dependencias y el harness ofensivo cuando corresponda.
7. Para ataques activos, sigue `tools/security/README.md`, usa una base desechable y limpia procesos y contenedores.
8. Clasifica cada resultado: `corregido y verificado`, `mitigado`, `pendiente` o `no probado`.

## Invariantes

- La captacion publica nunca revela ID, estado ni datos de una oportunidad existente.
- La continuacion usa token aleatorio, temporal, de un solo uso y almacenado como hash; correo + GUID nunca prueban identidad.
- Solo existe una oportunidad activa por `OpportunityKey`, incluso con concurrencia.
- Rutas privadas exigen autenticacion y RBAC por recurso; el Asesor queda limitado a registros autorizados.
- El JWT viaja solo en cookie segura y no vuelve en JSON; logout invalida la version de sesion.
- Login, captacion, continuacion y webhooks tienen limites separados; cuerpos y campos tienen topes.
- Wompi valida firma antes de mutar y procesa eventos idempotentemente.
- El catalogo no expone propietario, direccion exacta ni campos internos.
- Archivos requieren limite, decodificacion real, tipo permitido y nombre controlado por servidor.
- Produccion exige HTTPS/HSTS, CSP, anti-clickjacking, `nosniff` y secretos externos.

## Verificacion

- Autenticacion: fuerza bruta, token alterado/sin firma, cookie y revocacion.
- Autorizacion: anonimo, rol menor, acceso horizontal, lectura y escritura.
- Entradas: limites, SQLi, XSS, traversal, archivos y errores sin fuga.
- Concurrencia: solicitudes simultaneas y conteo final en PostgreSQL.
- Web: cabeceras, CORS, datos publicos, dependencias, build y E2E relevante.
- Despliegue: secretos, CORS real, proxy confiable, PostgreSQL privado, restauracion de backups y ZAP en staging autorizado.

## Entrega

Abre con el veredicto. Informa hallazgos cerrados, pendientes, pruebas exactas, archivos, commit y bloqueo. Un `0 alertas` de ZAP no demuestra ausencia de vulnerabilidades; una prueba local tampoco reemplaza staging ni monitoreo.
