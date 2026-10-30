#!/usr/bin/env bash
# Ensayo del pase a produccion en contenedores efimeros. Solo requiere Docker.
#
#  1. Simula produccion: migraciones hasta BASELINE_MIGRATION + datos sinteticos.
#  2. Backup pg_dump custom y verificacion con pg_restore --list.
#  3. Restore en una base aislada y comparacion de conteos por tabla.
#  4. Script idempotente aplicado dos veces (ON_ERROR_STOP) y verificacion
#     de historial y restricciones de coordenadas.
#  5. Imagen real de la API en modo Production + smoke-staging.sh.
#  6. Rollback: restaura el backup y comprueba que vuelve al estado previo.
#  7. Opcional (ZAP=1): ZAP baseline contra la API.
#
# Uso: tools/staging/ensayo-staging.sh
# Variables: API_IMAGE (por defecto construye el Dockerfile), BASELINE_MIGRATION,
#            ZAP=1, KEEP=1 (no destruye contenedores al terminar).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SQL="$ROOT/tools/migrations/migrations-idempotent.sql"
SEED="$ROOT/tools/staging/seed-sintetico.sql"
OUT="$ROOT/tools/staging/out"
# Ultima migracion aplicada en produccion segun docs/protocolo-migraciones-produccion.md.
BASELINE_MIGRATION="${BASELINE_MIGRATION:-20261010023810_UseLeadSlaResponseMinutes}"
API_IMAGE="${API_IMAGE:-}"
API_PORT="${API_PORT:-18080}"
PG="enlyce-ensayo-pg"
API="enlyce-ensayo-api"
NET="enlyce-ensayo"
STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
REPORT="$OUT/ensayo-$STAMP.md"

mkdir -p "$OUT"
log() { printf '\n== %s\n' "$*"; echo "- $*" >> "$REPORT"; }
fail() { echo "FALLO: $*" >&2; echo "- **FALLO:** $*" >> "$REPORT"; exit 1; }
psql_in() { docker exec -i "$PG" psql -U postgres -v ON_ERROR_STOP=1 -q "$@"; }
scalar() { docker exec -i "$PG" psql -U postgres -Atq -d "$1" -c "$2"; }

cleanup() {
  [[ "${KEEP:-0}" == "1" ]] && return
  docker rm -f "$API" "$PG" >/dev/null 2>&1 || true
  docker network rm "$NET" >/dev/null 2>&1 || true
}
trap cleanup EXIT

{
  echo "# Ensayo de staging $STAMP"
  echo
  echo "- Commit: \`$(git -C "$ROOT" rev-parse --short HEAD)\`$(git -C "$ROOT" diff --quiet || echo ' (con cambios sin commit)')"
  echo "- Linea base simulada de produccion: \`$BASELINE_MIGRATION\`"
} > "$REPORT"

# 0. Imagen de la API
if [[ -z "$API_IMAGE" ]]; then
  API_IMAGE="enlyce-api:ensayo"
  log "Construyendo imagen $API_IMAGE desde el Dockerfile"
  docker build -q -t "$API_IMAGE" "$ROOT" >/dev/null
fi

# 1. PostgreSQL efimero y simulacion de produccion
cleanup
docker network create "$NET" >/dev/null
docker run -d --name "$PG" --network "$NET" -e POSTGRES_HOST_AUTH_METHOD=trust postgres:17-alpine >/dev/null
# La imagen arranca un servidor temporal de inicializacion y luego el definitivo:
# se espera al segundo "ready to accept connections".
for _ in $(seq 1 60); do
  [[ "$(docker logs "$PG" 2>&1 | grep -c 'ready to accept connections')" -ge 2 ]] && break
  sleep 1
done
docker exec "$PG" pg_isready -U postgres >/dev/null || fail "PostgreSQL no arranco"

total_migrations="$(grep -o "'[0-9]\{14\}_[A-Za-z0-9]*'" "$SQL" | sort -u | wc -l)"
next_line="$(grep -o "'[0-9]\{14\}_[A-Za-z0-9]*'" "$SQL" | sort -u | grep -A1 "'$BASELINE_MIGRATION'" | tail -1 | tr -d "'")"
[[ -n "$next_line" && "$next_line" != "$BASELINE_MIGRATION" ]] || fail "No hay migraciones posteriores a $BASELINE_MIGRATION"
first_use="$(grep -n "$next_line" "$SQL" | head -1 | cut -d: -f1)"
cut_line="$(head -n "$first_use" "$SQL" | grep -n '^START TRANSACTION;' | tail -1 | cut -d: -f1)"

psql_in -c "CREATE DATABASE produccion_sim" -c "CREATE DATABASE staging"
head -n "$((cut_line - 1))" "$SQL" | psql_in -d produccion_sim 2>&1 | grep -v NOTICE || true
psql_in -d produccion_sim < "$SEED"
baseline_count="$(scalar produccion_sim 'SELECT count(*) FROM "__EFMigrationsHistory"')"
log "Produccion simulada: $baseline_count migraciones aplicadas y datos sinteticos cargados"

# 2. Backup
docker exec "$PG" pg_dump -U postgres -Fc -d produccion_sim -f /tmp/backup.dump
toc_entries="$(docker exec "$PG" pg_restore --list /tmp/backup.dump | grep -vc '^;')"
docker cp "$PG:/tmp/backup.dump" "$OUT/backup-$STAMP.dump" >/dev/null
[[ "$toc_entries" -gt 0 ]] || fail "pg_restore --list no lee el backup"
log "Backup custom verificado: $toc_entries entradas TOC"

# 3. Restore aislado y conteos
counts_sql="SELECT string_agg(format('%s=%s', t, (xpath('/row/c/text()', query_to_xml(format('SELECT count(*) AS c FROM %I', t), false, true, '')))[1]::text), ',' ORDER BY t) FROM (SELECT table_name AS t FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE') x"
docker exec "$PG" pg_restore -U postgres -d staging --exit-on-error /tmp/backup.dump
[[ "$(scalar produccion_sim "$counts_sql")" == "$(scalar staging "$counts_sql")" ]] || fail "Los conteos del restore no coinciden"
pre_counts="$(scalar staging "$counts_sql")"
log "Restore aislado con conteos identicos por tabla"

# 4. Migraciones pendientes, dos veces para probar idempotencia
for pass in 1 2; do
  psql_in -d staging < "$SQL" 2>&1 | grep -v NOTICE || true
  [[ "${PIPESTATUS[0]}" == "0" ]] || fail "El script idempotente fallo en la pasada $pass"
done
applied="$(scalar staging 'SELECT count(*) FROM "__EFMigrationsHistory"')"
[[ "$applied" == "$total_migrations" ]] || fail "Historial con $applied de $total_migrations migraciones"
log "Script idempotente aplicado dos veces: $((applied - baseline_count)) migraciones nuevas, historial $applied/$total_migrations"

# Las filas de datos previas se conservan (las migraciones pueden agregar filas de catalogo).
for table in Asesores Inmuebles PropertyPublications PropertyPhotos Propietarios PoliticasTratamiento; do
  before="$(echo "$pre_counts" | tr ',' '\n' | grep "^$table=" | cut -d= -f2)"
  after="$(scalar staging "SELECT count(*) FROM \"$table\"")"
  [[ "$before" == "$after" ]] || fail "$table cambio de $before a $after filas"
done
log "Datos previos conservados en las tablas principales"

coords="$(scalar staging 'SELECT string_agg(coalesce(round("ApproximateLatitude", 3)::text, $$NULL$$), $$,$$ ORDER BY "Slug") FROM "PropertyPublications"')"
[[ "$coords" == "6.244,NULL" ]] || fail "Coordenadas alteradas: $coords"
psql_in -d staging -c 'BEGIN; UPDATE "PropertyPublications" SET "ApproximateLatitude" = 6.245 WHERE "Slug" = $$apartamento-ensayo-a$$; ROLLBACK;' \
  || fail "Rechazo una coordenada valida de 3 decimales"
if psql_in -d staging -c 'BEGIN; UPDATE "PropertyPublications" SET "ApproximateLatitude" = 6.2451 WHERE "Slug" = $$apartamento-ensayo-a$$; ROLLBACK;' 2>/dev/null; then
  fail "Acepto una coordenada con 4 decimales"
fi
log "Coordenadas: conserva 3 decimales y NULL, rechaza mayor precision"

# 5. Imagen real en modo Production + smoke
docker run -d --name "$API" --network "$NET" -p "127.0.0.1:$API_PORT:8080" \
  -e ConnectionStrings__Default="Host=$PG;Port=5432;Database=staging;Username=postgres" \
  -e JwtSettings__SecretKey="$(head -c 48 /dev/urandom | base64)" \
  -e Cors__AllowedOrigins__0="https://staging.example.invalid" \
  -e PublicCatalog__Enabled=true \
  "$API_IMAGE" >/dev/null
for _ in $(seq 1 60); do curl -sf "http://127.0.0.1:$API_PORT/health" >/dev/null && break; sleep 1; done
smoke_log="$OUT/smoke-$STAMP.log"
if "$ROOT/tools/staging/smoke-staging.sh" "http://127.0.0.1:$API_PORT" apartamento-ensayo-a casa-ensayo-b | tee "$smoke_log"; then
  log "Smoke contra la imagen en modo Production: sin fallos"
else
  fail "Smoke con fallos (ver $(basename "$smoke_log"))"
fi
errors="$(docker logs "$API" 2>&1 | grep -E '^ *fail:|Unhandled exception' | head -5 || true)"
[[ -z "$errors" ]] || fail "La API registro errores: $errors"
log "Logs de la API sin errores"

# 7. ZAP opcional, antes del rollback
if [[ "${ZAP:-0}" == "1" ]]; then
  # ZAP corre con un usuario sin privilegios: necesita escribir en la carpeta montada.
  zap_dir="$(mktemp -d)"
  chmod 777 "$zap_dir"
  docker run --rm --network "$NET" -v "$zap_dir:/zap/wrk:rw" ghcr.io/zaproxy/zaproxy:stable \
    zap-baseline.py -t "http://$API:8080" -r "zap.html" -J "zap.json" -I > "$OUT/zap-$STAMP.log" 2>&1 || true
  [[ -f "$zap_dir/zap.json" ]] || fail "ZAP no genero informe (ver zap-$STAMP.log)"
  cp "$zap_dir/zap.html" "$OUT/zap-$STAMP.html"
  cp "$zap_dir/zap.json" "$OUT/zap-$STAMP.json"
  rm -rf "$zap_dir"
  zap_summary="$(grep -E '^(FAIL-NEW|WARN-NEW|PASS):' "$OUT/zap-$STAMP.log" | tail -1)"
  log "ZAP baseline: $zap_summary (informe zap-$STAMP.html)"
fi

# 6. Rollback: restaurar el backup sobre staging
docker rm -f "$API" >/dev/null
psql_in -c "DROP DATABASE staging WITH (FORCE)" -c "CREATE DATABASE staging"
docker exec "$PG" pg_restore -U postgres -d staging --exit-on-error /tmp/backup.dump
[[ "$(scalar staging 'SELECT count(*) FROM "__EFMigrationsHistory"')" == "$baseline_count" ]] || fail "El rollback no volvio a la linea base"
[[ "$(scalar staging "$counts_sql")" == "$pre_counts" ]] || fail "El rollback no recupero los mismos datos"
log "Rollback: el backup restaura la linea base ($baseline_count migraciones) con los mismos datos"

echo >> "$REPORT"
echo "**Resultado: APROBADO**" >> "$REPORT"
echo
echo "Ensayo APROBADO. Informe: $REPORT"
