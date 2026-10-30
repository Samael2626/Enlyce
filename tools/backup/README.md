# Respaldo diario de Enlyce

## Diseno

- Windows ejecuta `Backup-Enlyce.ps1` cada dia a las 2:00 AM, hora local.
- Railway CLI transmite un dump PostgreSQL custom; el archivo temporal queda fuera del repositorio. Los nombres nuevos usan fecha y hora UTC.
- El script valida el encabezado `PGDMP` y rechaza archivos de 49 MB o mas antes de subirlos.
- Supabase Storage recibe el archivo en el bucket privado `enlyce-backups` por HTTPS. La API secret nueva usa `apikey`; la legacy `service_role` tambien usa `Authorization: Bearer`.
- La clave se cifra con DPAPI para el usuario de Windows que instalo la tarea. No copiar ese archivo cifrado a otro usuario o equipo.
- Se conservan siete dias de dumps locales tras una subida y retencion remota correctas.
- En Supabase se conservan todas las copias de los ultimos 30 dias, la primera copia de cada mes durante los ultimos 12 meses calendario (UTC, incluido el actual), y al menos las 7 copias validas mas recientes. Se usa `created_at` del servidor, no la fecha del nombre. Fechas futuras se protegen sin descontarlas del minimo de 7 anteriores.
- La retencion solo empieza despues de subir una copia nueva y confirmar en el listado su nombre y tamano. Solo considera archivos `enlyce-yyyyMMdd-HHmmss.dump` con fecha valida, identificador y metadatos de archivo. Carpetas, otros nombres y metadatos incompletos quedan fuera de los borrados.
- Se lista todo antes de borrar, por paginas estables, y se borran lotes de hasta 100 nombres exactos. Un error de listado, una pagina repetida, una copia nueva ausente o un borrado no confirmado hacen fallar la ejecucion. No se continua borrando despues del fallo.
- El Programador ignora ejecuciones simultaneas; un mutex tambien impide dos ejecuciones manuales del mismo usuario/sesion. El bucket debe tener un solo proceso responsable de retencion; no hay bloqueo distribuido frente a escritores o borrados de otros equipos.

## Requisitos

- Windows encendido, conectado a Internet y con la sesion del usuario iniciada a la hora de ejecucion. La tarea inicia al volver a estar disponible si el equipo estaba apagado.
- Railway CLI instalado, autenticado y con el repositorio enlazado al proyecto/entorno de produccion de Enlyce.
- Clave privada `~/.ssh/id_ed25519_enlyce` y permiso para registrar una clave SSH en Railway.
- API secret del proyecto Supabase `Enlyce-backups`, guardada solo en este equipo mediante el instalador.
- El bucket `enlyce-backups` debe seguir privado y conservar limite por archivo inferior a 49 MB.

## Instalar

1. En Supabase Dashboard > Settings > API Keys, crea una API secret nueva con nombre `enlyce-backup-job`. Tambien se acepta la legacy `service_role` si aun no tienes la nueva; no uses `anon`/publishable ni la clave de Postgres. No publiques la secret en chat.
2. En PowerShell, desde la raiz del repo, ejecuta:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\backup\Backup-Enlyce.ps1 -Mode Install
   ```

3. Pega la clave cuando aparezca el prompt seguro. No queda escrita en pantalla ni en el repo.
4. Cada ejecucion registra la clave SSH en Railway solo para hacer el dump y la retira al terminar. Si el proceso se interrumpe, la siguiente ejecucion elimina primero el registro anterior con ese nombre.
5. Ejecuta la tarea manualmente desde el Programador de tareas (`Enlyce-Postgres-Backup`) y confirma un objeto nuevo en Storage > Files.

## Verificacion y restauracion

El log local queda en `%LOCALAPPDATA%\Enlyce\Backups\backup.log`; contiene estado, nombre y tamano, nunca API keys. Cada dump conserva formato PostgreSQL custom. Para restaurar, descarga el `.dump` desde el bucket y usa `pg_restore --list` antes de restaurar sobre una base vacia con version compatible. La restauracion de produccion debe seguir el procedimiento de backup/restore ya validado para Enlyce.

## Aviso externo por fallo o ausencia

1. Crea un check en healthchecks.io con horario `0 2 * * *`, zona `America/Bogota` y periodo de gracia acorde al limite de una hora de la tarea (por ejemplo, 90 minutos). Configura y prueba el canal que recibira las alertas. La ejecucion de Windows usa la hora local del equipo; debe coincidir con esa zona.
2. En el equipo y usuario que ejecutan el backup, guarda la URL del check sin publicarla en Git ni en chat:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\backup\Backup-Enlyce.ps1 -Mode ConfigureMonitoring
   ```

3. Pega la URL `https://hc-ping.com/<uuid>` en el prompt seguro. Se cifra con DPAPI en `healthchecks-url.dpapi`, al lado de la clave de Supabase. Solo se acepta ese host HTTPS y formato UUID, sin sufijo, query ni fragmento. No se imprime la URL ni se envian claves de Supabase a healthchecks.
4. El job avisa `/start`, exito en la URL base o `/fail`, todos con un identificador de ejecucion `rid`. No envia dumps, logs, detalles de excepcion ni cuerpos de peticion. El aviso de exito llega despues de la subida, la retencion y la limpieza local.
5. Si el aviso de inicio falla, se intenta igualmente el respaldo. Si el aviso final no se confirma, la ejecucion falla. Solo se acepta respuesta `OK`; un HTTP 200 con `OK (not found)` no se considera exito. Si no hay URL configurada, el backup funciona pero registra `Monitoreo externo SIN CONFIGURAR`.
6. El servicio externo debe tener el check activado y sus notificaciones operativas. Si Windows esta apagado, la tarea no ocurre o no hay red, detecta la ausencia del aviso esperado; el script no puede enviar un aviso desde un equipo apagado. Verificar esta alerta de ausencia antes de considerar cerrado el monitoreo.

La configuracion y las llamadas a servicios reales no se ejecutan al cargar el script con dot-sourcing. El cambio de codigo no instala ni modifica la tarea existente: actualizar sus archivos y reinstalar la tarea cuando se quiera activar la nueva version.

## Pruebas sin servicios reales

Desde la raiz del repositorio, en Windows PowerShell 5.1 con Pester 3.4 (version comprobada):

```powershell
$result = Invoke-Pester -Script tools/backup/Backup-Retention.Tests.ps1,tools/backup/Backup-Http.Tests.ps1 -PassThru
if ($result.FailedCount -gt 0) { exit 1 }
```

Los tests usan archivos sinteticos y mocks de las peticiones HTTP: no necesitan credenciales, no consultan Railway/Supabase y no borran backups. Cubren retencion y peticiones exactas de listado, subida, borrado y avisos. No sustituyen una restauracion de ensayo ni demuestran que un canal de alertas real este configurado.

Verificacion local: **24 tests correctos, 0 fallos**, ademas del analisis sintactico nativo de PowerShell 5.1. La seleccion de retencion usa ordenacion O(n log n) y memoria O(n); el listado tiene un limite de 100.000 objetos para abortar ante respuestas anormales.

Referencias: [API de Storage](https://supabase.com/docs/reference/self-hosting-storage), [eliminacion de objetos](https://supabase.com/docs/guides/storage/management/delete-objects), [API de pings](https://healthchecks.io/docs/http_api/).

## Limites

- La validacion automatica comprueba firma y tamano, no sustituye `pg_restore --list` ni una restauracion de ensayo.
- La tarea solo puede respaldar mientras Windows y Railway CLI esten disponibles y autenticados.
- La cuota Storage Free y el maximo por objeto pueden cambiar; vigila uso en Dashboard. Los objetos que no correspondan al patron de backup se conservan y requieren revision manual.
- Retener 7 copias o 12 meses no garantiza capacidad de restauracion. Programar ensayos de restauracion separados.
- Monitoreo real pendiente hasta configurar la URL, el horario y el canal de alertas y probar su recepcion y la ausencia de ejecuciones.
