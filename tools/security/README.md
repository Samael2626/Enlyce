# Auditoría ofensiva local

El harness prueba controles de autenticación, autorización y abuso contra una instancia aislada de ENLYCE. Se niega a ejecutarse fuera de `localhost:5029` y exige confirmación explícita.

## Requisitos

- Docker Desktop
- .NET SDK y `dotnet-ef`
- Node.js 20 o superior
- Imagen `ghcr.io/zaproxy/zaproxy:stable`

No necesita Burp Suite para la batería automatizada actual.

## Entorno exigido

1. PostgreSQL desechable en un puerto distinto al normal.
2. Migraciones aplicadas sobre esa base.
3. API en `http://localhost:5029`, con seed de desarrollo habilitado y credenciales únicamente sintéticas.
4. Nunca apuntar el harness a staging, producción ni a la base de desarrollo habitual.

## Ejecutar el harness

```powershell
$env:ENLYCE_SECURITY_TARGET = "http://localhost:5029"
$env:ENLYCE_SECURITY_CONFIRM = "ISOLATED"
$env:ENLYCE_SECURITY_SEED_PASSWORD = "<password-del-seed-local>"
node tools\security\strong-audit.mjs
```

El resultado completo se escribe en `security-reports/strong-audit-results.json`. Esa carpeta está ignorada por Git porque puede contener URLs, payloads y evidencia temporal.

## ZAP API activo

```powershell
docker run --rm `
  -v "${PWD}\security-reports:/zap/wrk/:rw" `
  -t ghcr.io/zaproxy/zaproxy:stable `
  zap-api-scan.py `
  -t http://host.docker.internal:5029/swagger/v1/swagger.json `
  -f openapi `
  -O http://host.docker.internal:5029 `
  -a `
  -r zap-api-active.html `
  -J zap-api-active.json `
  -w zap-api-active.md
```

El modo activo modifica datos y dispara muchos requests. Solo se ejecuta contra la base desechable.

## Burp Suite

Instalar Burp Suite Community solo para la siguiente fase manual: interceptar formularios, repetir solicitudes autenticadas y manipular cookies/cuerpos a mano. No reemplaza el harness ni ZAP.
