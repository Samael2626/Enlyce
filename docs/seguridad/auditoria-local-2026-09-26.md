---
title: "Auditoría local de seguridad ENLYCE"
date: 2026-09-26
tags: [enlyce, seguridad, auditoria, owasp, web, api]
status: corregido-y-verificado
---

# Auditoría local de seguridad — ENLYCE

Relacionado: [[Enlyce-MOC]] · [[Checklist-Producto-Web-Enlyce]] · [[Arquitectura-Web-API-Enlyce]]

## Veredicto

Los hallazgos críticos, altos y medios de esta auditoría quedaron corregidos y verificados localmente el 27 de septiembre de 2026. Los servicios de desarrollo quedaron limitados al propio equipo. La salida a Internet sigue condicionada a desplegar esta versión, configurar secretos reales y repetir el escaneo en staging. `TRACE` queda aceptado solo en local y debe bloquearse en el proxy del futuro despliegue.

## Hallazgos

### Crítico — enumeración y modificación no autorizada de oportunidades

**Estado:** corregido y verificado el 27 de septiembre de 2026.

1. `POST /api/leads` acepta un correo ya registrado.
2. La respuesta del recontacto entrega el identificador, nombre, correo, estado, fecha y metadatos de la oportunidad existente.
3. `PUT /api/leads/{id}/owner-details` es público y considera suficientes el identificador y ese mismo correo.
4. Un tercero que conozca el correo puede recuperar el identificador y sobrescribir detalles del propietario.

**Impacto:** exposición de datos personales, alteración de registros comerciales y contaminación del CRM. Corresponde a una falla de autorización sobre objetos, similar a BOLA/IDOR.

**Solución aplicada:** la respuesta pública ahora es igual para contactos nuevos y repetidos, y ya no entrega identificadores, correo, nombre ni estado. Para completar los datos opcionales se genera un permiso temporal aleatorio, válido por 20 minutos y utilizable una sola vez. La ruta anterior con identificador fue retirada. Una repetición o un permiso inventado recibe una respuesta neutra y no modifica datos.

**Verificación:** se añadieron pruebas automáticas que intentan repetir el ataque original, reutilizar el permiso y consultar un contacto repetido. El ataque quedó bloqueado y la suite completa terminó con 408 pruebas superadas.

### Alta — fuerza bruta y abuso sin límites

**Estado:** corregido y verificado. El login limita por IP y por correo normalizado; la captura pública, el enriquecimiento y el webhook tienen límites independientes. Una ráfaga de 60 logins produjo 7 respuestas `401` y 53 respuestas `429`.

Antes de la corrección no existía limitación específica para login, creación pública de oportunidades ni enriquecimiento de propietarios. Esto permitía fuerza bruta, spam y crecimiento artificial de consentimientos.

Los rechazos entregan `429` y `Retry-After`. La firma criptográfica de Wompi se conserva.

### Alta — deduplicación vulnerable a condiciones de carrera

**Estado:** corregido y verificado contra PostgreSQL desechable.

Una ráfaga de 30 capturas simultáneas con el mismo correo creó 30 identificadores distintos en la primera ejecución y 19 en la segunda. La secuencia “consultar si existe” y luego “insertar” no es atómica y la base no impide el duplicado.

**Impacto:** contaminación masiva del pipeline, correos repetidos, asignaciones contradictorias y consumo evitable de recursos.

**Solución aplicada:** cada oportunidad tiene una clave formada por correo normalizado y publicación. PostgreSQL aplica un índice único sobre oportunidades activas y el repositorio maneja el conflicto de inserción. Treinta solicitudes simultáneas respondieron correctamente, pero persistieron una sola oportunidad.

### Alta — amplificación de recursos y registros

**Estado:** corregido y verificado con el harness ofensivo.

- Sesenta logins inválidos simultáneos produjeron 60 respuestas `401` y ninguna `429`.
- `POST /api/leads` aceptó un cuerpo con un campo de 1 MB y respondió `201`; el valor se recorta después, pero el cuerpo completo ya fue recibido, materializado y procesado.
- Filtros hostiles del catálogo producen excepciones y stacks completos en el log de Next.js. El cliente recibe solo un digest, pero una ráfaga puede saturar CPU, disco y observabilidad.
- El API registra excepciones completas por credenciales inválidas; durante la prueba generó un volumen desproporcionado de logs para respuestas esperables.

**Solución aplicada:** límites de 16 KB para login, 32 KB para capturas y 256 KB para Wompi; longitudes validadas; filtros web acotados; errores esperables registrados sin stack ni datos enviados por el atacante. El cuerpo de 1 MB ahora responde `413`.

### Media — cabeceras defensivas ausentes

**Estado inicial:** confirmado manualmente y mediante OWASP ZAP sobre el build de producción de Next.js.

Faltaban CSP, protección anti-clickjacking, `X-Content-Type-Options`, `Referrer-Policy` y `Permissions-Policy`. Next.js además publicaba `X-Powered-By`; Kestrel publicaba `Server`.

**Impacto:** mayor superficie frente a XSS futuro, clickjacking y reconocimiento tecnológico.

**Estado:** corregido. API y sitio envían CSP, anti-clickjacking, `nosniff`, `Referrer-Policy` y `Permissions-Policy`. Next.js ya no envía `X-Powered-By` y Kestrel ya no agrega `Server`.

### Media — transporte y sesión incompletos para producción

- `RequireHttpsMetadata` está desactivado sin condición de ambiente.
- No se observó redirección HTTPS ni HSTS en la API.
- El login crea una cookie `HttpOnly`, `Secure` y `SameSite=Strict`, pero también devuelve el JWT en el JSON aunque el frontend no lo utiliza.
- Cerrar sesión elimina la cookie, pero no revoca un JWT ya copiado. La vigencia configurada es de ocho horas.

**Corrección aplicada:** HTTPS/HSTS en producción, token fuera del cuerpo de respuesta, vigencia reducida y revocación al cerrar sesión.

**Estado:** corregido. Producción exige metadatos HTTPS, usa redirección HTTPS y HSTS. El JWT ya no aparece en el JSON, dura 30 minutos y la cookie sigue siendo `HttpOnly`, `Secure` y `SameSite=Strict`. Cerrar sesión incrementa la versión de sesión del asesor: una copia anterior del JWT recibe `401`.

### Baja — huella tecnológica

Las respuestas revelan Next.js y Kestrel. No entrega acceso por sí sola, pero facilita reconocimiento automatizado.

**Estado:** corregido mediante la eliminación de ambas cabeceras.

## Controles que sí funcionaron

- Rutas privadas representativas devolvieron `401` sin sesión.
- CORS no autorizó el origen malicioso probado y sí reconoció el origen local permitido.
- Cookie de autenticación con `HttpOnly`, `Secure` y `SameSite=Strict`.
- JWT alterado y JWT con `alg: none` rechazados con `401`.
- Autorización horizontal validada: un asesor ajeno recibió `403` al leer, mover, reasignar, suplantar o administrar una oportunidad de otro asesor.
- Firma del webhook Wompi comparada en tiempo constante.
- Carga de imágenes autenticada, limitada y decodificada antes de persistir.
- Sin secretos reales encontrados en archivos versionables.
- `npm audit`: cero vulnerabilidades conocidas en CRM y sitio público.
- Auditoría NuGet: cero paquetes vulnerables conocidos.
- Payload XSS reflejado por filtros: codificado, no ejecutable.
- Payload de inyección SQL en catálogo: respondió normalmente; EF Core parametriza la consulta.
- ZAP API activo no confirmó SQLi, XSS, traversal, XXE, RCE, inyección de comandos ni inclusión de archivos.

## Herramientas y evidencia

- Revisión manual de autenticación, autorización, CORS, endpoints públicos, medios y webhook.
- Pruebas HTTP contra `localhost:5019` y build Next.js en `localhost:3100`.
- OWASP ZAP Baseline: 602 URL, 0 fallos altos, 10 familias de advertencias.
- OWASP ZAP API activo: 157 URL importadas, 119 reglas superadas, 0 alertas altas/medias y 4 bajas. Los `503` repetidos del webhook fueron causados por el secreto Wompi ausente en el entorno desechable, no por ejecución del payload.
- Harness ofensivo reproducible: fuerza bruta, integridad JWT, RBAC horizontal, carrera de duplicados, BOLA, cuerpos grandes, CORS y method override.
- El escaneo web activo con navegador fue detenido después de diez minutos: ZAP abrió decenas de procesos Firefox y superó un consumo razonable. Sus resultados son parciales y no se presentan como cobertura completa.
- Suite .NET: 408 de 408 pruebas superadas.
- Regresión ofensiva posterior: 53 de 60 logins bloqueados con `429`, 30 capturas concurrentes reducidas a un registro, cuerpo de 1 MB rechazado con `413`, JWT alterados rechazados y BOLA anterior en `404`.
- Reportes ZAP locales en `security-reports/`; son artefactos de diagnóstico, no producto final.

## Revalidación del 27 de septiembre de 2026

- Se creó una base PostgreSQL desechable y se aplicó desde cero toda la cadena de migraciones. La última migración registrada fue `20260927155326_HardenSecurityAudit`.
- La base confirmó los dos índices únicos que protegen la continuación privada de propietarios y la creación concurrente de oportunidades.
- El harness ofensivo repitió los controles principales: 53 de 60 intentos de acceso fueron limitados, los JWT manipulados recibieron `401`, el acceso de otro asesor recibió `403`, la sobrescritura anónima recibió `404`, el cuerpo de 1 MB recibió `413` y 30 capturas simultáneas produjeron un solo registro.
- ZAP API activo importó 56 operaciones, recorrió 156 URL y superó 120 reglas sin fallos confirmados. No encontró inyección SQL, XSS, traversal, XXE, ejecución de comandos ni inclusión remota.
- ZAP mostró tres advertencias operativas: respuestas `503` del webhook Wompi porque el entorno desechable no tenía secreto del proveedor, tipo de contenido inesperado en Swagger y cabecera `Cross-Origin-Resource-Policy` ausente en cuatro rutas de desarrollo. Ninguna demostró una explotación.
- La cuenta Railway fue consultada y solo contiene `BotLaw` y `Arcanum`. ENLYCE aún no tiene staging remoto; por eso no se ejecutó un ataque sobre una URL pública inventada ni sobre producción.
- Nmap confirmó las cabeceras defensivas en la compilación reconstruida de Next.js servida en `3100`; dejó de revelar `X-Powered-By`.
- La CSP se ajustó para permitir `unsafe-eval` exclusivamente durante `next dev`, porque React lo usa para sus herramientas de depuración. La comprobación HTTP confirmó que el aviso desaparece en desarrollo y que la compilación de producción sigue bloqueando `eval`.
- Burp confirmó `404` para rutas inexistentes y no encontró reflexión del payload XSS básico. `TRACE` devuelve la página sin reflejar la solicitud, por lo que no se reprodujo Cross-Site Tracing clásico. Next.js no permite interceptar correctamente ese método desde `proxy.ts`; se bloqueará en el proxy de despliegue para evitar introducir un servidor personalizado que elimine optimizaciones.
- PostgreSQL 17 nativo se limitó de `listen_addresses='*'` a `listen_addresses='localhost'`. `pg_isready` confirmó servicio local y Nmap mostró `5432` abierto en `127.0.0.1` pero cerrado en `192.168.40.7`. La configuración anterior quedó respaldada como `postgresql.conf.bak-20260927-2115`.
- Los scripts `dev` y `start` de Next.js ahora enlazan `3000` y `3100` exclusivamente a `127.0.0.1`. El contenedor `enlyce-db` conservó su volumen y cambió `5436` de todas las interfaces a `127.0.0.1`.
- La regresión final de Nmap mostró `3000`, `3100`, `5432` y `5436` abiertos por loopback y cerrados mediante `192.168.40.7`. Ambos PostgreSQL respondieron localmente y Burp siguió alcanzando las dos webs a través de su proxy local.

## Cierre aplicado

1. Fuga y modificación del recontacto: corregida.
2. Deduplicación atómica: corregida y probada con concurrencia real.
3. Límites, rate limiting y logging: corregidos.
4. Cabeceras, HTTPS/HSTS y sesión: corregidos.
5. Migración y ZAP: repetidos satisfactoriamente en un entorno local aislado. Queda una última repetición remota cuando se cree el staging público de ENLYCE.
6. Web `3100` y PostgreSQL local: compilación defensiva actualizada y puerto de base de datos retirado de la LAN.
7. `TRACE`: impacto bajo, sin reflexión confirmada; pendiente de bloqueo en el proxy de staging.
8. Superficie LAN local: webs y las dos instancias PostgreSQL de ENLYCE quedaron inaccesibles mediante la dirección de red del equipo.
