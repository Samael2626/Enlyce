# Pendientes operativos: paridad Wasi -> Enlyce para L&C

Estado al 24/10/2026. Este listado desglosa las casillas abiertas del [checklist de cierre](checklist-cierre-paridad-wasi-2026-10-24.md). Marcar una casilla exige evidencia del flujo Wasi, resultado Enlyce y prueba; código existente por sí solo no basta. No contiene datos personales.

## Ya comprobado; no repetir

- El ZIP real contiene 63 clientes. El ensayo en Wasi creó 19 y rechazó 44; las 19 fichas nuevas fueron conciliadas. No repetir esa importación en Wasi.
- Enlyce lee el XLSX real. La exportación por correo genera un XLSX que una prueba sintética puede reimportar. Falta verificar entrega SMTP y datos reales.
- El dump Railway del 24/10 se restauró en PostgreSQL 18 aislado. El SQL pasó de 23 a 44 migraciones y de 24 a 33 tablas, fue idempotente y revirtió una falla inyectada. Los conteos 3/1/26/1 de contactos/demandas/inmuebles/asesores se conservaron. No hubo migración ni despliegue en producción.
- Las cuatro etapas del embudo se crearon **solo en la copia aislada**. PR #1 continúa en borrador.

## 1. Gate Railway y respaldo

- [x] Confirmar respaldo automático tras renovar Railway CLI: tarea del 25/10 a las 2:00, `LastTaskResult=0`, log `OK enlyce-20261025-020002.dump 110901 bytes uploaded`. Falta retención y alerta activa.
- [ ] Definir e implementar retención remota y alerta externa por fallo **y** ejecución omitida. Propuesta por validar: 30 diarios y 12 mensuales. Acordar destino de alerta y probar ambos eventos; no borrar respaldos remotos sin autorización.
- [ ] Justo antes del pase, obtener un dump nuevo de la base objetivo, verificar integridad, restaurarlo en PostgreSQL aislado y comparar esquema, historial, conteos y huellas relevantes. Repetir SQL, segunda ejecución y rollback de falla sobre esa copia; archivar evidencia sin datos personales.
- [ ] Cerrar protocolo de reversa: operador, ventana, corte de conexiones, RPO/RTO, secuencia de restauración y comprobaciones de salud. Pedir autorización expresa para cualquier migración o despliegue en producción. No usar `pg_restore --clean` sobre la base viva.

## 2. Clientes y demandas

- [ ] Crear en Enlyce objetivo `Publicado`, `Cerrado Negativo`, `Seguimiento 2` y `Visita Inmueble` después del gate. Verificar orden, permisos, transiciones y borrado; `Nuevo` ya existe.
- [ ] Antes de cargar, previsualizar las 63 filas del XLSX real y registrar por fila la decisión crear/omitir/actualizar, duplicados por correo/teléfono, responsable, fechas, estado y campos sin destino. Conservar solo conteos y referencias anonimizadas.
- [ ] Cargar controladamente los 63 clientes en Enlyce **solo tras** gate y etapas; conciliar resultados y comprobar que reejecutar el lote no duplica fichas. No repetir importación en Wasi.
- [ ] Validar exportación real de clientes Enlyce: filtros, entrega SMTP, adjunto XLSX, valores y reimportación. Comparar con una exportación Wasi observada sin guardar datos personales en Git.
- [ ] Observar en Wasi importación/exportación de demandas con sesión real; documentar columnas, validaciones, rechazo por fila y permisos. Implementar lo aprobado con pruebas de éxito y error.
- [ ] Comparar alta, edición, estados y cruce de demandas con inmuebles propios: porcentajes, filtros, límites y acceso por asesor. Probar resultados concretos de Wasi en Enlyce.

## 3. Gestiones, calificaciones e inmuebles

- [ ] Atribuir y separar `ContactActivity`, `ServiceRating` y `PropertyMedia` del checkout principal. Revisar dependencias, DI, migraciones y snapshot EF por módulo; incorporar solo código trazable y probado en `codex/paridad-gate`.
- [ ] Observar y reproducir gestiones desde ficha: tipos, fechas, autor, visibilidad, edición/borrado, permisos y errores. Probar API, persistencia e interfaz.
- [ ] Observar y reproducir calificaciones: criterios, cálculo, permisos, invitación por correo, respuesta y errores. Probar casos reales anonimizados.
- [x] Sustituir ImageSharp vulnerable: Magick.NET-Q8-AnyCPU 15.0.0 pasó pruebas de formato, límites y orientación en Windows y Linux (23 dirigidas). NPOI se retiró por su EULA y ImageSharp transitivo; el lector XLS/XLSX usa ExcelDataReader y OpenMcdf. Auditoría NuGet sin paquetes vulnerables en Infrastructure. Fotos aún apagadas por `PropertyMedia`.
- [ ] Integrar fotos y cola solo tras separar `PropertyMedia` y ensayar migración sobre copia restaurada; probar fotos reales autorizadas, reintentos, concurrencia, atribución y acceso.
- [ ] Comparar ficha PDF y compartir por WhatsApp con inmueble real: imágenes, campos públicos, privacidad de propietario y errores.

## 4. Agenda, reportes, sitio y chat

- [ ] Comparar agenda Wasi: tareas personales/de equipo, eventos, estados, recordatorios y Google Calendar; implementar el alcance aprobado y probar permisos y resultados.
- [ ] Comparar reportes y ACM: fórmulas, filtros, períodos, estados, totales y exportación usando casos anonimizados; cubrir divergencias con pruebas.
- [ ] Probar CMS/blog en el entorno objetivo tras migraciones: borrador, publicación, SEO y visualización. Resolver edición desde web con sesión, dominio, cookies y permisos.
- [ ] Observar chat Wasi en sesión real: destinatarios, permisos, entrega, estados y errores; ensayar migración pendiente y probar equivalente Enlyce.
- [ ] Probar PWA en dispositivo real; decidir por función si se requiere alguna capacidad de la app nativa Wasi.

## 5. Contratos y decisiones de alcance

- [ ] Para cada portal que L&C use: identificar contrato/API, credenciales de prueba, campos, alta/edición/baja, estados, errores y reintentos. Probar sin afectar anuncios reales.
- [ ] Observar foro, inmuebles aliados, alianzas y grupos; acordar con L&C roles, acuerdos, audiencia, moderación y si requiere red multiempresa antes de implementarla.
- [ ] Recorrer configuración inicial de empresa en Wasi y registrar campos, validaciones y permisos; implementar el alta necesaria.
- [ ] Decidir con L&C alcance de redes sociales, marketing, cuenta, soporte, academia, reputación y facturación. Documentar contrato o exclusión aprobada por función.

## 6. Cierre

- [ ] Por cada función aprobada: flujo Wasi observado, matriz de campos/permisos/errores, equivalencia API/persistencia/UI, pruebas de éxito y rechazo, y diferencia resuelta o aceptada por L&C.
- [ ] Mantener verdes `dotnet test Enlyce.slnx`, builds CRM/sitio y pruebas PWA aplicables; registrar la prueba de PostgreSQL omitida y cualquier riesgo sin ocultarlo.
- [ ] Actualizar este listado y el checklist de cierre con evidencia; hacer commit y push solo con pruebas verdes, verificar nota en vault y mantener PR #1 en borrador hasta revisión y decisión explícita de integración.
