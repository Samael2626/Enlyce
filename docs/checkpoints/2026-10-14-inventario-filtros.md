# Checkpoint 2026-10-14 — filtros inventario Wasi

## Objetivo
Avanzar paridad funcional Wasi en Enlyce por módulos. Esta entrega completa la búsqueda avanzada que soportan los campos actuales del inmueble.

## Cambios
- API GET /api/inmuebles acepta filtros de texto, tipo, modalidad, estado, país/departamento/ciudad/barrio, moneda/precio, área, habitaciones, baños, parqueaderos y orden por reciente/antiguo/precio.
- Listado React presenta formulario de filtros aplicables y muestra ubicación y precio.
- Checklist actualizado: edición, código Wasi, año, asesor, etiquetas, disponibilidad, mapa, papelera y acciones masivas siguen pendientes.

## Verificación
- `dotnet build src/Enlyce.Api/Enlyce.Api.csproj --no-restore`: correcto, 0 errores; warnings preexistentes (CVE ImageSharp y nulabilidad).
- `npm run build`: correcto; advertencia de chunk grande y configuración Vite.
- No se ejecutaron pruebas. Sin despliegue ni cambios de producción.

## Archivos del alcance
API InmueblesModule; Application ListInmueblesQuery; Infrastructure InmuebleListReadRepository; frontend client, useApi, InmueblesPage; docs/checklist-paridad-wasi-enlyce.