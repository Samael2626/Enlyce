# Protocolo de Migraciones en Producción — Enlyce

## 1. Estado y Contexto de Producción

- **Base de datos objetivo:** PostgreSQL en Railway (`dbname: railway`, versión 18 / 17 compatible).
- **Almacenamiento de respaldos:** Bucket privado de Supabase Storage `enlyce-backups`.
- **Tarea programada de respaldo diario:** `Enlyce-Postgres-Backup` ejecutada automáticamente mediante `tools/backup/Backup-Enlyce.ps1` hacia `%LOCALAPPDATA%\Enlyce\Backups`.
- **Último respaldo restaurado y ensayado:** dump automático `enlyce-20261025-020002.dump` (110.901 bytes), PostgreSQL custom. Se restauró con `pg_restore --exit-on-error` en PostgreSQL 18 sin red ni puerto; el ensayo se detalla abajo. Antes de un pase autorizado se exige otro respaldo inmediato.
- **Política de congelamiento:** Ninguna migración nueva se aplica a producción sin autorización explícita del usuario y verificación de respaldo previo.

---

## 2. Cadena de Migraciones Pendientes en Producción

El dump actual de Railway del 24/10/2026 contiene 23 migraciones, incluida `20261013002606_AgregarExternalRefInmueble`. Quedan 21 de las 22 migraciones listadas abajo. Se debe volver a comparar el historial con un respaldo inmediato antes de cualquier pase a producción. Las suites de dominio e integración pasan en el checkout aislado; esto no acredita una aplicación de todas las migraciones en PostgreSQL:

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

**Ensayo local 2026-10-24:** PostgreSQL 17 desechable, base vacía: 44 migraciones aplicadas, segunda ejecución idempotente y fallo inyectado revertido sin tablas residuales. La primera ejecución detectó un punto y coma ausente en `AddCustomerFunnelStages`; se corrigió la migración fuente y se regeneró el SQL. Falta restaurar y ensayar la copia exacta de la base objetivo. El ensayo local no autoriza aplicar el script en Railway.

**Ensayo adicional 2026-10-24 con respaldo antiguo:** el dump local del 21/10 pasó `pg_restore --list` (151 entradas) y se restauró en PostgreSQL 18 aislado. Historial/esquema: 23 migraciones y 24 tablas antes; 44 y 33 tras aplicar el SQL pendiente con `ON_ERROR_STOP=1 --single-transaction`. La segunda aplicación fue correcta. Contactos, demandas, inmuebles y asesores mantuvieron sus conteos de 3, 1, 26 y 1. La tarea de backup falló los días 22-24/10 y Railway CLI informa OAuth `invalid_grant`; el dump del 21/10 no reemplaza el respaldo actual exigido para abrir el gate. El script de las cuatro etapas L&C se ensayó dos veces sobre esta copia: `tools/migrations/seed-lc-customer-stages.sql`. No se ejecutó en producción.

**Ensayo con respaldo actual del 24/10 a las 18:38:** tras renovar Railway CLI, `Backup-Enlyce.ps1 -Mode Run` creó y subió un dump de 110.901 bytes; la clave SSH temporal se retiró. PostgreSQL 18.6, 151 entradas TOC y restauración aislada correcta. El SQL llevó la copia de 23 migraciones/24 tablas a 44/33; segunda ejecución idempotente. Los conteos de contactos/demandas/inmuebles/asesores siguieron en 3/1/26/1. Otra copia restaurada recibió una falla `SELECT 1/0` al final de la misma transacción: código 1 y estado intacto de 23/24 con los mismos conteos. Las cuatro etapas L&C se crearon únicamente en la copia. Falta definir retención remota y alerta externa del backup, plan de reversa operativo y autorización para cualquier aplicación en producción.

**Ensayo con el respaldo automático del 25/10 a las 2:00:** `pg_restore --exit-on-error` terminó correctamente en PostgreSQL 18 aislado. [Consulta de conteos](../tools/migrations/conteos-gate.sql): 23 migraciones/24 tablas antes, 44/33 después; segunda aplicación atómica con código 0. Contactos/demandas/inmuebles/asesores: 3/1/26/1 antes y después. El seed de las cuatro etapas se ejecutó dos veces con código 0; quedaron nueve etapas de clientes, cuatro personalizadas. No hubo escritura en Railway. La reversión por falla inyectada se probó sobre el dump del 24/10 y debe repetirse si cambia el SQL.

### Paso 1: Respaldo Inmediato Pre-Vuelo
Ejecutar el script de respaldo para capturar el estado exacto de producción minutos antes del pase:
```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup\Backup-Enlyce.ps1 -Mode Run
```
Verificar que el log en `%LOCALAPPDATA%\Enlyce\Backups\backup.log` reporte `OK <nombre>.dump <tamaño> bytes uploaded`.

### Paso 2: Aplicación del Script SQL Atómico
Aplicar el script idempotente mediante `psql` con control de detención ante error y una sola transacción. Primero comparar `__EFMigrationsHistory` con la cadena prevista (21 pendientes en el dump del 24/10; recalcular sobre el respaldo inmediato) y ensayar este archivo en una restauración de la base objetivo:
```bash
psql "$DATABASE_URL" --set ON_ERROR_STOP=1 --single-transaction -f tools/migrations/migrations-idempotent.sql
```

### Paso 3: Verificación de Invariantes y Salud
1. Verificar conteo de tablas y registro en historial:
   ```sql
   SELECT count(*) FROM "__EFMigrationsHistory";
   ```
2. Ejecutar prueba de humo en la API. El 24/10/2026 la URL y ruta reales respondieron HTTP 200 con `database=connected`:
   ```bash
   curl -i https://enlyce-production.up.railway.app/health
   ```

### Paso 4: Plan de Reversión (Rollback)
Si el script falla antes de confirmar la transacción, detener el pase. El archivo actual sin `COMMIT` internos revierte toda la ejecución con `--single-transaction`; comprobar historial, tablas y conteos antes de reintentar. La falla inyectada sobre la restauración del 24/10 dejó 23 migraciones y 24 tablas, igual que el origen. Regenerar el SQL obliga a repetir ese ensayo.

Si la migración confirma pero falla el código nuevo, detener escrituras y volver a la versión anterior de la API y el sitio. Las migraciones actuales son aditivas; verificar que el código anterior funciona con el esquema nuevo antes de abrir tráfico. No borrar columnas ni tablas para simular una reversión.

Si se corrompen datos tras confirmar, mantener el respaldo previo recuperable, restaurarlo en **otra** base aislada y comparar historial, esquema y conteos con el registro pre-vuelo. Calcular las escrituras posteriores que se perderían y obtener autorización expresa para el corte a esa base. No ejecutar `pg_restore --clean` contra la base viva: borraría datos posteriores al respaldo. El responsable, ventana, RPO/RTO y mecanismo de cambio de conexión siguen pendientes de aprobación.

### Decisiones para autorizar el pase

- Registrar quién opera Railway y quién confirma el resultado de L&C; fijar ventana con hora y zona `America/Bogota`.
- Definir cuánto dato posterior al respaldo podría perderse (RPO) y cuánto tiempo puede estar indisponible Enlyce (RTO). No asumir cero: el respaldo diario y una eventual restauración implican pérdida potencial de escrituras recientes.
- Acordar cómo detener nuevas escrituras, informar a usuarios y comprobar que no quedan operaciones en curso antes del respaldo inmediato. Mantener la instancia antigua disponible hasta verificar la nueva.
- Preparar base alternativa vacía y credenciales de conexión para restauración; ensayar el cambio de conexión y la vuelta a la instancia anterior sin modificar la base viva. Registrar conteos, historial y `GET /health` antes y después.
- Si falla el SQL antes del commit, comprobar reversión de la transacción y no desplegar. Si falla el código, volver a la versión anterior compatible y conservar el esquema aditivo. Si se requiere restaurar datos, calcular pérdida desde el respaldo, pedir aprobación específica para ese corte y cambiar la conexión solo después de restaurar y verificar la base alternativa.
- La autorización final debe mencionar el dump inmediato, el ensayo sobre él, el plan de corte y la versión de código. Hasta entonces no aplicar SQL, seed, carga de clientes ni despliegue en producción.
