# Gestiones y tareas Wasi: observacion autenticada del 25/10/2026

Sesión L&C. Se abrieron formularios desde una ficha existente y se cancelaron sin guardar. No se copiaron valores de clientes ni se generaron correos.

## Flujos observados

- `Gestiones realizadas` (`/usuario/crm`) mostró filtros por tipo de gestión, nombre de cliente, rango de fecha y usuario encargado. El selector de tipo ofrece `Todos`, `Llamada`, `Correo`, `Evento` y `Visita`. La tabla tiene columnas tipo, asunto y cliente; en esta sesión mostró `Sin datos que mostrar`. No se pudo observar edición, borrado ni resultado de una gestión existente.
- La ficha de cliente contiene pestañas `Correos`, `Llamadas`, `Eventos` y `Visitas`. `Llamadas` ofrece `Registrar llamada` y `Agendar llamada`; su tabla muestra llamada y estado. `Eventos` ofrece `Agregar evento` y muestra evento y estado. `Visitas` ofrece `Agregar Visita` y muestra visita, observaciones, estado, calificación y visitado.
- `Registrar llamada` abrió cliente, ficha de inmueble opcional, asunto, fecha, hora y detalles/resultados de la llamada, con acciones guardar y cancelar. Se canceló.
- `Agendar llamada`, `Agregar evento` y `Agregar Visita` abrieron un formulario de tarea: tipo, cliente, opción de notificar al cliente, asunto, estado, ficha de inmueble, asignado, fecha, hora, aviso y detalles/notas. El tipo ofrece `Llamada`, `Evento`, `Visita`; el estado, `Sin iniciar`, `En proceso`, `Terminada`. El aviso ofrece ninguno, a la hora, 5/10/15/30 minutos, 1/2 horas o 1/2 días antes. Se cancelaron los tres formularios.

## Contraste y límites

- Enlyce tiene `CommercialTask` con tipo, vencimiento, recordatorio, responsable y eventos de historial. En la interfaz de contacto ofrece `Llamada`, `Mensaje`, `Reunión`, `Seguimiento` y `Otro`. El módulo `ContactActivity` del checkout principal sigue sin autor atribuible y no está en `codex/paridad-gate`; por eso `Registrar llamada` no tiene una equivalencia completa publicada.
- No se verificaron validaciones al guardar, reglas de notificación por correo, permisos por rol, transiciones, borrado, calificación de visita ni entrega de avisos. Los nombres de campos y controles no bastan para afirmar esos comportamientos.
- Antes de portar gestiones o calificaciones: atribuir y separar `ContactActivity`/`ServiceRating`, observar una acción completa autorizada en Wasi y probar persistencia, API y UI en Enlyce.

## Ajuste Enlyce en la rama

- La ficha de contacto ahora ofrece los intervalos de `Aviso` observados y conserva fecha personalizada. Calcula `ReminderAt` desde la fecha de la tarea y lo envía al API existente; `npm run build` del CRM pasó. Es un recordatorio de Enlyce: no se probó entrega de correo ni equivalencia del mecanismo de aviso Wasi.
