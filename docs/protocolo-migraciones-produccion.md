# Protocolo de Migraciones en Producción — Enlyce

## 1. Estado y Contexto de Producción

- **Base de datos objetivo:** PostgreSQL en Railway (`dbname: railway`, versión 18 / 17 compatible).
- **Almacenamiento de respaldos:** Bucket privado de Supabase Storage `enlyce-backups`.
- **Tarea programada de respaldo diario:** `Enlyce-Postgres-Backup` ejecutada automáticamente mediante `tools/backup/Backup-Enlyce.ps1` hacia `%LOCALAPPDATA%\Enlyce\Backups`.
- **Último respaldo verificado:**
  - Archivo: `%LOCALAPPDATA%\Enlyce\Backups\enlyce-20261021-030036.dump` (110.901 bytes).
  - Formato: PostgreSQL Custom Dump (gzip, versión 1.16-0).
  - Integridad: 151 entradas TOC verificadas exitosamente con `pg_restore.exe --list`.
- **Política de congelamiento:** Ninguna migración nueva se aplica a producción sin autorización explícita del usuario y verificación de respaldo previo.

---

## 2. Cadena de Migraciones Pendientes en Producción

Las siguientes 22 migraciones aditivas están generadas en código y esperan verificación del historial real antes de cualquier pase a producción. Las suites de dominio e integración pasan en el checkout aislado; esto no acredita una aplicación de todas las migraciones en PostgreSQL:

1. `20261013002606_AgregarExternalRefInmueble`: Referencia externa Wasi para trazabilidad de sincronización.
2. `20261014132917_AddIndependentImportedContacts`: Contactos independientes importados sin oportunidad obligatoria.
3. `20261014142031_AddContactEmailPermissionEvents`: Registro y auditoría de permisos de correo bajo Ley 1581.
4. `20261014142900_AddContactImportIdempotency`: Prevención de duplicados en importación masiva de contactos.
5. `20261014143455_AddCustomerDemandAreaAndCurrency`: Filtros de área y moneda en demandas de clientes.
6. `20261014144248_AddPropertyCountryDepartmentAndDemandLocation`: Ubicación geográfica extendida para cruces de demanda.
7. `20261014145955_AddContactPrivateProfileFields`: Campos privados de perfil de contacto.
8. `20261014160000_AddContactFunnel`: Embudos personalizados para contactos.
9. `20261014162300_AddDemandAndPropertyFeatures`: Características internas y externas para cruces algorítmicos.
10. `20261014170000_AddMultipleCustomerTypes`: Clasificación multietiqueta y tipos personalizados de cliente.
11. `20261014180000_AddCustomerLabels`: Etiquetas de contacto libres y catalogadas.
12. `20261015010253_AddCustomerCatalogItems`: Catálogo administrable de opciones globales en CRM.
13. `20261015043830_AgregarAnioConstruccionInmueble`: Año de construcción de propiedades.
14. `20261015044422_AgregarEstadoFisicoInmueble`: Estado físico de conservación del inmueble.
15. `20261015044838_AgregarEtiquetasInmueble`: Etiquetas para categorización de propiedades.
16. `20261015045612_AgregarAsesorResponsableInmueble`: Asesor responsable comercial por propiedad.
17. `20261015052059_AddCustomerFunnelStages`: Etapas dinámicas de embudo y etapa predeterminada para inbound.
18. `20261015154409_AddRealizedDeals`: Registro financiero de cierres de negocio y cálculo de comisiones.
19. `20261015162531_AddEmailDeliveryLogs`: Bitácora de entregas de correo y notificaciones SMTP.
20. `20261023232115_AddChatMessages`: Mensajería interna en tiempo real (SignalR + buzones 1-a-1 y general).
21. `20261023235334_AddPublicWebsiteConfig`: CMS para edición del sitio web público, banners y metadatos SEO.
22. `20261024014120_AddBlogPosts`: artículos editoriales con borrador y publicación.

---

## 3. Script Idempotente Generado

El script SQL idempotente preparado para una transacción única se encuentra ubicado en:
`tools/migrations/migrations-idempotent.sql` (80.598 bytes; 44 migraciones históricas).

Características técnicas del script:
- Bloques condicionales PL/pgSQL `DO $EF$ ... IF NOT EXISTS ...` que garantizan idempotencia total.
- Sin `START TRANSACTION` ni `COMMIT` internos. El SQL generado por EF Core contiene un par por migración; cada `COMMIT` rompe la transacción externa de `psql --single-transaction` y permite una aplicación parcial.
- Regenerar el SQL en un archivo temporal y preparar el artefacto con `node tools/migrations/prepare-atomic.mjs <sql-generado> tools/migrations/migrations-idempotent.sql`; la herramienta verifica el número de límites y registros. Antes de ejecutar, comprobar de nuevo que no aparezcan límites internos.
- Registro automático en la tabla `__EFMigrationsHistory`.
- Libre de dependencias destructivas (`DROP TABLE` o eliminación de columnas); todas las modificaciones son estrictamente aditivas o de compatibilidad hacia adelante.

---

## 4. Procedimiento de Ejecución del Gate

### Paso 1: Respaldo Inmediato Pre-Vuelo
Ejecutar el script de respaldo para capturar el estado exacto de producción minutos antes del pase:
```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup\Backup-Enlyce.ps1
```
Verificar que el log en `%LOCALAPPDATA%\Enlyce\Backups\backup.log` reporte `OK <nombre>.dump <tamaño> bytes uploaded`.

### Paso 2: Aplicación del Script SQL Atómico
Aplicar el script idempotente mediante `psql` con control de detención ante error y una sola transacción. Primero comparar `__EFMigrationsHistory` con las 22 migraciones previstas y ensayar este archivo en una restauración de la base objetivo:
```bash
psql "$DATABASE_URL" --set ON_ERROR_STOP=1 --single-transaction -f tools/migrations/migrations-idempotent.sql
```

### Paso 3: Verificación de Invariantes y Salud
1. Verificar conteo de tablas y registro en historial:
   ```sql
   SELECT count(*) FROM "__EFMigrationsHistory";
   ```
2. Ejecutar prueba de humo en la API:
   ```bash
   curl -I https://api.enlyce.com/healthz
   ```

### Paso 4: Plan de Reversión (Rollback)
Si el script falla o se interrumpe:
- Con el archivo actual sin `COMMIT` internos, `--single-transaction` revierte toda la ejecución si falla una sentencia. Verificar el historial y el esquema tras el error; no asumir que un archivo regenerado conserva esta propiedad.
- En caso de requerir restauración forzada desde el dump:
  ```bash
  pg_restore --clean --if-exists -d "$DATABASE_URL" "%LOCALAPPDATA%\Enlyce\Backups\<dump_pre_vuelo>.dump"
  ```
