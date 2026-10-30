# Ensayo de staging

Ensaya el pase a produccion completo en contenedores efimeros. Solo requiere
Docker (Linux, macOS o Windows con Git Bash/WSL).

```bash
tools/staging/ensayo-staging.sh          # construye la imagen desde el Dockerfile
ZAP=1 tools/staging/ensayo-staging.sh    # agrega ZAP baseline
API_IMAGE=enlyce-api:x tools/staging/ensayo-staging.sh   # usa una imagen ya construida
```

Pasos, y se detiene en el primero que falle:

1. Simula produccion: aplica el script idempotente hasta `BASELINE_MIGRATION`
   (por defecto la ultima aplicada segun `docs/protocolo-migraciones-produccion.md`)
   y carga `seed-sintetico.sql` (sin datos reales).
2. Backup `pg_dump -Fc` verificado con `pg_restore --list`.
3. Restore en base aislada y comparacion de conteos por tabla.
4. `tools/migrations/migrations-idempotent.sql` dos veces con `ON_ERROR_STOP`;
   historial completo, datos conservados y restricciones de coordenadas
   (3 decimales y `NULL` aceptados, mayor precision rechazada).
5. Imagen real de la API en modo Production + `smoke-staging.sh`.
6. ZAP baseline (con `ZAP=1`).
7. Rollback: restaura el backup y verifica que vuelve a la linea base.

Los informes, logs y backups quedan en `tools/staging/out/` (ignorado por Git).

## Contra el staging remoto

`smoke-staging.sh` tambien sirve contra cualquier URL:

```bash
tools/staging/smoke-staging.sh https://staging.ejemplo.co
```

Comprueba salud, base conectada, catalogo, politica, CRM, login invalido,
`TRACE`/`TRACK` con 405 sin eco y cabeceras de seguridad (HSTS si es https).
