# Runbook P0 de staging

Estado: preparado; no hay staging ENLYCE remoto ni proveedor/proxy elegido. No usar contra producción.

## 1. Gate PostgreSQL local ya ejecutado

PostgreSQL 17 efímero, sin volumen y publicado solo en `127.0.0.1:55437`. La prueba aplicó todas las migraciones en un esquema aislado, conservó coordenadas de 3 decimales y `NULL`, y rechazó precisión mayor. Resultado: 1 aprobada, 0 omitidas. El contenedor fue eliminado.

Para repetir en PowerShell desde la raíz:

```powershell
docker run --rm -d --name enlyce-coordinate-p0-test `
  -e POSTGRES_HOST_AUTH_METHOD=trust `
  -e POSTGRES_DB=enlyce_coordinate_test `
  -p 127.0.0.1:55437:5432 postgres:17-alpine

try {
  $ready = $false
  for ($attempt = 0; $attempt -lt 30; $attempt++) {
    docker exec enlyce-coordinate-p0-test pg_isready -U postgres -d enlyce_coordinate_test *> $null
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 1
  }
  if (-not $ready) { throw 'PostgreSQL did not become ready.' }

  $env:ENLYCE_TEST_POSTGRES_CONNECTION = 'Host=127.0.0.1;Port=55437;Database=enlyce_coordinate_test;Username=postgres'
  dotnet test tests\Enlyce.IntegrationTests\Enlyce.IntegrationTests.csproj --filter FullyQualifiedName~PublicCoordinatePostgresConstraintTests
  $testExitCode = $LASTEXITCODE
} finally {
  Remove-Item Env:\ENLYCE_TEST_POSTGRES_CONNECTION -ErrorAction SilentlyContinue
  docker stop enlyce-coordinate-p0-test *> $null
}
if ($testExitCode -ne 0) { throw "PostgreSQL constraint test failed: $testExitCode" }
```

## 2. Antes de habilitar staging

- Elegir hosting, proxy, dominio y dueño operativo; registrar solo nombres de ambiente y recursos, nunca credenciales.
- Crear staging aislado con TLS, acceso restringido y datos sintéticos o anonimización aprobada.
- Inyectar secretos desde el almacén del proveedor; no en Git, imágenes, argumentos, logs o tickets.
- Configurar backup automatizado, retención y alerta de fallo. Guardar dumps cifrados fuera del repositorio.

## 3. Gate de cada despliegue a staging

1. Confirmar ambiente y base por identificador; detenerse si destino no coincide. Revisar que la versión esté identificada y que staging no sea producción.
2. Crear backup custom de la base objetivo con `pg_dump`; comprobar que el archivo existe y `pg_restore --list` lo lee.
3. Restaurar el backup en una base aislada. Comparar conteos e invariantes acordados. Aplicar allí el SQL idempotente de migraciones con `psql --set ON_ERROR_STOP=1 --single-transaction`.
4. Ejecutar smoke de API/web, pruebas E2E y ZAP API contra el host de staging. Verificar la migración `20261009224818_EnforcePublicCoordinatePrecision`, constraints de latitud/longitud, coordenadas existentes, `NULL`, 3 decimales y rechazo de mayor precisión en copia desechable.
5. Probar `TRACE` únicamente contra staging: debe responder `405` o `501`, sin eco. La regla debe vivir en el proxy elegido; Next.js `proxy.ts` no intercepta este método correctamente.
6. Revisar errores y salud tras el despliegue. Si falla un gate, detener promoción y restaurar backup probado o revertir la versión según el plan ensayado.
7. Registrar commit, migraciones, backup, resultado de restore, smoke/ZAP, operador y hora. Definir RPO/RTO con responsable antes de aceptar el runbook.

## Bloqueos actuales

- No existe staging remoto ENLYCE ni configuración de proxy/despliegue en el repo.
- Falta proveedor/host, URL de staging, secreto de despliegue, almacén de secretos y política de backups.
- El gate local no sustituye aplicar y verificar esta migración en staging.
- No ejecutar `dotnet ef database update` a ciegas: una prueba previa falló contra base vacía al consultar `__EFMigrationsHistory`; generar e inspeccionar SQL antes de aplicarlo.
