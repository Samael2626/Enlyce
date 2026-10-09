# Hoja de ruta del CRM inmobiliario

**Estado al 9 de octubre de 2026.** Prioridad inmediata: cerrar CRM comercial. La hoja también define la ruta para incorporar en Enlyce contabilidad y administración de arriendos; facturación electrónica queda sujeta a habilitación DIAN por emisor. No se presupone operar como proveedor tecnológico para terceros.

## Entrega implementada en esta sesión

- [x] Contacto centralizado por correo normalizado, separado de oportunidades; migración enlaza oportunidades históricas.
- [x] Ficha de contacto con oportunidades e historial comercial respetando el alcance por asesor.
- [x] Tareas comerciales con prioridad, vencimiento, recordatorio registrado, reprogramación, cierre, cancelación y comentarios auditables.
- [x] Agenda que muestra tareas y permite operar visitas (reprogramar, marcar realizada con resultado y cancelar).
- [x] Demandas de clientes y cruce manual con inventario disponible; seguimiento de inmueble sugerido, compartido, visitado o descartado.
- [x] Reporte CRM inicial por origen y tipo de operación, junto al embudo anónimo de la web.
- [x] Captaciones web sin publicación asignadas al asesor activo con menor carga abierta; publicaciones conservan su asesor y referencias inválidas quedan en revisión.
- [x] Asignar ya no marca como contactada la oportunidad; la primera interacción del asesor registra `FechaPrimerContacto` y alimenta el promedio de respuesta.
- [x] Permisos de demandas e historial para contactos compartidos: cada asesor ve demandas ligadas a sus oportunidades; prueba encontró y corrigió exposición entre asesores.
- [x] Alertas internas consultan tareas pendientes por vencimiento o recordatorio y permiten completar/reprogramar desde la pantalla.
- [x] Métricas de primera respuesta por asesor, origen y rango inclusivo; solo cuentan oportunidades con respuesta registrada.
- [x] Pruebas de dominio, aplicación y rutas nuevas; compilación de API y frontend.

## Trabajo pendiente, en orden

### P0 — cerrar el flujo comercial

- [x] Validar migraciones, deduplicación, backup y restore en copia anonimizada PostgreSQL 17 (4 leads); antes de cada despliegue, respaldar y restaurar la base objetivo exacta.
- [x] Regla configurable de reparto (menor carga abierta o turnos rotativos), con actor, motivo, origen y hora en el historial.
- [x] Cerrar autorización de contactos compartidos para oportunidades, demandas e historial; restringir mutaciones de demanda al asesor del lead.
- [x] Alertas internas accionables: mostrar tareas pendientes con `DueAt` o `ReminderAt` dentro de ventana y permitir completar/reprogramar. Correo y aviso con navegador cerrado quedan fuera.
- [x] Mostrar primera respuesta por asesor, origen y periodo inclusivo; oportunidades sin respuesta no entran en el promedio.

### P1 — operación y control

- [ ] Reportes por asesor, etapa, fuente/campaña, operación, tiempo de primera respuesta, visitas y cierres.
- [ ] Exportación CSV y filtros por periodo; definir permisos y campos exportables.
- [ ] Búsqueda, filtros, paginación y acciones masivas para contactos, oportunidades y tareas.
- [ ] Historial unificado e inmutable de cambios a oportunidad, asignación y etapa (asignaciones implementadas; cambios de etapa pendientes).
- [ ] Pruebas E2E de contacto → oportunidad → tarea/visita → demanda → cierre.

### P2 — integración y automatización

- [ ] Definir reglas de seguimiento y SLA por fuente y tipo de operación; hacerlas configurables antes de automatizar.
- [ ] Plantillas y notificaciones por correo con proveedor real y registro de entrega.
- [ ] Evaluar WhatsApp Business Platform oficial: consentimiento, ventanas de conversación, plantillas, costos y bandeja compartida.
- [ ] Construir en Enlyce las funciones contables necesarias, sin depender de Siigo ni duplicar datos; empezar por arriendos y cartera después de cerrar CRM.

### Fase propia de contabilidad inmobiliaria — después del CRM comercial

No es una integración con Siigo ni una copia completa de su suite. Enlyce será dueño del proceso y sus datos. El análisis de alcance y fuentes oficiales está en [Siigo dentro de Enlyce](../reports/Siigo/Enlyce.md).

- [ ] Definir operación objetivo: ventas, arriendos o ambas; mapear contrato, propietario, arrendatario, inmueble, obligaciones y responsables.
- [ ] Administración de arriendos: contratos, cánones recurrentes, fechas de vencimiento, pagos parciales/anticipos, mora, reversos, soportes y liquidación al propietario.
- [ ] Cartera y caja: cuentas por cobrar/pagar, recibos, gastos, proveedores, recaudos, categorías y reportes auditables; corregir movimientos mediante reversos, no edición de saldos.
- [ ] Contabilidad: plan de cuentas, comprobantes balanceados de partida doble, cierres y estados financieros; validar reglas con contador antes de uso real.
- [ ] Facturación electrónica: implementar software propio/adquirido y completar habilitación/pruebas para cada emisor. Antes de ofrecer generación/transmisión como servicio de Enlyce a inmobiliarias, confirmar el modelo legal DIAN; la habilitación como proveedor tecnológico es una ruta distinta.
- [ ] Nómina electrónica solo si se define una necesidad y alcance. Importación/conciliación bancaria después. POS fuera salvo caso de negocio.

## Criterios para considerar terminado el CRM inicial

1. Una persona se conserva como contacto único y puede tener varias oportunidades independientes.
2. Cada asesor ve solo los registros que tiene asignados o que una regla explícita le comparte; administración tiene vista global.
3. Se puede llevar una oportunidad desde captación hasta cierre dejando responsable, fechas e historial verificables.
4. Las tareas vencidas y compromisos próximos son visibles y accionables dentro del CRM.
5. Los reportes concilian con los registros consultables y permiten explicar cada indicador.
6. Migraciones probadas sobre copia, suite verde y recorrido E2E antes de tocar producción.

## Verificación de esta entrega

- Domain: 217 pruebas aprobadas.
- Application: 40 pruebas aprobadas.
- Integration: 196 pruebas aprobadas. Incluyen aislamiento de demandas e historial, recordatorios y métricas de primera respuesta.
- Frontend: `npm run build` aprobado.
- SQL EF Core aplicado en copia PostgreSQL anonimizada; no aplicado a producción.

## Avance del 9 de octubre de 2026

- Asignación manual y reasignación ahora rechazan asesores inactivos y administradores como destino.
- La pantalla Alertas incluye tareas pendientes vencidas y próximas a 7 días; avisos por correo o navegador siguen pendientes.
- Verificación de esa entrega: Domain 214, Application 39 e Integration 181 pruebas aprobadas; frontend compila. Ver conteos vigentes en «Verificación de esta entrega».
- En esa entrega se generó SQL para inspección. Estado vigente: migraciones aplicadas en copia PostgreSQL anonimizada; no en producción.
- Sigue pendiente segmentar métricas de respuesta y validar permisos de contacto compartido.

### Auditoría de asignaciones

- Historial inmutable por oportunidad: asesor anterior y nuevo, actor, motivo, origen y fecha UTC.
- Asignaciones automáticas, por publicación y manuales se registran; los cambios manuales exigen motivo.
- API protegida y ficha de oportunidad muestra la cronología. La migración crea una línea base para oportunidades ya asignadas; no reconstruye cambios anteriores que nunca se persistieron.
- Verificación de esa entrega: API build limpio, Domain 217 e Integration 185 pruebas aprobadas; frontend compila. Ver estado vigente de base de datos y pruebas arriba.


### Regla de reparto configurable

- Configuración solo para administradores desde Configuración; el cambio aplica a nuevos leads web sin publicación. Los leads de una publicación conservan su asesor.
- Menor carga abierta sigue como valor inicial; turnos rotativos usa un contador atómico y orden estable de asesores activos.
- El historial identifica cuál regla asignó cada lead. La migración agrega la configuración y el contador con valor inicial.
- Verificación: Domain 217/217, Application 40/40, Integration 189/189; frontend compila. SQL inspeccionado, migración no aplicada.

### Validación de migraciones en PostgreSQL (9 de octubre)

- Script completo aplicado en PostgreSQL 16 aislado; no se conectó a producción.
- Fixture sintético: 3 oportunidades con 2 correos normalizados; migración creó 2 contactos, enlazó las 3 oportunidades y el índice rechazó un correo duplicado.
- FechaPrimerContacto quedó en la primera interacción no web; historial creó la línea base para el lead asignado; configuración inició en LeastOpenLeads con cursor 0.
- pg_dump en formato custom restaurado en otra base: se conservaron 3 leads, 3 interacciones y la versión de migración previa.
- La copia local anonimizada pasó el backup/restore; antes del despliegue debe respaldarse y restaurarse la base objetivo exacta. dotnet ef database update falló al consultar __EFMigrationsHistory ausente en una base vacía; el SQL generado sí aplicó correctamente con psql. Revisar ese flujo si se usará el comando EF en despliegue.


### Backup y restore de copia local (9 de octubre)

- Origen verificado como el contenedor local enlyce-db (PostgreSQL 17), base enlyce, detenido al finalizar. No se accedió a producción.
- Dump custom creado en D:\tmp\enlyce-local-backup-20261009.dump y restaurado en un PostgreSQL 17 efímero; antes de migrar se anonimizaron datos personales y textos libres en la copia.
- La copia restaurada tenía 4 leads y 2 asesores. Con dos correos de prueba normalizados iguales, las migraciones pendientes llegaron a 20261009042459_AddRoundRobinCursor, crearon 3 contactos y enlazaron los 4 leads; el índice único rechazó el duplicado.
- La copia local no tenía leads asignados ni primeras respuestas no web, así que esos dos backfills se habían verificado con el fixture sintético anterior.
- El archivo dump conserva los datos originales locales; no compartirlo ni subirlo al repositorio.


- Repetición: dump nuevo guardado en D:\tmp\enlyce-local-backup-20261009.dump y restore anonimizado validado hasta la migración final; contenedor local de prueba enlyce-restore-confirmation queda disponible, sin puerto publicado. El origen enlyce-db quedó detenido.
