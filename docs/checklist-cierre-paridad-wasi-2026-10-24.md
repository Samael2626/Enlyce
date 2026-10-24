# Checklist de cierre de paridad Wasi -> Enlyce

Estado al 2026-10-24. Alcance: funciones que L&C usa o aprueba para su operacion, verificadas contra Wasi y Enlyce reales. Este checklist complementa [el estado por modulo](estado-paridad-wasi-2026-10-24.md) y [el inventario historico](checklist-paridad-wasi-enlyce.md). Marcar una casilla solo con evidencia reproducible y enlace al cambio o prueba. No guardar datos personales del ZIP en Git.

## 0. Base segura y entrega

- [ ] Mantener todo el trabajo en `codex/paridad-gate` y PR #1 en borrador; no integrar ni limpiar el checkout principal con cambios ajenos.
- [ ] Revisar por separado `ContactActivity`, `ServiceRating`, `PropertyMedia` y sus migraciones del checkout principal; identificar autor, dependencias y cambios propios antes de portar algo.
- [ ] Restaurar un respaldo exacto de la base Railway objetivo en PostgreSQL aislado; comparar esquema e historial de migraciones, aplicar el SQL pendiente, comprobar datos y segunda ejecucion idempotente, y documentar restauracion/retencion/alertas.
- [ ] Resolver los avisos y condiciones de licencia de ImageSharp antes de activar el procesamiento de fotos.
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
