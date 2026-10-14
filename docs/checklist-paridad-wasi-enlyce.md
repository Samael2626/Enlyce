# Checklist de paridad Wasi → Enlyce

**Corte:** 14 de octubre de 2026  
**Objetivo:** llevar a Enlyce las funciones de Wasi usadas por L&C, sin rehacer funciones que ya existen.  
**Referencias externas para todo Enlyce:** ver [radar de repositorios externos](radar-repositorios-externos.md), que cubre CRM, inmuebles, red, agenda, web, finanzas y operacion.
**Fuentes:** 27 JSON de interfaz en `C:\Users\USUARIO\Downloads\Wasi\Inventarios`, capturas compartidas, ayuda oficial de Wasi y código actual de Enlyce. Algunos JSON iniciales incluyen texto de filas; no copiar esos datos a esta checklist ni al vault.

## Estados

- [x] Cubierto: función existente en Enlyce con alcance comparable.
- [~] Parcial: existe una parte, falta completar paridad o validar comportamiento.
- [ ] Faltante: no se encontró implementación equivalente.
- [?] Sin verificar: evidencia insuficiente para definir el comportamiento esperado.

## Inventario funcional

| Área Wasi | Estado Enlyce | Evidencia / brecha |
|---|---|---|
| Escritorio y métricas del equipo | [~] | Enlyce tiene panel; verificar equivalencia de widgets, periodos y accesos directos de Wasi. |
| Foro inmobiliario | [ ] | Wasi permite publicar y consultar actividad; no hay módulo equivalente. |
| Agenda y tareas | [~] | Tareas y visitas existen; faltan agenda día/semana/mes por equipo, Google Calendar, eventos generales y filtros Pendiente/Vencida/Terminada. |
| Alta, lista y ficha de clientes | [~] | Contactos, oportunidades y demandas existen. Contacto guarda tipo/estado Wasi, ubicación y dirección; el directorio filtra por tipo, estado, asesor (admin), texto y fechas. La ficha privada permite teléfono fijo, nacimiento, identificación, referido, medio de captación y observaciones; migración preparada, no aplicada, contactos existentes quedan sin esos datos. Demanda admite presupuesto/moneda, rango de área, habitaciones, baños, garajes y detalles libres; país y departamento participan en el cruce. Faltan filtro de embudo, características internas/externas estructuradas y registro de correos/llamadas/eventos. |
| Embudo de clientes | [~] | Pipeline Enlyce maneja oportunidades de venta/arriendo. Wasi gestiona clientes con Nuevo/En proceso/Convertido/Recuperado/Perdido y adjuntar cliente; no asumir equivalencia. |
| Gestiones realizadas | [~] | Interacciones, tareas e historial existen; el formulario de leads ahora envía el GUID del asesor autenticado (antes enviaba email y chocaba con la API). Falta comparar filtros, tipos, asuntos y presentación de la lista Wasi. |
| Demandas y cruce cliente → inmuebles | [~] | Coincidencias propias cubiertas. Wasi separa Propios/Aliados/En Red y filtra por cliente, tipo/estado, asesor y rangos de fecha; aliados/red faltan. |
| Cruce inmueble → clientes posibles | [x] | Conteo por página y detalle existen, con el mismo alcance por asesor. |
| Inmuebles vinculados al cliente y estado del vínculo | [~] | Vínculos y estados existen; historial de cada cambio de estado sigue pendiente de aprobación/migración. |
| Lista, ficha y edición de inmuebles | [~] | Enlyce lista/crea inmuebles. Faltan edición completa y paridad de filtros; Wasi añade mapa/lista, papelera, selección masiva, orden, búsqueda y búsqueda avanzada. |
| Importación de inmuebles | [~] | Importación desde API Wasi en segundo plano existe. Wasi también importa XLS/XLSX/CSV y otras fuentes; falta cubrir formatos aplicables. |
| Exportación de inmuebles | [?] | Wasi muestra acceso de importar/exportar; el flujo de exportación no quedó observado. |
| Importación de clientes | [~] | CSV/XLS/XLSX; Wasi documenta mapeo de nombre (obligatorio), apellidos, email y teléfono, con opción de ignorar columnas. Enlyce agrega mapeo de tipo/estado/ubicación/permiso de correo, vista previa, validación, confirmación, deduplicación dentro del alcance visible, CSV de errores sin datos personales e idempotencia por contenido/mapeo. No hay evidencia de importar demandas en Wasi; la captura de L&C no muestra mapeo ni resultados. |
| Exportación de clientes | [~] | Enlyce exporta contactos/CRM en CSV. Wasi permite elegir grupo y envía el archivo al correo; falta paridad. |
| Inmuebles de aliados | [ ] | Wasi lista inmuebles compartidos y permite republicación; Enlyce carece de red multiempresa. |
| Alianzas, propietarios aliados y grupos | [ ] | Faltan modelo multiempresa, permisos, invitaciones, solicitudes recibidas/aceptadas, acuerdos y grupos. |
| Mercado inmobiliario | [?] | Pantalla Wasi detectada, contenido dinámico y métricas no identificados. |
| Reporte general | [~] | Analítica CRM/web existe; faltan totales comparables de inventario/negocios/gestiones y exportación Wasi a Excel/PDF. |
| Reportes de negocios, usuarios, propietarios y sitio web | [~] | Hay reportes CRM y web; los reportes específicos de Wasi requieren comparación de filtros y métricas. |
| Correos enviados | [ ] | Wasi muestra reporte de envíos; Enlyce aún no tiene correo saliente integrado con registro de entrega. |
| Usuarios y asesores | [~] | Autenticación y roles existen; falta comparar administración de usuarios, estados, permisos y campos de Wasi. |
| Sitio web inmobiliario | [~] | Catálogo público y publicaciones existen; faltan funciones de administración y configuración web de Wasi. |
| Portales inmobiliarios | [~] | Enlyce administra publicaciones, pero no está probada publicación directa/sincronización con portales. Wasi conecta portales externos. |
| Compartir fichas por PDF, WhatsApp y correo | [~] | Revisar generación/compartición de fichas; correo y WhatsApp no están integrados como canales CRM. |
| Marketing web (plantillas, dominio, formularios, blog y banners) | [~] | Catálogo y captación pública existen; faltan editor de sitio, dominios, plantillas, blog y banners equivalentes. |
| Aplicación móvil | [ ] | Wasi ofrece aplicaciones Android/iOS; Enlyce no tiene app móvil identificada. |
| Publicación en redes sociales | [~] | Wasi muestra controles para Facebook e Instagram en inmuebles; paridad de publicación no comprobada. |
| Control de portales y publicación masiva | [~] | Enlyce gestiona publicaciones propias; falta verificar integración, sincronización, errores y publicación masiva en portales Wasi. |
| Búsqueda y operaciones del inventario | [~] | Faltan comparar filtros avanzados, mapa/lista, selección y acciones masivas, papelera y recuperación. |
| Configuración inicial de empresa | [ ] | Wasi presenta un asistente de puesta en marcha; Enlyce no muestra equivalente. |
| Configuración comercial | [~] | Reglas de reparto y SLA existen; comparar el resto de ajustes visibles en Wasi. |
| Mi Cuenta, reputación, academia y soporte | [ ] | Accesos visibles en Wasi; sin equivalentes CRM identificados en Enlyce. |
| Chat interno | [ ] | Enlyce muestra placeholder; Wasi muestra chat y contactos de red. |
| Plan y facturación | [~] | Enlyce tiene checkout y estado de pago; comparar gestión de plan, factura y cuenta frente a Wasi. |

## Orden de trabajo acordado

1. **Importar clientes (parcial):** CSV/XLS/XLSX, API de vista previa/confirmación, contactos independientes, asignación privada, campos mapeables, CSV de filas omitidas y reintentos idempotentes por usuario/clave/hash de contenido y mapeo. Wasi documenta nombre obligatorio, apellidos, email y teléfono; permite ignorar otras columnas. Email opcional y repetible; consentimiento de datos y permiso de correo no se mezclan. La bitácora registra fuente/actor/fecha de registro; la fecha original del consentimiento no se conoce si el archivo no la trae. La guía no documenta importación de demandas; no inventar ese requisito. Exportación Wasi selecciona grupo y envía el archivo por correo, flujo que falta en Enlyce.
2. **Directorio y ficha de cliente (en curso):** tipo/estado Wasi, apellidos, ubicación y dirección editables; directorio filtra tipo/estado/asesor/fechas y conserva alcance por asesor. Falta filtro de embudo, campos privados del formulario, eventos y paridad de gestiones.
3. **Cerrar ciclo de importación/exportación:** resultados por fila, errores descargables, reintentos seguros y exportación con filtros/alcances equivalentes.
4. **Completar paridad de inmuebles:** validar campos/formulario y determinar si hace falta importación por archivo además de la integración Wasi existente.
5. **Red inmobiliaria:** acordar modelo multiempresa, invitaciones, visibilidad, publicación, comisiones y auditoría antes de implementarla.
6. **Módulos restantes:** agenda/eventos, reportes, usuarios, sitio web/portales, foro, chat, app móvil, configuración inicial y funciones de cuenta. No sacar funciones de alcance; completar el inventario hasta cubrir Wasi.

## Criterio de cierre por módulo

- [ ] Flujo Wasi observado de inicio a fin; reglas no inferidas solo de nombres de campos o capturas.
- [ ] Campos, validaciones, permisos, filtros y errores documentados sin copiar datos personales.
- [ ] Enlyce implementado en API, dominio, persistencia e interfaz cuando aplique.
- [ ] Casos de éxito, permisos, duplicados y fallos cubiertos por pruebas.
- [ ] Migraciones revisadas con respaldo y restore de la base objetivo antes de producción.
- [ ] Captura visual y diferencias restantes anotadas aquí.

## Límites de la evidencia actual

- Los JSON muestran rutas, etiquetas, tipos de control y encabezados; no revelan de forma fiable validaciones, reglas del servidor ni resultados de acciones.
- Las capturas aclaran pantallas de clientes, cruces, agenda, inmuebles y reportes; faltan recorridos interactivos de importación, alianzas, ajustes y otros flujos.
- El inventario combina funciones visibles en la cuenta L&C y catálogo público oficial; confirmar disponibilidad por plan/país antes de replicar integraciones externas.

## Hallazgos de auditoría del código

- El importador crea contactos independientes sin oportunidades y admite email opcional; el acceso combina propietario asesor y oportunidades asignadas.
- El matcher cubre algunos criterios, pero no replica todos los campos ni filtros Wasi. El cruce cliente→inmuebles es parcial mientras falten aliados y red.
- El importador Wasi de inmuebles es API en segundo plano; no equivale al importador Excel/CSV documentado por Wasi.
- Chat sigue como placeholder. Faltan UI equivalente para red, usuarios y reportes; el importador de clientes ya tiene UI básica.
- Enlyce ofrece calendario con tareas/visitas, analítica CRM/web y checkout; no prueba paridad con agenda/equipos, reportes y ciclo de cuenta Wasi.

## Ajuste importador (14 oct 2026)

- Ahora mapea nombre, apellidos, email, teléfono y «Recibe correo» por separado. Solo valores afirmativos explícitos activan correos automáticos; el permiso de tratamiento de datos no se infiere.
- El correo puede repetirse en varios contactos, igual que en Wasi. La vista previa detecta repeticiones, pero no expone emails ajenos al alcance del asesor.
- Administrador puede conceder o revocar el permiso desde la ficha; conceder exige una referencia de evidencia. El historial registra estado, fuente, actor y fecha de registro, en tabla append-only consultable solo por administradores.
- La importación registra como fuente la columna «Recibe correo» y como actor quien confirma. La fecha es la de registro en Enlyce; la fecha original del permiso no se reconstruye. Contactos previos a la migración no reciben eventos inventados.
- Migraciones preparadas, no aplicadas. Contactos existentes quedan con `CanReceiveAutomatedEmail=false`; verificar evidencia real antes de habilitar envíos.

## Fuentes oficiales Wasi

- [Importar y exportar clientes](https://ayuda.wasi.co/es/article/como-importar-exportar-clientes): formatos, mapeo de columnas, nombre requerido y exportación por grupo al correo.
- [Importar y exportar inmuebles](https://ayuda.wasi.co/es/article/como-importar-exportar-inmuebles): archivo Excel/CSV, mapeo de atributos y datos mínimos.
- [Software inmobiliario Wasi](https://wasi.co/softwareinmobiliario/): CRM, web inmobiliaria, red de aliados, portalización, móvil y academia.
