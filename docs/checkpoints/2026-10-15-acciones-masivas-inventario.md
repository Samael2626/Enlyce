# Checkpoint 2026-10-15 — acciones masivas del inventario

## Cambios
- Seleccion por pagina en inventario y papelera.
- Acciones masivas para enviar a papelera o restaurar, con tope de 100 IDs en API.
- Validacion all-or-none: IDs repetidos, inexistentes o con estado distinto cancelan la operacion completa. Persistencia en un solo SaveChanges de EF.
- Checklist actualizado con esta cobertura y brechas restantes.

## Verificacion
- dotnet build src/Enlyce.Api/Enlyce.Api.csproj --no-restore: 0 errores.
- npm run build: correcto.
- No se ejecutaron pruebas ni se desplego.