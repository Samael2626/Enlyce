# Checklist de cierre de paridad Wasi -> Enlyce

Estado al 2026-10-24. Alcance: funciones que L&C usa o aprueba para su operacion, verificadas contra Wasi y Enlyce reales. Este checklist complementa [el estado por modulo](estado-paridad-wasi-2026-10-24.md) y [el inventario historico](checklist-paridad-wasi-enlyce.md). Marcar una casilla solo con evidencia reproducible y enlace al cambio o prueba. No guardar datos personales del ZIP en Git.

## 0. Base segura y entrega

- [ ] Mantener todo el trabajo en `codex/paridad-gate` y PR #1 en borrador; no integrar ni limpiar el checkout principal con cambios ajenos.
- [ ] Revisar por separado `ContactActivity`, `ServiceRating`, `PropertyMedia` y sus migraciones del checkout principal; identificar autor, dependencias y cambios propios antes de portar algo.
- [ ] Restaurar un respaldo exacto de la base Railway objetivo en PostgreSQL aislado; comparar esquema e historial de migraciones, aplicar el SQL pendiente, comprobar datos y segunda ejecucion idempotente, y documentar restauracion/retencion/alertas.
- [x] Resolver los avisos y condiciones de licencia de ImageSharp antes de activar el procesamiento de fotos. Magick.NET sustituyó el procesador; NPOI se eliminó para retirar ImageSharp transitivo. Falta `PropertyMedia` y pruebas reales antes de activar fotos.
- [ ] Antes de produccion: aprobar ventana y plan de reversa, desplegar migraciones y codigo en orden, verificar salud y registros, mantener respaldo recuperable. El ensayo en PostgreSQL vacio no cierra este gate.
- [ ] Mantener pruebas de dominio, aplicacion, integracion, CRM, sitio y PWA verdes; documentar la prueba omitida y cualquier riesgo real. Commit, push y nota en vault por entrega.

## 1. Clientes y operacion comercial

- [ ] Crear en Enlyce las etapas `Publicado`, `Cerrado Negativo`, `Seguimiento 2` y `Visita Inmueble`; `Nuevo` ya existe. Verificar permisos, orden y reglas de cambio/borrado.
- [ ] Ejecutar vista previa y carga controlada del XLSX real de 63 clientes solo despues del gate de migraciones; comparar por fila crear/omitir/actualizar, duplicados por correo y telefono, responsable, fechas, estado y campos sin destino. Conservar conteos, no PII.
- [ ] Validar exportacion real de clientes de Enlyce, entrega por correo, filtros, formato y reimportacion; comparar con el flujo Wasi y corregir errores observados.
- [ ] Observar en Wasi importacion/exportacion de demandas y decidir alcance segun uso real de L&C; implementar solo reglas comprobadas.
- [ ] Comparar demandas, porcentajes de cruce, transiciones, permisos por asesor y resultados con inventario propio; cubrir casos limite con pruebas.
- [ ] Publicar gestiones desde ficha y calificaciones tras separar codigo ajeno; validar catalogo, fechas, visibilidad, permisos y correo de invitacion con flujos reales de Wasi.
- [ ] Comparar agenda: estados y acciones, tareas de equipo, eventos generales, recordatorios y Google Calendar; cerrar lo aprobado por L&C.
- [ ] Comparar reportes/ACM: formulas, filtros, periodos, estados, totales y exportaciones con casos reales anonimizados.

## 2. Inmuebles, publicacion y sitio

- [ ] Integrar `PropertyMedia` con autoria aclarada; portar cola de fotos, ensayar migracion e idempotencia sobre restauracion, y probar fotos reales, reintentos, concurrencia y atribucion.
- [ ] Verificar ficha PDF y compartir por WhatsApp con inmueble real, imagenes, datos publicos y privacidad de propietario; corregir diferencias con Wasi.
- [ ] Publicar y verificar CMS/blog en entorno objetivo tras migraciones; comparar borrador, publicacion, SEO y visualizacion real con Wasi.
- [ ] Resolver editor directo desde web: dominio, sesion/cookies y permisos; implementar flujo aprobado.
- [ ] Para cada portal externo requerido: conseguir contrato/API y credenciales de prueba, mapear alta/edicion/baja, estados, errores y reintentos; probar sin afectar anuncios reales.

## 3. Red y funciones restantes

- [ ] Observar en Wasi foro, inmuebles aliados, alianzas y grupos: roles, acuerdos, audiencia, moderacion y ciclo completo; acordar si L&C necesita red multiempresa antes de diseñarla.
- [ ] Recorrer configuracion inicial en Wasi y registrar campos, validaciones y estados; implementar el alta de empresa necesaria.
- [ ] Comparar chat en sesion Wasi real: permisos, entrega, errores y estados; probar la migracion pendiente.
- [ ] Probar PWA en dispositivo real y decidir si hace falta alguna capacidad de la app movil nativa Wasi.
- [ ] Decidir con L&C el alcance de redes sociales, marketing, cuenta, soporte, academia, reputacion y facturacion; observar e implementar solo lo aprobado.

## Criterio de cierre

- [ ] Cada funcion aprobada tiene flujo Wasi observado, matriz de campos/permisos/errores, implementacion Enlyce y prueba de exito/fallo; cada diferencia queda resuelta o aceptada por L&C.
- [ ] Ensayo de migraciones sobre restauracion exacta, validacion posterior al despliegue y plan de reversa documentados.
- [ ] PR #1 revisado, checklist actualizado con evidencia y decision explicita de integracion. Hasta entonces, paridad funcional abierta.

## Evidencia adicional del 24/10/2026

- PR #1: `gh pr view 1` confirmo `isDraft=true` y cabeza `codex/paridad-gate`. El checkout principal no se modifico.
- Railway CLI: la tarea `Enlyce-Postgres-Backup` fallo el 22, 23 y 24/10 por OAuth `invalid_grant`. Tras renovar sesion, `railway status --json` confirmo el proyecto Enlyce. El respaldo manual del 24/10 a las 18:38 termino `OK`, 110.901 bytes y subida al bucket privado; la clave SSH temporal se retiro. Faltan retencion remota y alerta externa ante fallos. El gate de produccion sigue abierto.
- Ensayo aislado con el dump del 21/10: `pg_restore --list` valido 151 entradas; `pg_restore --exit-on-error` en PostgreSQL 18 sin red ni puerto resulto correcto. Antes: 23 migraciones y 24 tablas. Tras `psql -X -v ON_ERROR_STOP=1 --single-transaction -f tools/migrations/migrations-idempotent.sql`: 44 migraciones y 33 tablas. Segunda ejecucion correcta. Los conteos de contactos, demandas, inmuebles y asesores permanecieron en 3, 1, 26 y 1. Ese dump no acredita el estado actual de Railway.
- Historial restaurado: `20261013002606_AgregarExternalRefInmueble` ya estaba aplicada; para ese dump habia 21 migraciones pendientes. El protocolo decia 22 y debe leerse con esta correccion.
- Etapas L&C: [script idempotente](../tools/migrations/seed-lc-customer-stages.sql) probado dos veces tras migrar la copia. `Clientes` contiene las cinco etapas base y las cuatro nuevas en orden 5-8; solo `Nuevo` es predeterminada. No se crearon en produccion. Faltan verificar permisos, cambio y borrado con flujos reales antes de marcar la casilla.
- [Auditoria separada de `ContactActivity`, `ServiceRating` y `PropertyMedia`](auditoria-modulos-ajenos-2026-10-24.md): permanecen no rastreados en el checkout principal; el snapshot EF mezcla los tres. `git log --all` no atribuye esos archivos a un commit ni autor. No se portaron. Falta confirmar procedencia antes de incorporarlos.
- Respaldo actual: `enlyce-20261024-183807.dump`, PostgreSQL 18.6, 151 entradas TOC. Restauracion aislada con `pg_restore --exit-on-error` correcta. Antes del SQL: 23 migraciones, 24 tablas; despues: 44 y 33. Segunda ejecucion idempotente; contactos, demandas, inmuebles y asesores siguieron en 3/1/26/1. Una falla inyectada al final de la transaccion devolvio codigo 1 y dejo una segunda restauracion en 23 migraciones, 24 tablas y los mismos conteos. El script de etapas creo las cuatro en esta copia, orden 5-8; no se ejecuto en Railway.
- Wasi autenticado: en `Importar - Exportar > Exportar Clientes`, el modal solo solicito grupo de clientes (valor `Todos` y tipos disponibles). Al pulsar `Exportar`, Wasi informo que procesaria el archivo y lo enviaria al correo configurado; la notificacion posterior dijo que el archivo estaba listo y adjunto. No se accedio al buzon receptor ni se comprobo adjunto, contenido o reimportacion. No se repitio la importacion Wasi.
- Verificacion de esta entrega: `dotnet test Enlyce.slnx --no-restore` aprobo dominio 256, aplicacion 86 e integracion 299; una prueba de PostgreSQL quedo omitida por configuracion propia. `npm run build` aprobo CRM y sitio Next.js; `npm run test:pwa` aprobo 4/4. `git diff --check` sin errores. ImageSharp 3.1.12 conserva avisos NU1902/NU1903; no activar procesamiento de fotos.
- API actual en Railway: `GET https://enlyce-production.up.railway.app/health` devolvio HTTP 200 con `database=connected` el 24/10; no hubo despliegue. El protocolo ahora usa esa ruta real y evita restaurar con `pg_restore --clean` sobre la base viva. Falta aprobar operador, ventana, RPO/RTO y corte de conexion para una reversa posterior al commit.
- ImageSharp: el usuario eligio evaluar otra biblioteca. [Comparacion y criterio de prototipo](evaluacion-biblioteca-imagenes-2026-10-24.md): Magick.NET-Q8-AnyCPU 15.0.0 es el candidato inicial Apache-2.0; falta prototipo Windows/Linux y pruebas antes de sustituir ImageSharp 3.1.12 o activar fotos.
- Correo de exportacion Enlyce: ahora adjunta XLSX `Clientes` con las mismas 22 columnas de la descarga CSV. Prueba sintetica dirigida: filtro deja una fila, las celdas son texto (sin formulas ejecutables) y `ClientImportTableReader` reabre el adjunto con 22 columnas y una fila. Suite final: dominio 256, aplicacion 86, integracion 300 aprobadas y 1 omitida. La entrega SMTP real y la comparacion de valores con el archivo Wasi siguen abiertas.
- Respaldo automatico: propuesta de 30 diarios y 12 mensuales remotos, con alerta externa por fallo o ausencia de ejecucion. El usuario pidio ayuda para decidir; faltan destinatario/canal, prueba de alerta y autorizacion para borrar objetos antiguos. La retencion remota sigue sin aplicar.

## Evidencia posterior del 24/10/2026

- La tarea `Enlyce-Postgres-Backup` ejecutada manualmente **desde el Programador** a las 19:53 terminó con `LastTaskResult=0` y registró `OK enlyce-20261024-195315.dump 110901 bytes uploaded`. La siguiente ejecución automática está programada para el 25/10 a las 2:00; aún no está comprobada. El destinatario de alertas se acordó fuera de Git; faltan mecanismo emisor, prueba de fallo/omisión y retención remota.
- [Sustitución de ImageSharp y NPOI](evaluacion-biblioteca-imagenes-2026-10-24.md): Magick.NET 15.0.0 y OpenMcdf 3.3.0. `dotnet nuget why` no encontró ImageSharp; la consulta actual de NuGet no reportó paquetes vulnerables en Infrastructure. Pruebas dirigidas Linux: 23/23, incluida orientación EXIF. Suite Windows: dominio 256, aplicación 86, integración 310 aprobadas y 1 omitida. Builds CRM y sitio verdes; PWA 4/4. No hubo despliegue.
- Respaldo del 25/10: `Enlyce-Postgres-Backup` se ejecutó automáticamente a las 2:00:01 con `LastTaskResult=0`; el log registró `OK enlyce-20261025-020002.dump 110901 bytes uploaded` a las 2:00:11. [Monitor de alertas](../tools/backup/Vigilar-Respaldo-Enlyce.ps1): 4/4 casos sintéticos y comprobación de salud real correctos. La tarea se instaló para las 4:00, pero la credencial local guardada no pasó la prueba de correo; alerta externa aún inactiva. Sigue pendiente retención remota y vigilancia independiente del equipo.
- [Vista previa de retención](../tools/backup/Plan-Retencion-Enlyce.ps1) solo lectura: 13 objetos, 1.244.311 bytes, 13 conservados, 0 candidatos y 0 nombres desconocidos con la propuesta de 7 días completos + 30 copias diarias + 12 mensuales. La autoprueba sintética conservó 5, propuso 3 y protegió 2 nombres desconocidos o con fecha inválida. No se descargó ni borró ningún objeto; política y borrado automático siguen sin aprobación.

## Evidencia adicional del 25/10/2026

- El dump automático `enlyce-20261025-020002.dump` se restauró con `pg_restore --exit-on-error` en PostgreSQL 18 aislado (`--network none`, sin puerto). [Conteos reproducibles](../tools/migrations/conteos-gate.sql): antes, 23 migraciones y 24 tablas; después del SQL atómico, 44 y 33. Segunda ejecución con `ON_ERROR_STOP=1` y transacción única terminó en código 0. Contactos/demandas/inmuebles/asesores siguieron en 3/1/26/1. El seed de etapas terminó dos veces en código 0: 9 etapas de clientes, 4 personalizadas. No hubo escritura en Railway.
- [Cruce de demandas Wasi observado](evidencia-cruce-demandas-wasi-2026-10-25.md): en una fila, `En Red 1` abrió un inmueble con `Acierto: 56.6%` y acciones de alianza, detalle y contacto. Se registraron campos del editor sin guardar datos. Enlyce solo cuenta coincidencias propias con regla booleana. Fórmula de porcentaje, permisos, errores y contratos de red siguen pendientes; no se marca la casilla.
- [Gestiones y tareas Wasi observadas](evidencia-gestiones-wasi-2026-10-25.md): la lista global estaba vacía; se abrieron y cancelaron los formularios de registrar llamada, agendar llamada, evento y visita desde una ficha. Se documentaron campos, tipos, estados y avisos. Sin guardado ni correo real, no están verificados permisos, errores o resultados; `ContactActivity` y `ServiceRating` siguen sin atribución.
- [Blog y banners Wasi observados](evidencia-cms-wasi-2026-10-25.md): listas vacías; formularios de alta inspeccionados sin guardar. El blog pidió título, imagen, resumen y cuerpo; banner pidió imagen, título, vínculo y estado activo/inactivo. Falta decidir si L&C necesita banners múltiples, probar publicación/visualización y ensayar migraciones.
- Avisos de tareas: la ficha Enlyce ofrece los intervalos observados en Wasi y fecha personalizada; calcula el recordatorio desde el vencimiento. Build CRM verde. No se probó la entrega del aviso Wasi ni notificación al cliente; agenda sigue abierta.
- [Conexión de portales Wasi observada](contratos-portales-wasi-2026-10-25.md): Fincaraiz y Metrocuadrado aparecen conectados con configuraciones distintas; otros portales gratuitos también. No se leyó credencial ni se publicó anuncio. Faltan contrato/API y decisión L&C por portal antes de implementar conectores.
- [Red Wasi observada](evidencia-red-wasi-2026-10-25.md): alianzas muestran solicitud y aceptar/rechazar, grupos ofrecen ubicación y privacidad, foro permite audiencia todos/aliados y comentarios. No se accionó ningún flujo; faltan acuerdos, permisos, moderación y decisión de alcance multiempresa.
