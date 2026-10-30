#!/usr/bin/env bash
# Smoke de staging: salud, catalogo publico, CRM, cabeceras de seguridad y
# rechazo de TRACE. Sirve para el ensayo local y para el staging remoto.
# Uso: tools/staging/smoke-staging.sh https://staging.ejemplo.co [slug-visible] [slug-oculto]
set -uo pipefail

BASE_URL="${1:?Uso: smoke-staging.sh <url-base> [slug-visible] [slug-oculto]}"
BASE_URL="${BASE_URL%/}"
VISIBLE_SLUG="${2:-}"
HIDDEN_SLUG="${3:-}"
failures=0

check() {
  local name="$1" expected="$2" actual="$3"
  if [[ "$actual" == "$expected" ]]; then
    printf '  [ok]    %s (%s)\n' "$name" "$actual"
  else
    printf '  [FALLO] %s: esperado %s, obtenido %s\n' "$name" "$expected" "$actual"
    failures=$((failures + 1))
  fi
}

status() { curl -s -o /dev/null -w '%{http_code}' --max-time 20 "$@"; }

echo "Smoke contra $BASE_URL"

health="$(curl -s --max-time 20 "$BASE_URL/health")"
check "GET /health" "200" "$(status "$BASE_URL/health")"
check "base de datos conectada" "yes" "$([[ "$health" == *'"database":"connected"'* ]] && echo yes || echo no)"

check "GET catalogo publico" "200" "$(status "$BASE_URL/api/public/inmuebles")"
check "GET politica activa" "200" "$(status "$BASE_URL/api/politica/activa")"
check "GET CRM /" "200" "$(status "$BASE_URL/")"

if [[ -n "$VISIBLE_SLUG" ]]; then
  catalog="$(curl -s --max-time 20 "$BASE_URL/api/public/inmuebles")"
  check "catalogo incluye $VISIBLE_SLUG" "yes" "$([[ "$catalog" == *"\"$VISIBLE_SLUG\""* ]] && echo yes || echo no)"
  check "GET ficha $VISIBLE_SLUG" "200" "$(status "$BASE_URL/api/public/inmuebles/$VISIBLE_SLUG")"
fi
if [[ -n "$HIDDEN_SLUG" ]]; then
  check "ficha incompleta $HIDDEN_SLUG oculta" "404" "$(status "$BASE_URL/api/public/inmuebles/$HIDDEN_SLUG")"
fi

check "login invalido rechazado" "401" "$(status -X POST -H 'Content-Type: application/json' \
  -d '{"correo":"smoke@example.invalid","password":"incorrecta-smoke"}' "$BASE_URL/api/auth/login")"

# TRACE/TRACK deben responder 405 sin reflejar cabeceras (XST).
for path in / /api/public/inmuebles; do
  body="$(curl -s --max-time 20 -X TRACE -H 'X-Eco-Smoke: no-reflejar' "$BASE_URL$path")"
  check "TRACE $path" "405" "$(status -X TRACE "$BASE_URL$path")"
  check "TRACE $path sin eco" "no" "$([[ "$body" == *no-reflejar* ]] && echo si || echo no)"
done
check "TRACK /health" "405" "$(status -X TRACK "$BASE_URL/health")"

headers="$(curl -s -D - -o /dev/null --max-time 20 "$BASE_URL/api/public/inmuebles" | tr -d '\r' | tr '[:upper:]' '[:lower:]')"
for header in "x-content-type-options: nosniff" "x-frame-options: deny" "content-security-policy:" "referrer-policy:"; do
  check "cabecera $header" "yes" "$([[ "$headers" == *"$header"* ]] && echo yes || echo no)"
done

if [[ "$BASE_URL" == https://* ]]; then
  check "cabecera HSTS" "yes" "$([[ "$headers" == *"strict-transport-security:"* ]] && echo yes || echo no)"
fi

echo "Fallos: $failures"
exit "$((failures > 0))"
