# Checkpoint 2026-10-14 — edicion basica de inmuebles

## Cambio
- Agregado PUT /api/inmuebles/{id} y caso de uso que edita ficha descriptiva existente.
- La UI carga datos, permite corregir nombre, descripcion, tipo, modalidad, direccion, precio, area, habitaciones, banos, parqueaderos y caracteristicas.
- Conserva estado comercial, propietario, fotos/publicacion y referencia externa. No requiere migracion.
- Checklist marca que los campos completos del formulario Wasi siguen pendientes.

## Verificacion
- dotnet build src/Enlyce.Api/Enlyce.Api.csproj --no-restore: 0 errores.
- npm run build: correcto; advertencia Vite por chunk >500 KB.
- No se ejecutaron pruebas ni se desplego.