# Checkpoint 2026-10-14 — papelera reversible de inmuebles

## Evidencia Wasi
La guia oficial indica que los inmuebles eliminados aparecen en Ver basurero y que Restaurar los devuelve al inventario en estado inactivo. Tambien permite borrado permanente individual o vaciar papelera.

## Implementado
- GET /api/inmuebles?papelera=true lista elementos inactivos; el inventario normal conserva solo activos.
- POST /api/inmuebles/{id}/papelera marca Activo=false.
- POST /api/inmuebles/{id}/restaurar reactiva el registro y fija Estado=Inactivo, conforme a Wasi.
- UI permite abrir papelera, enviar elementos y restaurarlos.
- Borrado permanente queda pendiente: CustomerDemandPropertyLinks y PropertyPublications tienen DeleteBehavior.Restrict. No destruir relaciones ni publicaciones sin politica de retencion.

## Verificacion
- dotnet build src/Enlyce.Api/Enlyce.Api.csproj --no-restore: correcto, 0 errores.
- npm run build en frontend: correcto.
- No se ejecutaron pruebas ni se desplego.