# Monitoreo de errores

La API envia a Sentry cada `LogError` y cada excepcion no manejada. Sin DSN
configurado no envia nada y los errores quedan solo en los logs de Railway.

## Activar en Railway

1. Crear un proyecto en sentry.io (plataforma ASP.NET Core). El plan gratuito basta.
2. En cada entorno de Railway agregar las variables:
   - `Sentry__Dsn`: DSN del proyecto (secreto; nunca en Git ni en `appsettings.json`).
   - `Sentry__Environment`: `staging` o `production`.
3. Redesplegar. La version enviada es el commit (`RAILWAY_GIT_COMMIT_SHA`).
4. En Sentry, crear una regla de alerta por correo para eventos nuevos o con
   pico de frecuencia, dirigida al responsable tecnico.

## Datos personales (Ley 1581)

`src/Enlyce.Api/Observability/ErrorMonitoring.cs` desactiva `SendDefaultPii`,
no envia cuerpos de solicitud y, antes de enviar, elimina query strings
(tokens de baja de alertas), cookies, cabeceras distintas de
Accept/Content-Type/Content-Length/User-Agent/Host y el usuario. Tambien quita
la query de las migas de pan generadas por los logs de ASP.NET. Verificado con
un receptor local: ningun valor de cookie, `Authorization` ni query llego al
evento. No agregar contexto de usuario ni datos de leads en `LogError`.

## Pendiente

- CRM (React) y sitio publico (Next.js) aun no reportan errores del navegador.
