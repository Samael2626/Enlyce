# Respaldo diario de Enlyce

## Diseno

- Windows ejecuta `Backup-Enlyce.ps1` cada dia a las 2:00 AM, hora local.
- Railway CLI transmite un dump PostgreSQL custom; el archivo temporal queda fuera del repositorio.
- El script valida el encabezado `PGDMP` y rechaza archivos de 49 MB o mas antes de subirlos.
- Supabase Storage recibe el archivo en el bucket privado `enlyce-backups` por HTTPS. La API secret nueva usa `apikey`; la legacy `service_role` tambien usa `Authorization: Bearer`.
- La clave se cifra con DPAPI para el usuario de Windows que instalo la tarea. No copiar ese archivo cifrado a otro usuario o equipo.
- Se conservan siete dias de dumps locales tras una subida correcta. Los objetos de Supabase no se borran automaticamente.

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

## Limites

- La validacion automatica comprueba firma y tamano, no sustituye `pg_restore --list` ni una restauracion de ensayo.
- La tarea solo puede respaldar mientras Windows y Railway CLI esten disponibles y autenticados.
- La cuota Storage Free y el maximo por objeto pueden cambiar; vigila uso en Dashboard. Los objetos remotos se acumulan hasta borrado manual.
- El monitor por correo es opcional hasta instalar `Vigilar-Respaldo-Enlyce.ps1`; revisa el log y la fecha del objeto mas reciente mientras tanto.

## Monitor de fallos y ejecuciones omitidas

`Vigilar-Respaldo-Enlyce.ps1` consulta la tarea diaria y la ultima linea `OK` del log a las 4:00 a. m. Distingue tarea no ejecutada, resultado distinto de cero y subida no confirmada. Solo envia una alerta por fecha y estado. Los correos se configuran fuera de Git; la contrasena de aplicacion se cifra con DPAPI para el usuario de Windows.

1. En la cuenta Gmail remitente, activa verificacion en dos pasos y crea una [contrasena de aplicacion](https://support.google.com/accounts/answer/185833). No la pegues en el chat ni en el repositorio.
2. En PowerShell, desde el checkout que conservara el script, ejecuta:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\backup\Vigilar-Respaldo-Enlyce.ps1 -Mode SelfTest
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\backup\Vigilar-Respaldo-Enlyce.ps1 -Mode Install
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\backup\Vigilar-Respaldo-Enlyce.ps1 -Mode TestEmail
   ```

3. Confirma recepcion del correo de prueba. `TestEmail` solo comprueba aceptacion SMTP; la recepcion se verifica en el buzon. Ejecuta `-Mode Check` para consultar el respaldo actual.

El monitor usa Gmail SMTP con TLS en el puerto 587. Necesita que Windows este encendido y la sesion del usuario abierta; si el equipo permanece apagado, la alerta sale al volver a ejecutarse la tarea. Un observador externo independiente aun no esta instalado. La retencion remota de 30 diarios y 12 mensuales sigue pendiente de aprobacion y ensayo; este monitor no borra objetos.

## Vista previa de retencion remota

`Plan-Retencion-Enlyce.ps1` lista metadatos del bucket privado y calcula candidatos sin borrar ni descargar dumps. Conserva todas las copias de los ultimos siete dias, la copia mas reciente de cada dia durante los 30 dias recientes y la mas reciente de cada mes durante los 12 meses anteriores. El respaldo mas reciente siempre queda protegido, aunque todos sean antiguos. Los objetos con nombres desconocidos o fechas invalidas tambien quedan protegidos. La vista previa muestra el total de objetos, bytes y los nombres candidatos. Ejecuta `-Mode SelfTest` y luego `-Mode Preview` desde PowerShell. La politica y cualquier borrado remoto requieren aprobacion expresa; el script no tiene modo de borrado.
