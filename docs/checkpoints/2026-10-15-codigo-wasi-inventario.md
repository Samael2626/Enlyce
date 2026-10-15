# Checkpoint 2026-10-15 — buscar codigo Wasi en inventario

## Cambios
- El listado acepta codigoWasi y filtra referencias con prefijo `wasi:<id>:`; no confunde IDs parecidos.
- La tabla muestra el codigo extraido de la referencia sincronizada.
- Inmuebles creados manualmente siguen sin codigo Wasi asignado; se documenta esa brecha.

## Verificacion
- dotnet build src/Enlyce.Api/Enlyce.Api.csproj --no-restore: 0 errores.
- npm run build en frontend: correcto.
- No se ejecutaron pruebas ni se desplego.