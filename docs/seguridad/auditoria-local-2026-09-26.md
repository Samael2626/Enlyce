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

**Estado:** corregido y verificado el 27 de septiembre de 2026.

1. `POST /api/leads` acepta un correo ya registrado.
2. La respuesta del recontacto entrega el identificador, nombre, correo, estado, fecha y metadatos de la oportunidad existente.
3. `PUT /api/leads/{id}/owner-details` es público y considera suficientes el identificador y ese mismo correo.
4. Un tercero que conozca el correo puede recuperar el identificador y sobrescribir detalles del propietario.

**Impacto:** exposición de datos personales, alteración de registros comerciales y contaminación del CRM. Corresponde a una falla de autorización sobre objetos, similar a BOLA/IDOR.

**Solución aplicada:** la respuesta pública ahora es igual para contactos nuevos y repetidos, y ya no entrega identificadores, correo, nombre ni estado. Para completar los datos opcionales se genera un permiso temporal aleatorio, válido por 20 minutos y utilizable una sola vez. La ruta anterior con identificador fue retirada. Una repetición o un permiso inventado recibe una respuesta neutra y no modifica datos.

**Verificación:** se añadieron pruebas automáticas que intentan repetir el ataque original, reutilizar el permiso y consultar un contacto repetido. El ataque quedó bloqueado y la suite completa terminó con 403 pruebas superadas.

### Alta — fuerza bruta y abuso sin límites

**Estado:** reproducido. Quince intentos consecutivos de login inválido respondieron `401`; ninguno respondió `429`.

No existe limitación específica para login, creación pública de oportunidades ni enriquecimiento de propietarios. Esto permite fuerza bruta, spam, crecimiento artificial de consentimientos y abuso combinado con el hallazgo crítico.

**Corrección exigida:** límites por IP y por identidad normalizada, respuesta `429`, ventana progresiva y telemetría. El webhook debe conservar la validación criptográfica y recibir protección de volumen independiente.

### Alta — deduplicación vulnerable a condiciones de carrera

**Estado:** reproducido dos veces contra PostgreSQL desechable.

Una ráfaga de 30 capturas simultáneas con el mismo correo creó 30 identificadores distintos en la primera ejecución y 19 en la segunda. La secuencia “consultar si existe” y luego “insertar” no es atómica y la base no impide el duplicado.

**Impacto:** contaminación masiva del pipeline, correos repetidos, asignaciones contradictorias y consumo evitable de recursos.

**Corrección exigida:** restricción o índice coherente con la verdadera clave de oportunidad y operación atómica con manejo explícito del conflicto. Una comprobación previa en código no basta.

### Alta — amplificación de recursos y registros

**Estado:** reproducido con harness propio y escaneo activo.

- Sesenta logins inválidos simultáneos produjeron 60 respuestas `401` y ninguna `429`.
- `POST /api/leads` aceptó un cuerpo con un campo de 1 MB y respondió `201`; el valor se recorta después, pero el cuerpo completo ya fue recibido, materializado y procesado.
- Filtros hostiles del catálogo producen excepciones y stacks completos en el log de Next.js. El cliente recibe solo un digest, pero una ráfaga puede saturar CPU, disco y observabilidad.
- El API registra excepciones completas por credenciales inválidas; durante la prueba generó un volumen desproporcionado de logs para respuestas esperables.

**Corrección exigida:** límites de cuerpo y longitud antes de materializar comandos, rate limiting, validación temprana de filtros y logging resumido para fallos esperables sin stack por petición.

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
- Suite .NET: 400 de 400 pruebas superadas (`--no-build --no-restore`).
- Reportes ZAP locales en `security-reports/`; son artefactos de diagnóstico, no producto final.

## Orden obligatorio

1. Cerrar la fuga y modificación del recontacto.
2. Hacer atómica la deduplicación y probarla con concurrencia real.
3. Agregar límites de cuerpo, rate limiting y logging resistente al abuso.
4. Añadir cabeceras, HTTPS/HSTS y endurecer sesión.
5. Convertir el harness ofensivo en prueba de regresión y repetir ZAP antes de staging público.
